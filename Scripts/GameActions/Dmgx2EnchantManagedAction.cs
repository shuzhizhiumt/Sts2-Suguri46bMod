using System;
using System.Collections.Generic;
using System.Linq;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Ui.Toast;
using Suguri46b.Scripts.Enchantments;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.GameActions;

/// <summary>
///     Dmgx2 附魔 ManagedNetAction（支持手牌选牌 + 多人同步）。
///     点击按钮 → Request() 入队 → ExecutePayload 在所有客户端运行：
///         - 本地玩家弹出手牌选择 UI（CardSelectCmd.FromHand 自带多人同步）
///         - 选中的牌按 耗能×5+10 消耗 OJStar；星星不足则不附魔并退出选择
///         - 选中后对所有客户端统一附魔（X 费牌不可选）
/// </summary>
public static class Dmgx2EnchantManagedAction
{
    private const string ActionKey = "dmgx2_enchant";

    /// <summary>每 1 点耗能需要的星星数。</summary>
    private const int StarCostPerEnergy = 5;

    /// <summary>固定的额外星星消耗。</summary>
    private const int BaseStarCost = 10;

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

    /// <summary>
    ///     这张牌附魔需要消耗的星星数：耗能×5+10（X 费牌不可附魔，耗能按当前结算值取非负）。
    /// </summary>
    public static int GetEnchantStarCost(CardModel card)
    {
        int energy = Math.Max(0, card.EnergyCost.GetResolved());
        return energy * StarCostPerEnergy + BaseStarCost;
    }

    /// <summary>
    ///     可附魔的手牌：攻击牌、无附魔、且不是 X 费。
    /// </summary>
    public static bool IsValidCandidate(CardModel card)
    {
        return card.Type == CardType.Attack
            && card.Enchantment == null
            && !card.EnergyCost.CostsX;
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

        // 来源：Mori_no_MajoPower
        var source = player.Creature.GetPower<Mori_no_MajoPower>();
        if (source == null) return;

        var hand = PileType.Hand.GetPile(player);
        if (hand == null || hand.Cards.Count == 0) return;

        var candidates = hand.Cards.Where(IsValidCandidate).ToList();
        if (candidates.Count == 0) return;

        // 弹出手牌选择 UI（CardSelectCmd.FromHand 内部处理多人同步）
        // minCount: 0 —— 允许不选任何牌直接确认（取消此次附魔）
        var prefs = new CardSelectorPrefs(
            new LocString("card_selection", "DMGX2_ENCHANT_COST"),
            minCount: 0,
            maxCount: candidates.Count);

        // 星星不足时不退出选择（退出会让已选牌卡在选牌界面），
        // 而是提示后重新打开选牌界面，让玩家少选几张或直接取消。
        //
        // source 传 null 的原因：NPlayerHand.AfterCardsSelected 把"选牌托盘"的清理
        // （OnSelectModeSourceFinished）挂到 source.ExecutionFinished 上。常驻 Power 永远不会
        // 执行完成，托盘里的牌就会一直卡在选牌位置；传 null 时清理立即执行，牌回手牌区。
        while (true)
        {
            var selected = (await CardSelectCmd.FromHand(
                ctx.PlayerChoiceContext,
                player,
                prefs,
                IsValidCandidate,
                null!)).ToList();

            // 玩家未选牌（取消）：正常退出
            if (selected.Count == 0)
            {
                return;
            }

            // 费用 = Σ(耗能×5+10)
            int requiredStars = selected.Sum(GetEnchantStarCost);
            int currentStars = SecondaryResourceCmd.Get(player, ModResources.OJStarId);
            if (currentStars >= requiredStars)
            {
                await SecondaryResourceCmd.Lose(player, ModResources.OJStarId, requiredStars);

                // 所有客户端统一执行附魔
                foreach (var card in selected)
                {
                    var ench = ModelDb.Enchantment<Dmgx2Enchantment>().ToMutable();
                    CardCmd.Enchant(ench, card, 1);
                }
                return;
            }

            if (LocalContext.IsMe(player))
            {
                RitsuToastService.ShowWarning(
                    $"星星不足：需要 {requiredStars}，当前 {currentStars}。可少选几张或取消。",
                    "翻倍附魔");
            }
        }
    }
}

public readonly record struct Dmgx2EnchantPayload(ulong OwnerNetId);
