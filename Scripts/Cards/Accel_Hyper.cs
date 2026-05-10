using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
[RegisterCharacterStarterCard(typeof(Suguri46bCharacter), 1)]
public class  Accel_Hyper: ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("doubleDamageAmount", 1)
    ];

    public Accel_Hyper() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<Accel_HyperPower>(choiceContext, base.Owner.Creature, base.DynamicVars["doubleDamageAmount"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
       base.DynamicVars["doubleDamageAmount"].UpgradeValueBy(1);
    }
}