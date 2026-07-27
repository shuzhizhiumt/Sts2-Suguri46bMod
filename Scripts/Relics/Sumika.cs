using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Combat.HandSize;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Relics;

[RegisterRelic(typeof(Suguri46bRelicPool))]
public class Sumika : ModRelicTemplate, IMaxHandSizeModifier
{
	private bool ActivatedThisTurn = false;
	private int AttackCardsPlayedThisCombat = 3;
	public override RelicRarity Rarity => RelicRarity.Starter;
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
    ];
	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new CardsVar(1),
		new DynamicVar("Turns", 3),
		new DynamicVar("MaxHandSize",2)
	];

	public override RelicAssetProfile AssetProfile => new(
		IconPath: $"res://Suguri46b/images/relics/{GetType().Name}.png",
		IconOutlinePath: $"res://Suguri46b/images/relics/{GetType().Name}.png",
		BigIconPath: $"res://Suguri46b/images/relics/{GetType().Name}.png"
	);
	public override bool ShowCounter => base.ShowCounter;
	public override Task AfterRoomEntered(AbstractRoom room)
	{

		if (room is CombatRoom)
		{
			ActivatedThisTurn= false;
		}
		return Task.CompletedTask;
	}
    public override Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
		if (
			CombatManager.Instance.IsInProgress
			&& player == base.Owner
			&& (decimal)base.Owner.PlayerCombatState.TurnNumber <= base.DynamicVars["Turns"].BaseValue
		)
		{
			ActivatedThisTurn = false;
		}
		return Task.CompletedTask;
    }
    public int ModifyMaxHandSize(Player player, int currentMaxHandSize)
    {
        if (player != Owner)
            return currentMaxHandSize;
        return currentMaxHandSize + DynamicVars["MaxHandSize"].IntValue;
    }
	public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if ((decimal)base.Owner.PlayerCombatState.TurnNumber > base.DynamicVars["Turns"].BaseValue)
		{
			return;
		}
		if (
			CombatManager.Instance.IsInProgress
			&& cardPlay.Card.Owner == base.Owner
			&& cardPlay.Card.Type == CardType.Attack
			&& !ActivatedThisTurn
		)
		{
			Flash();
            CardModel clonedCard = cardPlay.Card.CreateClone();
            clonedCard.AddKeyword(CardKeyword.Retain);
            clonedCard.AddKeyword(CardKeyword.Exhaust);
            await CardPileCmd.AddGeneratedCardToCombat(
                clonedCard,
                PileType.Hand,
                base.Owner
            );
			ActivatedThisTurn = true;
		}
	}
}
