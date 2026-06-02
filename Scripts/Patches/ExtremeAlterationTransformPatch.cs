using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.Powers;

namespace Suguri46b.Scripts.Patches;
public class ExtremeAlterationTransformPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_extreme_alteration_transform";
    public static string Description => "Draw a card when a card transforms if the owner has Extreme Alteration power.";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() => [new(typeof(CardModel), "AfterTransformedTo")];

    public static void Postfix(CardModel __instance)
    {
        if (__instance?.Owner == null)
            return;

        var power = __instance.Owner.Creature?.GetPower<Extreme_AlterationPower>();
        if (power == null)
            return;

        var stacks = power.Amount;
        if (stacks <= 0)
            return;
        CardPileCmd.Draw(new BlockingPlayerChoiceContext(), (decimal)stacks, __instance.Owner);
    }
}
