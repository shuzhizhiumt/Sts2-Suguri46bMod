using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Runs;
using Suguri46b.Scripts.Extensions;


static class PlayerCmdExtensions
{
    public static async Task GainOJStar(decimal amount, Player player)
    {
        if (!CombatManager.Instance.IsEnding)
        {
            player.PlayerCombatState.GainOJStar((int)amount, player);
        }
    }
    public static Task LoseOJStar(decimal amount, Player player)
    {
        if (CombatManager.Instance.IsEnding)
        {
            return Task.CompletedTask;
        }
        player.PlayerCombatState.LoseOJStar((int)amount, player);
        return Task.CompletedTask;
    }
}