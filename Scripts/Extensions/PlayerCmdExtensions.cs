using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
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
using Suguri46b.Scripts.Powers;


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
    public static async Task NormaUPCheck(PlayerChoiceContext choiceContext,Player player,CardModel cardModel)
    {
        int Level = player?.Creature.GetPower<Norma>()?.Amount ?? 0;
        switch (Level)
        {
            case 0: await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);break;
            case 1: if (player.PlayerCombatState.GetOJStarTotal()>10)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 2: if (player.PlayerCombatState.GetOJStarTotal()>30)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 3: if (player.PlayerCombatState.GetOJStarTotal()>70)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 4: if (player.PlayerCombatState.GetOJStarTotal()>120)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 5: if (player.PlayerCombatState.GetOJStarTotal()>200)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
        }
    }
}