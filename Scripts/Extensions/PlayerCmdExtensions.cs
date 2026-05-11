using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Players;


static class PlayerCmdExtensions
{
    public static async Task GainOJStar(decimal amount,Player player)
    {
        if (!CombatManager.Instance.IsEnding)
        {
            player.PlayerCombatState.GainOJStar((int)amount,player);
        }
    }
    public static Task LoseOJStar(decimal amount,Player player)
    {
        if (CombatManager.Instance.IsEnding)
        {
            return Task.CompletedTask;
        }
        player.PlayerCombatState.LoseOJStar((int)amount,player);
        return Task.CompletedTask;
    }
}