using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Networking.ManagedActions;
using Suguri46b.Scripts.Enchantments;
using Suguri46b.Scripts.Powers;

namespace Suguri46b.Scripts.GameActions;

/// <summary>
///     Dmgx2 附魔 ManagedNetAction（支持手牌选牌 + 多人同步）。
///     点击按钮 → Request() 入队 → ExecutePayload 在所有客户端运行：
///         - 本地玩家弹出手牌选择 UI（CardSelectCmd.FromHand 自带多人同步）
///         - 选中后对所有客户端统一附魔
/// </summary>
public static class Dmgx2EnchantManagedAction
{
    private const string ActionKey = "dmgx2_enchant";

    public static RitsuLibManagedNetActionDescriptor<Dmgx2EnchantPayload> Descriptor { get; private set; } = null!;

    public static void Register()
    {
        Descriptor = new RitsuLibManagedNetActionDescriptor<Dmgx2EnchantPayload>(
            ModuleId: Entry.ModId,
            ActionKey: ActionKey,
            Serialize: SerializePayload,
            Deserialize: DeserializePayload,
            Execute: ExecutePayload,
            ActionType: GameActionType.CombatPlayPhaseOnly);
        RitsuLibManagedNetActions.Register(Descriptor);
    }

    /// <summary>
    ///     从按钮回调直接调用：入队一个 ManagedNetAction。
    /// </summary>
    public static bool Request(Player player)
    {
        var payload = new Dmgx2EnchantPayload(player.NetId);
        return RitsuLibManagedNetActions.Request(
            RunManager.Instance, Descriptor, payload, player.NetId);
    }

    // ===========================
    // Payload 序列化
    // ===========================

    private static byte[] SerializePayload(Dmgx2EnchantPayload payload)
    {
        return System.BitConverter.GetBytes(payload.OwnerNetId);
    }

    private static Dmgx2EnchantPayload DeserializePayload(ReadOnlySpan<byte> bytes)
    {
        return new Dmgx2EnchantPayload(System.BitConverter.ToUInt64(bytes));
    }

    // ===========================
    // 同步执行（所有客户端统一运行）
    // ===========================

    private static async Task ExecutePayload(RitsuLibManagedNetActionContext<Dmgx2EnchantPayload> ctx)
    {
        var player = RunManager.Instance.DebugOnlyGetState()
            ?.Players.FirstOrDefault(p => p.NetId == ctx.Message.OwnerNetId);
        if (player == null) return;

        // 来源：DoublePower
        var source = player.Creature.GetPower<DoublePower>();
        if (source == null) return;

        // 手牌中可附魔的攻击牌（无附魔）
        var hand = PileType.Hand.GetPile(player);
        if (hand == null || hand.Cards.Count == 0) return;

        var candidates = hand.Cards
            .Where(c => c.Type == CardType.Attack && c.Enchantment == null)
            .ToList();

        if (candidates.Count == 0) return;

        // 弹出手牌选择 UI（CardSelectCmd.FromHand 内部处理多人同步）
        var prefs = new CardSelectorPrefs(
            CardSelectorPrefs.EnchantSelectionPrompt,
            minCount: 1,
            maxCount: candidates.Count);

        var selected = (await CardSelectCmd.FromHand(
            ctx.PlayerChoiceContext,
            player,
            prefs,
            c => c.Type == CardType.Attack && c.Enchantment == null,
            source)).ToList();

        if (selected.Count == 0) return;

        // 所有客户端统一执行附魔
        foreach (var card in selected)
        {
            var ench = ModelDb.Enchantment<Dmgx2Enchantment>().ToMutable();
            CardCmd.Enchant(ench, card, 1);
        }
    }
}

public readonly record struct Dmgx2EnchantPayload(ulong OwnerNetId);
