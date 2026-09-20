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

namespace Suguri46b.Scripts.Extensions;

/// <summary>
/// 「遗忘」关键词效果：此牌将要进入弃牌堆时，改为进入消耗牌堆。
/// 按路径分别处理，目标是"不进入弃牌堆、不播弃牌动画"：
///   1. 被打出时 → <see cref="ModifyCardPlayResultLocation"/>：落点直接改写为消耗堆。
///   2. 回合结束弃置整手牌时 → <see cref="BeforeFlush"/>：在批量弃牌前先自行消耗这些牌。
///   3. 效果弃牌（单卡）→ ForgetDiscardRedirectPatch：改写 CardPileCmd.Add 的目标牌堆。
///   4. 兜底 → <see cref="AfterCardChangedPiles"/>：任何仍落入弃牌堆的情况立即改送消耗堆。
/// </summary>
[RegisterSingleton]
public class ForgetKeywordHandler : HookedSingletonModel
{
    public ForgetKeywordHandler() : base(HookType.Combat)
    {
    }

    // 防止"消耗"自身再触发本钩子造成递归
    private readonly HashSet<CardModel> _moving = [];

    // 路径 1：打出「遗忘」牌时，落点由弃牌堆改写为消耗堆
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
    {
        if (cardLocation.pileType == PileType.Discard && card.HasModKeyword(MyKeywords.Forget))
        {
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
            await CardCmd.Exhaust(choiceContext, card);
        }
    }

    // 路径 4：兜底——任何仍进入弃牌堆的情况立即改送消耗堆
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        // 只在"进入弃牌堆"时生效
        if (card?.Pile?.Type != PileType.Discard)
        {
            return;
        }
        // 新生成的牌（原本不在任何牌堆）不算被弃置
        if (oldPileType == PileType.None)
        {
            return;
        }
        if (!card.HasModKeyword(MyKeywords.Forget))
        {
            return;
        }
        if (!_moving.Add(card))
        {
            return;
        }
        try
        {
            await CardCmd.Exhaust(new BlockingPlayerChoiceContext(), card);
        }
        finally
        {
            _moving.Remove(card);
        }
    }
}
