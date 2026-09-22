using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Extreme_AlterationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new BlockVar(4, ValueProp.Unpowered),
    ];

    /// <summary>
    ///     「遗忘」结算入口：一张牌因[遗忘]关键词改为进入消耗堆时调用。
    ///     若该牌的拥有者拥有魔改能力，则获得格挡（4×层数）。
    /// </summary>
    public static async Task OnCardForgotten(CardModel? card)
    {
        var creature = card?.Owner?.Creature;
        if (creature == null)
        {
            return;
        }
        var power = creature.GetPower<Extreme_AlterationPower>();
        if (power == null || power.Amount <= 0m)
        {
            return;
        }
        decimal block = power.DynamicVars.Block.IntValue * power.Amount;
        power.Flash();
        await CreatureCmd.GainBlock(creature, block, ValueProp.Unpowered, null);
    }
}
