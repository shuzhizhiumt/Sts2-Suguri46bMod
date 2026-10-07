using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Cards.Token;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Norma : ModPowerTemplate,ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );
    public bool Norma2;
    public bool Norma3;
    public bool Norma4;
    public bool Norma5;
    public bool Norma6;


    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || Owner == null || Owner.Player == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }
        if (!Norma2 && Amount>=2)
        {
            Norma2=true;
            Flash();
            await PowerCmd.Apply<StrengthPower>(choiceContext,base.Owner, 1, base.Owner,cardSource);
        }
        if (!Norma3 && Amount>=3)
        {
            Norma3=true;
            Flash();
        }
        if (!Norma4 && Amount>=4)
        {
            Norma4=true;
            Flash();
            await PowerCmd.Apply<Mori_no_MajoPower>(choiceContext, base.Owner, 1, base.Owner, cardSource);
        }
        if (!Norma5 && Amount>=5)
        {
            Norma5=true;
            Flash();
        }
        if (!Norma6 && Amount>=6)
        {
            Norma6=true;
            Flash();
        }
    }

    // ---------- 3 层：每回合开始时，可选择至多 2 张手牌使其本回合[保留] ----------

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (!Norma3 || Owner.Player == null || player != Owner.Player)
        {
            return;
        }
        CardPile hand = PileType.Hand.GetPile(player);
        // 已经会保留的牌（天生带保留 / 已被赋予本回合保留）不必再选
        List<CardModel> candidates = hand.Cards.Where(c => !c.ShouldRetainThisTurn).ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        Flash();
        // source 传 null：能力不是"正在执行的模型"，否则选牌托盘清理会挂到永不触发的事件上
        IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(
                new LocString("card_selection", "NORMA3_RETAIN"),
                0,
                Math.Min(2, candidates.Count)),
            card => !card.ShouldRetainThisTurn,
            null!);

        foreach (CardModel card in selected.ToList())
        {
            CardCmd.ApplySingleTurnRetain(card);
        }
    }

    // ---------- 6 层：一张牌被遗忘时，触发一次它的打出效果 ----------

    // 防递归：正在触发中的（原）卡牌
    private static readonly HashSet<CardModel> TriggeringForgotten = [];

    /// <summary>
    ///     诺玛 6 层：非打出的牌（从手牌被弃置/被效果弃置）被遗忘时，自动打出一次。
    ///     调用方（ForgetKeywordHandler）已排除"打出的牌"（oldPileType == Play），因为其效果已经结算过。
    ///     做法是"复制一份→剥离遗忘、改为消耗→自动打出"：
    ///     剥离遗忘可避免"遗忘→触发→再遗忘"的连锁，改为消耗则该复制品打完即离场，不会污染牌堆。
    /// </summary>
    public static async Task TriggerForgottenCardEffect(PlayerChoiceContext choiceContext, CardModel? card)
    {
        if (card == null || card.CombatState == null || card.Owner == null)
        {
            return;
        }
        var creature = card.Owner.Creature;
        if (creature == null)
        {
            return;
        }
        Norma? norma = creature.GetPower<Norma>();
        if (norma == null || !norma.Norma6)
        {
            return;
        }
        if (!TriggeringForgotten.Add(card))
        {
            return;
        }
        try
        {
            norma.Flash();
            CardModel clone = card.CreateClone();
            clone.RemoveKeyword(MyKeywords.Forget);
            clone.AddKeyword(CardKeyword.Exhaust);
            await CardPileCmd.AddGeneratedCardToCombat(clone, PileType.Hand, card.Owner);
            await CardCmd.AutoPlay(choiceContext, clone, null);
        }
        finally
        {
            TriggeringForgotten.Remove(card);
        }
    }
}