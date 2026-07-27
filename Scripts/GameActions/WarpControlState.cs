using MegaCrit.Sts2.Core.Runs;

namespace Suguri46b.Scripts.GameActions;

/// <summary>
/// Static tracker for Warp Control free travel charges.
/// When a player takes the Warp Control reward, a charge is added.
/// The charge is consumed upon entering the next room.
/// </summary>
public static class WarpControlState
{
    private static int _freeTravelCharges;

    public static bool HasFreeTravel => _freeTravelCharges > 0;

    public static void AddCharge()
    {
        _freeTravelCharges++;
    }

    /// <summary>
    /// Consume one free travel charge when entering a top-level room.
    /// </summary>
    public static void TryConsume(IRunState runState)
    {
        if (_freeTravelCharges <= 0) return;

        // Only consume on top-level room entry, not nested sub-rooms
        if (runState.CurrentRoomCount > 1) return;

        _freeTravelCharges--;
    }
}
