using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Intelligence_Officer : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Intelligence_Officer() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            yield return HoverTipFactory.FromPower<StrengthPower>();
            yield return HoverTipFactory.FromPower<DexterityPower>();
            yield return HoverTipFactory.FromPower<PlatingPower>();

            // 此牌在手牌中时，悬停可看到抽牌堆顶的牌（下一张会抽到的牌）。
            // 多张情报官在手也只会显示同一张顶牌（顶牌只有一张）。
            CardModel? topCard = GetDrawPileTop();
            if (topCard != null)
            {
                yield return HoverTipFactory.FromCard(topCard);
            }
        }
    }

    /// <summary>抽牌堆顶（下一张会抽到的牌）；不在战斗中或抽牌堆为空时返回 null。</summary>
    private CardModel? GetDrawPileTop()
    {
        if (Owner == null || Owner.Creature?.CombatState == null)
        {
            return null;
        }
        CardPile? drawPile = PileType.Draw.GetPile(Owner);
        if (drawPile == null || drawPile.Cards.Count == 0)
        {
            return null;
        }
        return drawPile.Cards.First();
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Intelligence", 3),
        new CardsVar(2)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile drawPile = PileType.Draw.GetPile(base.Owner);
        if (drawPile.Cards.Count == 0)
            return;
        CardModel topCard = drawPile.Cards.First();
        if(topCard == null)
        {
            return;
        }
        switch (topCard.Type)
        {
            case CardType.Attack:
                await PowerCmd.Apply<ATK_UP>(choiceContext, base.Owner.Creature, base.DynamicVars["Intelligence"].BaseValue, base.Owner.Creature, this);
                break;
            case CardType.Skill:
                await PowerCmd.Apply<DEF_UP>(choiceContext, base.Owner.Creature, base.DynamicVars["Intelligence"].BaseValue, base.Owner.Creature, this);
                break;
            case CardType.Power:
                await PowerCmd.Apply<PlatingPower>(choiceContext, base.Owner.Creature, base.DynamicVars["Intelligence"].BaseValue, base.Owner.Creature, this);
                break;
            default:
                await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.IntValue,Owner);
                break;
        }
    }

    protected override void OnUpgrade()
    {
        this.AddKeyword(CardKeyword.Retain);
    }
}
