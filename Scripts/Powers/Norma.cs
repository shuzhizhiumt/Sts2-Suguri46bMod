using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Norma : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/powers/Norma.png",
        BigIconPath: "res://Suguri46b/images/powers/Norma.png"
    );
    public bool Norma2;
    public bool Norma3;
    public bool Norma4;
    public bool Norma5;
    public bool Norma6;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power != this || Owner == null || Owner.Player == null || !CombatManager.Instance.IsInProgress)
        {
            return;
        }
        if (!Norma2 && Amount>=2)
        {
            Norma2=true;
            Flash();
            await PowerCmd.Apply<StrengthPower>(choiceContext,base.Owner, 1, base.Owner,cardSource);
        }
        if (!Norma3 && Amount>=3)
        {
            Norma3=true;
            Flash();
            await PowerCmd.Apply<DexterityPower>(choiceContext,base.Owner, 1, base.Owner,cardSource);
        }
        if (!Norma4 && Amount>=4)
        {
            Norma4=true;
            Flash();
            await PowerCmd.Apply<StrengthPower>(choiceContext,base.Owner, 1, base.Owner,cardSource);
            await PowerCmd.Apply<DexterityPower>(choiceContext,base.Owner, 1, base.Owner,cardSource);
        }
        if (!Norma5 && Amount>=5)
        {
            Norma5=true;
            Flash();
        }
        if (!Norma6 && Amount>=6)
        {
            Norma6=true;
            Flash();
            await ExecuteCombatVictory();
        }
    }
        private async Task ExecuteCombatVictory()
        {
            var combatState = CombatManager.Instance.DebugOnlyGetState();
            List<Creature> allEnemies = [.. combatState!.Enemies];

            if (allEnemies.Count == 0)
            {
                await CombatManager.Instance.CheckWinCondition();
                return;
            }

            foreach (var enemy in allEnemies)
            {
                enemy.RemoveAllPowersInternalExcept();
                await CreatureCmd.Kill(enemy);
            }

            await CombatManager.Instance.CheckWinCondition();
        }
    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (Norma5)
        {
            return amount + 1;
        }
        return amount;
    }
}