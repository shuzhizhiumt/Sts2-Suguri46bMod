using System.Text;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Networking.ManagedActions;
using Suguri46b.Scripts.Cards.Token;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.GameActions;

/// <summary>
///     管理 Affluence 右键升级的网络同步 Action。
///     不走 RightClick 的 ExecuteManaged（它有 OwnerNetId 过滤），
///     而是通过 RitsuLibManagedNetActionDescriptor 直接入队，
///     利用 ActionQueue 在所有客户端上同步执行状态修改。
/// </summary>
public static class AffluenceUpgradeAction
{
    private const string ActionKey = "affluence_right_click_upgrade";

    public static RitsuLibManagedNetActionDescriptor<AffluenceUpgradePayload> Descriptor { get; private set; } = null!;

    public static void Register()
    {
        Descriptor = new RitsuLibManagedNetActionDescriptor<AffluenceUpgradePayload>(
            ModuleId: Entry.ModId,
            ActionKey: ActionKey,
            Serialize: SerializePayload,
            Deserialize: DeserializePayload,
            Execute: ExecutePayload,
            ActionType: GameActionType.CombatPlayPhaseOnly);
        RitsuLibManagedNetActions.Register(Descriptor);
    }

    // ===========================
    // Payload 序列化
    // ===========================

    private static byte[] SerializePayload(AffluenceUpgradePayload payload)
    {
        var cardIdBytes = Encoding.UTF8.GetBytes(payload.CardEntryId);
        var buffer = new byte[
            8                           // OwnerNetId (ulong)
            + 4                         // CardEntryId length (int)
            + cardIdBytes.Length        // CardEntryId (UTF-8)
            + 4                         // Cost (int)
        ];
        var pos = 0;
        WriteULong(buffer, ref pos, payload.OwnerNetId);
        WriteString(buffer, ref pos, payload.CardEntryId, cardIdBytes);
        WriteInt(buffer, ref pos, payload.Cost);
        return buffer;
    }

    private static AffluenceUpgradePayload DeserializePayload(ReadOnlySpan<byte> bytes)
    {
        var pos = 0;
        var ownerNetId = ReadULong(bytes, ref pos);
        var cardEntryId = ReadString(bytes, ref pos);
        var cost = ReadInt(bytes, ref pos);
        return new AffluenceUpgradePayload(ownerNetId, cardEntryId, cost);
    }

    // ===========================
    // 同步执行（在所有客户端上运行）
    // ===========================

    private static async Task ExecutePayload(RitsuLibManagedNetActionContext<AffluenceUpgradePayload> ctx)
    {
        var payload = ctx.Message;
        var player = RunManager.Instance.DebugOnlyGetState()
            ?.Players
            .FirstOrDefault(p => p.NetId == payload.OwnerNetId);

        if (player == null)
            return;

        // 在手牌中查找对应的 Affluence 卡牌
        var handPile = PileType.Hand.GetPile(player);
        var card = handPile?.Cards
            .OfType<Affluence>()
            .FirstOrDefault(c =>
                c.Id.Entry == payload.CardEntryId
                && c.CurrentUpgradeLevel < c.MaxUpgradeLevel);

        if (card == null)
            return;

        // 执行期守卫：再次验证资源是否足够
        if (SecondaryResourceCmd.Get(player, ModResources.OJStarId) < payload.Cost)
            return;
        if (player.PlayerCombatState.Energy < 1)
            return;

        // 执行实际的资源消耗和升级（现在所有客户端都执行，状态保持同步）
        await SecondaryResourceCmd.Lose(player, ModResources.OJStarId, payload.Cost);
        player.PlayerCombatState.LoseEnergy(1);
        CardCmd.Upgrade(card);
    }

    // ===========================
    // 简易二进制读写（避免依赖 internal API）
    // ===========================

    private static void WriteULong(byte[] buffer, ref int pos, ulong value)
    {
        buffer[pos++] = (byte)(value & 0xFF);
        buffer[pos++] = (byte)((value >> 8) & 0xFF);
        buffer[pos++] = (byte)((value >> 16) & 0xFF);
        buffer[pos++] = (byte)((value >> 24) & 0xFF);
        buffer[pos++] = (byte)((value >> 32) & 0xFF);
        buffer[pos++] = (byte)((value >> 40) & 0xFF);
        buffer[pos++] = (byte)((value >> 48) & 0xFF);
        buffer[pos++] = (byte)((value >> 56) & 0xFF);
    }

    private static ulong ReadULong(ReadOnlySpan<byte> bytes, ref int pos)
    {
        var value = BitConverter.ToUInt64(bytes.Slice(pos, 8));
        pos += 8;
        return value;
    }

    private static void WriteInt(byte[] buffer, ref int pos, int value)
    {
        BitConverter.TryWriteBytes(buffer.AsSpan(pos, 4), value);
        pos += 4;
    }

    private static int ReadInt(ReadOnlySpan<byte> bytes, ref int pos)
    {
        var value = BitConverter.ToInt32(bytes.Slice(pos, 4));
        pos += 4;
        return value;
    }

    private static void WriteString(byte[] buffer, ref int pos, string value, byte[] utf8Bytes)
    {
        WriteInt(buffer, ref pos, utf8Bytes.Length);
        Array.Copy(utf8Bytes, 0, buffer, pos, utf8Bytes.Length);
        pos += utf8Bytes.Length;
    }

    private static string ReadString(ReadOnlySpan<byte> bytes, ref int pos)
    {
        var length = ReadInt(bytes, ref pos);
        var str = Encoding.UTF8.GetString(bytes.Slice(pos, length));
        pos += length;
        return str;
    }
}

/// <summary>
///     Affluence 右键升级的网络同步载荷。
/// </summary>
public readonly record struct AffluenceUpgradePayload(
    ulong OwnerNetId,
    string CardEntryId,
    int Cost);
