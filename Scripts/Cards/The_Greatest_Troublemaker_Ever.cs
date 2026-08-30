using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class The_Greatest_Troublemaker_Ever : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.RandomEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public The_Greatest_Troublemaker_Ever() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Repeat];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new CardsVar(1),
        new RepeatVar(1),
        new DynamicVar("ExtraRepeat",1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RepeatCount").WithMultiplier((CardModel card, Creature? _) => RepeatCount.ThisCardRepeatCount(card))
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int repeatcount = RepeatCount.ThisCardRepeatCount(cardPlay.Card);
        int attacksandstatusCount = ((int)((CalculatedVar)base.DynamicVars["RepeatCount"]).Calculate(cardPlay.Target)/2+DynamicVars.Repeat.IntValue)*base.DynamicVars["ExtraRepeat"].IntValue;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(attacksandstatusCount)
            .FromCard(this,cardPlay)
            .TargetingRandomOpponents(base.CombatState)
            .Execute(choiceContext);
        await CardPileCmd.Draw(choiceContext,repeatcount/3*DynamicVars.Cards.IntValue, cardPlay.Card.Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
