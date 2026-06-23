using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;
using Suguri46b.Scripts.Extensions;
using Godot;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.CardTargeting;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Combat.SecondaryResources;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.CardKeyWords;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Little_War: ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Little_War() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }    
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new RepeatVar(2),
        new ExtraDamageVar(2),
        new DynamicVar("ExtraRepeat",1),
        ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {

        int repeat=base.DynamicVars.Repeat.IntValue;
        int repeatcount=RepeatCount.ThisCardRepeatCount(cardPlay.Card);
        if (repeatcount>=2)
        {
            repeat+=base.DynamicVars["ExtraRepeat"].IntValue;
        }
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .WithHitCount(repeat)
            .FromCard(this)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        int? repeatcount=RepeatCount.ThisCardRepeatCount(cardSource);
        if (cardSource==this  && repeatcount>=1 && dealer==Owner.Creature)
        {
            return base.DynamicVars.ExtraDamage.IntValue;
        }
        return 0;
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
    }
}