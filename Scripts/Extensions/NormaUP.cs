using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using Suguri46b.Scripts.Enchantments;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Extensions;

[RegisterSingleton]
public class NormaUP : HookedSingletonModel,ISecondaryResourceHookListener
{
    public NormaUP() : base(HookType.Combat)
    {
    }
    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {

        if (context.Definition.Id != ModResources.OJStarId || !(context.NewAmount > 0))
            return;
        await PlayerCmdExtensions.NormaUPCheck(new ThrowingPlayerChoiceContext(),context.Player, null);
    }
}
