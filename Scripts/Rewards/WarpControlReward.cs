using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Rewards;
using STS2RitsuLib.Combat.Rewards;
using Suguri46b.Scripts.GameActions;

namespace Suguri46b.Scripts.Rewards;

/// <summary>
/// Post-combat reward granted by Warp Control card.
/// When selected, enables free map travel for the next room choice.
/// </summary>
public class WarpControlReward : ModCustomReward
{
    public const string RewardStem = "warp_control";

    private const string LocTable = "gameplay_ui";
    private const string DescLocKey = "SUGURI46B_REWARD_WARP_CONTROL.reward_desc";

    /// <summary>
    /// The registered RewardType for this custom reward. Set during mod initialization.
    /// </summary>
    public static RewardType RegisteredRewardType { get; set; }

    public override RewardType ModRewardType { get; }

    public override LocString Description => new(LocTable, DescLocKey);

    protected override string? RewardIconPath => "res://Suguri46b/images/powers/Warp_ControlPower.png";

    public override int RewardsSetIndex => 5;

    public WarpControlReward(Player player, RewardType rewardType) : base(player)
    {
        ModRewardType = rewardType;
    }

    protected override Task<bool> OnSelect()
    {
        WarpControlState.AddCharge();
        return Task.FromResult(true);
    }

    public override void MarkContentAsSeen()
    {
    }
}
