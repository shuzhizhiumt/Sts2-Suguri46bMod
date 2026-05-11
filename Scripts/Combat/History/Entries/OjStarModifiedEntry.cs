using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;

public class OJStarModifiedEntry
{
    public PlayerCombatState State { get; }
    public int Amount { get; }
    public int RoundNumber { get; }
    public CombatSide Side { get; }

    public OJStarModifiedEntry(PlayerCombatState state, int amount, int roundNumber, CombatSide side)
    {
        State = state;
        Amount = amount;
        RoundNumber = roundNumber;
        Side = side;
    }

    public bool HappendThisTurn(CombatState? combatState)
    {
        if (combatState == null) return false;
        return RoundNumber == combatState.RoundNumber && Side == combatState.CurrentSide;
    }

}