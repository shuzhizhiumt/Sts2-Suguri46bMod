using MegaCrit.Sts2.Core.Combat;

public class OJStarHistory
{
    private readonly List<OJStarModifiedEntry> _entries = new();
    public IEnumerable<OJStarModifiedEntry> Entries => _entries;
    public void Add(OJStarModifiedEntry entry)
    {
        _entries.Add(entry);
    }
    public void Clear()
    {
        _entries.Clear();
    }
    public int GainedThisTurn(CombatState state)
    {
        return _entries.Where(e => e.HappendThisTurn(state) && e.Amount > 0).Sum(e => e.Amount);
    }

    internal int GainedThisTurn(ICombatState? combatState)
    {
        throw new NotImplementedException();
    }
}