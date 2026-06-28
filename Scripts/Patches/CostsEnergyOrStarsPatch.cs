using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Patches;

public class CostsEnergyOrStarsPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_ojstar_costs_energy_or_stars";
    public static string Description => "Make CostsEnergyOrStars return true for cards with OJStar secondary resource costs.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() => [
        new(typeof(CardModel), nameof(CardModel.CostsEnergyOrStars))
    ];

    public static void Postfix(CardModel __instance, bool includeGlobalModifiers, ref bool __result)
    {
        if (__result)
            return;

        if (__instance.TryGetSecondaryCosts(out var costSet)
            && costSet.HasCosts
            && costSet.ResourceIds.Contains(ModResources.OJStarId))
        {
            __result = true;
            return;
        }

        if (__instance.TryGetSecondaryResourceUses(out var useSet)
            && useSet.HasUses
            && useSet.UseIds.Any(id => id.Contains("ojstar")))
        {
            __result = true;
            return;
        }
    }
}
