using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Final_BattlePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Debuff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );
    private readonly List<(CardModel card, int amount)> _affectedCards = [];

    public void TrackReplay(CardModel card, int amount)
    {
        _affectedCards.Add((card, amount));
    }

    public override bool ShouldPlay(CardModel card, AutoPlayType _)
    {
        if (card.Owner.Creature != base.Owner)
            return true;
        return card.Type == CardType.Attack;
    }
	public override bool ShouldDraw(Player player, bool fromHandDraw)
	{
		if (fromHandDraw)
		{
			return true;
		}
		if (player != base.Owner.Player)
		{
			return true;
		}
		Flash();
		return false;
	}
    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(base.Owner))
            return;

        foreach (var (card, amount) in _affectedCards)
        {
            if (card.IsInCombat && card.BaseReplayCount >= amount)
                card.BaseReplayCount -= amount;
        }
        _affectedCards.Clear();
        await PowerCmd.Remove(this);
    }
}
