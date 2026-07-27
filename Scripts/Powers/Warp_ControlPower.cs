using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Rewards;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Warp_ControlPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    public override Task AfterCombatEnd(CombatRoom room)
    {
        var reward = new WarpControlReward(base.Owner.Player, WarpControlReward.RegisteredRewardType);
        room.AddExtraReward(base.Owner.Player, reward);
        return Task.CompletedTask;
    }
}
