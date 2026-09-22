using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Powers;

namespace Suguri46b.Scripts.Extensions;

/// <summary>
/// 「遗忘」关键词效果：此牌将要进入弃牌堆时，改为进入消耗牌堆。
/// 按路径分别处理，目标是"不进入弃牌堆、不播弃牌动画"：
///   1. 被打出时 → <see cref="ModifyCardPlayResultLocation"/>：落点直接改写为消耗堆。
///   2. 回合结束弃置整手牌时 → <see cref="BeforeFlush"/>：在批量弃牌前先自行消耗这些牌。
///   3. 效果弃牌（单卡）→ ForgetDiscardRedirectPatch：改写 CardPileCmd.Add 的目标牌堆。
///   4. 兜底与结算 → <see cref="AfterCardChangedPiles"/>：仍落入弃牌堆的立即改送消耗堆。
/// 结算规则：只有"因弃置而触发遗忘改道"的牌才被标记（<see cref="MarkForgotten"/>），
/// 只有被标记的牌真正进入消耗堆时才结算魔改的格挡；
/// 被其它效果直接消耗（星爆之光、消耗关键词等）的遗忘牌不结算。
/// </summary>
[RegisterSingleton]
public class ForgetKeywordHandler : HookedSingletonModel
{
    public ForgetKeywordHandler() : base(HookType.Combat)
    {
    }

    // 防止"消耗"自身再触发本钩子造成递归
    private readonly HashSet<CardModel> _moving = [];

    // 因"弃置改道"而被遗忘的牌（static：补丁与单例共用同一标记集合）
    private static readonly HashSet<CardModel> Forgotten = [];

    /// <summary>标记一张牌为"因弃置而遗忘"（由其落点被改写到消耗堆时调用）。</summary>
    public static void MarkForgotten(CardModel card)
    {
        Forgotten.Add(card);
    }

    public override Task BeforeCombatStart()
    {
        Forgotten.Clear();
        _moving.Clear();
        return base.BeforeCombatStart();
    }

    // 路径 1：打出「遗忘」牌时，落点由弃牌堆改写为消耗堆
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
    {
        if (cardLocation.pileType == PileType.Discard && card.HasModKeyword(MyKeywords.Forget))
        {
            MarkForgotten(card);
            return cardLocation with { pileType = PileType.Exhaust };
        }
        return cardLocation;
    }

    // 路径 2：回合结束弃置整手牌之前，先消耗掉「遗忘」牌（不进入弃牌堆，无弃牌动画，正常触发消耗事件）
    public override async Task BeforeFlush(PlayerChoiceContext choiceContext, Player player)
    {
        var combatState = player.Creature?.CombatState;
        if (combatState == null)
        {
            return;
        }
        // 与 FlushPlayerHand 的判定保持一致：ShouldFlush 为假则整手保留
        if (!Hook.ShouldFlush(combatState, player))
        {
            return;
        }
        CardPile hand = PileType.Hand.GetPile(player);
        List<CardModel> toExhaust = hand.Cards
            .Where(c => c.HasModKeyword(MyKeywords.Forget) && !c.ShouldRetainThisTurn)
            .ToList();
        foreach (CardModel card in toExhaust)
        {
            // 这些牌本来会被弃置，属于"因弃置而遗忘"
            MarkForgotten(card);
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    // 路径 4：兜底改道 + 结算
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (card == null || !card.HasModKeyword(MyKeywords.Forget))
        {
            return;
        }

        // 进入消耗堆：仅当它是"因弃置改道"被标记的牌时才结算（被其它效果直接消耗的不结算）
        if (card.Pile?.Type == PileType.Exhaust)
        {
            if (oldPileType != PileType.Exhaust && Forgotten.Remove(card))
            {
                await Extreme_AlterationPower.OnCardForgotten(card);
            }
            return;
        }

        // 落到其它牌堆（含落点被别的模型改写的情况）：撤销标记，避免误结算
        Forgotten.Remove(card);

        // 仍落入弃牌堆：改送消耗堆（改送后会再次进入本钩子，由上面的分支结算）
        if (card.Pile?.Type != PileType.Discard)
        {
            return;
        }
        // 新生成的牌（原本不在任何牌堆）不算被弃置
        if (oldPileType == PileType.None)
        {
            return;
        }
        if (!_moving.Add(card))
        {
            return;
        }
        try
        {
            MarkForgotten(card);
            await CardCmd.Exhaust(new BlockingPlayerChoiceContext(), card);
        }
        finally
        {
            _moving.Remove(card);
        }
    }
}
