using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using STS2RitsuLib.Combat.CardTargeting;

namespace Suguri46b.Scripts.Extensions;

public static class TheCustomTargetType
{
    public static TargetType AnyPlayerTarget { get; private set; }
     public static void Register()
    {
        AnyPlayerTarget = CustomTargetType.RegisterSingleTargetType(
            Entry.ModId,
            "ANY_PLAYER",
            creature => creature is { IsPlayer: true, IsAlive: true }
        );
    }
 
}