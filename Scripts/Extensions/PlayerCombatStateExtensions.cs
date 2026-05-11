using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;

static class PlayerCombatStateExtensions
{
    private class ExtraOJStarData
    {
        public int OJStarTotal;
        public OJStarHistory History = new OJStarHistory();
    }

    private static readonly ConditionalWeakTable<PlayerCombatState, ExtraOJStarData> _data = new ConditionalWeakTable<PlayerCombatState, ExtraOJStarData>();
    private static ExtraOJStarData GetData(PlayerCombatState pcs) => _data.GetOrCreateValue(pcs);
    public static int GetOJStarTotal(this PlayerCombatState pcs) => GetData(pcs).OJStarTotal;
    public static void GainOJStar(this PlayerCombatState pcs, int amount,Player player)
    {
        if(amount <0) throw new ArgumentException("Must not be negative", nameof(amount));
        var data = GetData(pcs);
        data.OJStarTotal += amount;
        data.History.Add(new OJStarModifiedEntry(pcs, amount, player.Creature.CombatState.RoundNumber,MegaCrit.Sts2.Core.Combat.CombatSide.Player));
        if (LocalContext.IsMe(player.Creature))
        {
            Suguri46bOJStarNodeInitPatch.GetOJStarCounterLabel().Text = $"[center]{data.OJStarTotal}[/center]";
        }
        Log.Info($">>>[Suguri46bMod]Player = " + player.NetId + "GainOjstar Successfully " + $"[OJStar] +{amount}, total = {data.OJStarTotal}");
    }
    public static void LoseOJStar(this PlayerCombatState pcs, int amount,Player player)
    {
        if(amount <0) throw new ArgumentException("Must not be negative", nameof(amount));
        var data = GetData(pcs);
        data.OJStarTotal = Math.Max(data.OJStarTotal - amount,0);
        if (LocalContext.IsMe(player.Creature))
        {
            Suguri46bOJStarNodeInitPatch.GetOJStarCounterLabel().Text = $"[center]{data.OJStarTotal}[/center]";
        }
        Log.Info($">>>[Suguri46bMod]Player = " + player.NetId + "LoseOjstar Successfully " + $"[OJStar] -{amount}, total = {data.OJStarTotal}");
    }

    public static int GetOJStarGainedThisTurn(this PlayerCombatState pcs, Player player)
    {
        return GetData(pcs).History.GainedThisTurn(player.Creature.CombatState);
    }
}