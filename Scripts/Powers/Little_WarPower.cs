using System.Collections.Generic;
using System.Threading.Tasks;
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
public class Little_WarPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    // 攻击牌对有此 Debuff 的敌人伤害减半
    public override decimal ModifyDamageMultiplicative(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != base.Owner || cardSource == null || cardSource.Type != CardType.Attack)
            return 1;
        return 0.5m;
    }
    	public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
	{
		if (card.Type != CardType.Attack)
		{
			return playCount;
		}

		if (card.TargetType == TargetType.AnyEnemy && target != base.Owner)
		{
			return playCount;
		}
		TargetType targetType = card.TargetType;
		if ((uint)(targetType - 2) > 1u)
		{
			return playCount;
		}
		return playCount + 1;
	}

	public override async Task AfterModifyingCardPlayCount(CardModel card)
	{
		await PowerCmd.Decrement(this);
	}
}
