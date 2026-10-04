using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using Suguri46b.Scripts.Enchantments;

namespace Suguri46b.Scripts.Extensions;

/// <summary>
///     「混成化」（Mix 附魔）的变化链：
///       1. 打出时 → 变化为随机牌，并为变化后的牌重新附魔 Mix（延迟到牌堆变更后执行，避免打断出牌流程）。
///       2. 回合结束被保留时（Mix 附魔自带[保留]）→ 同样变化为随机牌并重新附魔 Mix。
///     由于新牌同样带 Mix，会被保留并在下个回合继续变化，形成持续的变化链。
/// </summary>
[RegisterSingleton]
public class TransSelf : HookedSingletonModel
{
    public TransSelf() : base(HookType.Combat)
    {
    }

    // 待变化的卡 → (目标卡, 是否续附混成化)，支持同时多张（static：卡牌与单例共用同一队列）
    private static readonly Dictionary<CardModel, (CardModel Replacement, bool EnchantWithMix)> PendingTransforms = [];

    public override Task BeforeCombatStart()
    {
        PendingTransforms.Clear();
        return base.BeforeCombatStart();
    }

    /// <summary>
    ///     登记一次"延迟变化"：指定的牌在出牌结算结束（牌堆变更）后变化为 replacement。
    ///     供其它卡牌复用（例如部件扩张把自己变化为卡组中某张牌的复制品）。
    /// </summary>
    /// <param name="card">被变化的牌。</param>
    /// <param name="replacement">变化结果（会作为替换牌直接使用，务必传入新建的实例/克隆）。</param>
    /// <param name="enchantWithMix">变化结果是否再附上[混成化]（混成化链专用）。</param>
    public static void QueueTransform(CardModel card, CardModel replacement, bool enchantWithMix = false)
    {
        PendingTransforms[card] = (replacement, enchantWithMix);
    }

    // ---------- 路径 1：打出时（延迟到牌堆变更后执行） ----------

    public override Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        if (!IsMixEnchanted(card))
        {
            return Task.CompletedTask;
        }
        var newcard = PickRandomReplacement(card);
        if (newcard != null)
        {
            QueueTransform(card, newcard, enchantWithMix: true);
        }
        return Task.CompletedTask;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (PendingTransforms.Remove(card, out var pending))
        {
            var result = await CardCmd.Transform(card, pending.Replacement);
            if (result.HasValue && pending.EnchantWithMix)
                CardCmd.Enchant<Mix>(result.Value.cardAdded, 1);
        }
    }

    // ---------- 路径 2：回合结束被保留时 ----------

    public override async Task AfterFlush(
        PlayerChoiceContext choiceContext,
        Player player,
        IReadOnlyCollection<CardModel> flushedCards,
        IReadOnlyCollection<CardModel> retainedCards)
    {
        foreach (CardModel card in retainedCards.ToList())
        {
            if (!IsMixEnchanted(card))
            {
                continue;
            }
            await TransformAndReenchant(card);
        }
    }

    // ---------- 公共逻辑 ----------

    private static bool IsMixEnchanted(CardModel card)
    {
        return card.Enchantment != null
            && card.Enchantment.GetType() == ModelDb.Enchantment<Mix>().GetType();
    }

    /// <summary>从角色卡池中随机取一张可替换的牌（攻击/技能/能力）。</summary>
    private static CardModel? PickRandomReplacement(CardModel card)
    {
        var owner = card.Owner;
        if (owner == null)
        {
            return null;
        }
        List<CardPoolModel> allPools = [.. owner.UnlockState.CharacterCardPools];
        IEnumerable<CardModel> allCards = allPools
            .SelectMany(pool => pool.GetUnlockedCards(
                owner.UnlockState,
                owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Type == CardType.Attack || c.Type == CardType.Skill || c.Type == CardType.Power);

        return CardFactory.GetDistinctForCombat(
            owner,
            allCards,
            1,
            owner.RunState.Rng.CombatCardGeneration
        ).FirstOrDefault();
    }

    /// <summary>立即变化为随机牌，并为变化后的牌重新附魔 Mix。</summary>
    private static async Task TransformAndReenchant(CardModel card)
    {
        CardModel? target = PickRandomReplacement(card);
        if (target == null)
        {
            return;
        }
        var result = await CardCmd.Transform(card, target);
        if (result.HasValue)
        {
            CardCmd.Enchant<Mix>(result.Value.cardAdded, 1);
        }
    }
}
