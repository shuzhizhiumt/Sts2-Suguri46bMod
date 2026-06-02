using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Deceptive_Disarming : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(13, ValueProp.Move),
        new PowerVar<WeakPower>(2),
        new DynamicVar("Additional_Payment",10)
    ];
    public Deceptive_Disarming() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    private int ojstartotal;
    protected override bool ShouldGlowGoldInternal => Owner.PlayerCombatState.GetOJStarTotal() >= base.DynamicVars["Additional_Payment"].BaseValue;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
        if (cardPlay.Target.Monster.IntendsToAttack)
		{
			await PowerCmd.Apply<WeakPower>(choiceContext, cardPlay.Target, base.DynamicVars.Weak.BaseValue, base.Owner.Creature, this);
		}
        ojstartotal=Owner.PlayerCombatState.GetOJStarTotal();
        if (ojstartotal >= base.DynamicVars["Additional_Payment"].BaseValue && cardPlay.Target.Monster.IntendsToAttack)
        {
            await PowerCmd.Apply<ReflectPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
            await PlayerCmdExtensions.LoseOJStar(base.DynamicVars["Additional_Payment"].BaseValue,Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }
}