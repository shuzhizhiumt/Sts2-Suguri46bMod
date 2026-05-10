using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class AcceleratorPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/powers/AcceleratorPower.png",
        BigIconPath: "res://Suguri46b/images/powers/AcceleratorPower.png"
    );

	public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
	{
		if (dealer == null)
		{
			return 1;
		}
		if (dealer != base.Owner && !base.Owner.Pets.Contains<Creature>(dealer))
		{
			return 1;
		}
		if (!props.IsPoweredAttack())
		{
			return 1;
		}
		if (cardSource == null)
		{
			return 1;
		}
		return 2;
	}
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (base.Owner != target)
        {
            return 1;
        }
        return (decimal)Math.Pow(2.0, base.Amount);
    }
     public override async Task AfterTurnEnd(PlayerChoiceContext choiceContext, CombatSide side)
	{
		if (side == base.Owner.Side)
		{
			await PowerCmd.Decrement(this);
		}
	}
}