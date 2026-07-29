using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Lucky_Charm : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );

    public Lucky_Charm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {

    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Norma>()
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GainOJStar", 3),
        new DynamicVar("ReduceMaxHandSize", 2)
    ];
    private bool IsDrawn=false;
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        int Level = Owner?.Creature.GetPower<Norma>()?.Amount ?? 0;
        bool InHand=false;
        foreach (var item in PileType.Hand.GetPile(this.Owner).Cards)
        {
            if (item==this)
            {
                InHand=true;
                break;
            }
        }
        if (participants.Contains(base.Owner.Creature) && InHand)
        {
            await SecondaryResourceCmd.Gain(Owner, ModResources.OJStarId,Level*base.DynamicVars["GainOJStar"].IntValue);
        }
    }
    public override async Task AfterCardDrawn(PlayerChoiceContext choiceContext, CardModel card, bool fromHandDraw)
    {
        if (card==this && !IsDrawn)
        {
            await PowerCmd.Apply<MaxHandSize>(choiceContext, base.Owner.Creature, -base.DynamicVars["ReduceMaxHandSize"].IntValue, base.Owner.Creature, this);
            IsDrawn=true;
        }
    }
    protected override void OnUpgrade()
    {
        base.DynamicVars["GainOJStar"].UpgradeValueBy(1);
        base.DynamicVars["ReduceMaxHandSize"].UpgradeValueBy(-1);
    }
}
