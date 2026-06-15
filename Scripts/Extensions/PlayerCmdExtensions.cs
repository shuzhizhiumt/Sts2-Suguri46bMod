using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;


static class PlayerCmdExtensions
{
    public static async Task NormaUPCheck(PlayerChoiceContext choiceContext,Player player,CardModel cardModel)
    {
        int Level = player?.Creature.GetPower<Norma>()?.Amount ?? 0;
        int currentOJStar = SecondaryResourceCmd.Get(player, ModResources.OJStarId);
        switch (Level)
        {
            case 0: await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);break;
            case 1: if (currentOJStar>10)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 2: if (currentOJStar>30)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 3: if (currentOJStar>70)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 4: if (currentOJStar>120)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
            case 5: if (currentOJStar>200)
                    {
                        await PowerCmd.Apply<Norma>(choiceContext, player.Creature, 1, player.Creature,cardModel);
                    };break;
        }
    }
}