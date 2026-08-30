using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Dangerous_PuddingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    // 直到下回合结束前，不能获得能量（与原版 NoEnergyGainPower 相同的封锁方式）
    public override decimal ModifyEnergyGain(Player player, decimal amount)
    {
        return player != base.Owner.Player ? amount : 0m;
    }

    public override Task AfterModifyingEnergyGain()
    {
        Flash();
        return Task.CompletedTask;
    }

    // 保留能量到下回合（与原版 IceCream 遗物相同的保留机制：
    // 返回 false 时回合开始不重置能量，改为在现有能量上叠加能量上限）
    public override bool ShouldPlayerResetEnergy(Player player)
    {
        return player != base.Owner.Player;
    }

    // 层数表示剩余持续的玩家侧回合数：打出当回合结束时减1，下回合结束时减1，
    // 减到0时由 PowerCmd 自动移除（对应"直到下回合结束前"）
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side,
        IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
            await PowerCmd.Decrement(this);
    }
}
