using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Cards.Token;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Norma : ModPowerTemplate,ISecondaryResourceHookListener
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
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
        }
        if (!Norma4 && Amount>=4)
        {
            Norma4=true;
            Flash();
            await PowerCmd.Apply<DoublePower>(choiceContext, base.Owner, 1, base.Owner, cardSource);
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
            await RemoveAllBuff();
        }
    }
    private static async Task RemoveAllBuff()
    {
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        var allEnemies = combatState!.Enemies.ToList();
        if (allEnemies.Count == 0)
        {
            await CombatManager.Instance.CheckWinCondition();
            return;
        }
        foreach (var enemy in allEnemies)
        {
            // 清除所有增益（Buff），保留减益（Debuff）
            var buffs = enemy.Powers.Where(p => p.Type == PowerType.Buff).ToList();
            foreach (var buff in buffs)
            {
                await PowerCmd.Remove(buff);
            }
        }
    }
    public override decimal ModifyMaxEnergy(Player player, decimal amount)
    {
        if (Norma3 && player==Owner.Player)
        {
            return amount + 1;
        }
        return amount;
    }
    public override int ModifyCardPlayCount(CardModel card, Creature? target, int playCount)
    {
        if (Norma5 && card.Owner.Creature.Player==Owner.Player && card.Type==CardType.Attack)
        {
            return playCount+1;
        }
        return playCount;
    }
}