using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Extended_Photon_Rifle : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Extended_Photon_Rifle() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Repeat];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(2),
        new ExtraDamageVar(1),
        new DynamicVar("ExtraRepeat",1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RepeatCount").WithMultiplier((CardModel card, Creature? _) => RepeatCount.ThisCardRepeatCount(card))
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int attacksandstatusCount = ((int)((CalculatedVar)base.DynamicVars["RepeatCount"]).Calculate(cardPlay.Target)/3+DynamicVars.Repeat.IntValue)*base.DynamicVars["ExtraRepeat"].IntValue;
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(attacksandstatusCount)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource,CardPlay? cardPlay)
    {
        int? repeatcount=RepeatCount.ThisCardRepeatCount(cardSource);
        if (cardSource==this  && repeatcount>=1 && dealer==Owner.Creature)
        {
            return (decimal)(base.DynamicVars.ExtraDamage.IntValue*repeatcount);
        }
        return 0;
    }
    protected override void OnUpgrade()
    {
        DynamicVars.ExtraDamage.UpgradeValueBy(1);
    }
}
