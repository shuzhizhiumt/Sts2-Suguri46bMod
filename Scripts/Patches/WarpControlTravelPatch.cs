using MegaCrit.Sts2.Core.Hooks;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.GameActions;

namespace Suguri46b.Scripts.Patches;

/// <summary>
/// Patch Hook.ShouldAllowFreeTravel: if Warp Control charges are available,
/// return true immediately without iterating hook listeners.
/// </summary>
public class WarpControlShouldAllowFreeTravelPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_warp_control_free_travel";
    public static string Description => "Enable Warp Control free travel when charges are available.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(Hook), nameof(Hook.ShouldAllowFreeTravel)),
    ];

    public static bool Prefix(ref bool __result)
    {
        if (WarpControlState.HasFreeTravel)
        {
            __result = true;
            return false;
        }
        return true;
    }
}

/// <summary>
/// Patch Hook.AfterRoomEntered: attempt to consume a Warp Control charge
/// if free travel was actually used to reach this room.
/// </summary>
public class WarpControlAfterRoomEnteredPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_warp_control_consume";
    public static string Description => "Consume Warp Control charge when free travel is used.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
    [
        new(typeof(Hook), nameof(Hook.AfterRoomEntered)),
    ];

    public static void Prefix(IRunState runState, AbstractRoom room)
    {
        WarpControlState.TryConsume(runState);
    }
}
