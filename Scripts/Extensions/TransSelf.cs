using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using Suguri46b.Scripts.Enchantments;

namespace Suguri46b.Scripts.Extensions;

[RegisterSingleton]
public class TransSelf : HookedSingletonModel
{
    public TransSelf() : base(HookType.Combat)
    {
    }
    private readonly List<CardModel> cardModel= new();
    private CardModel? newcard;
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Enchantment != null && cardPlay.Card.Enchantment.GetType() == ModelDb.Enchantment<Mix>().GetType())
        {
            cardModel.Add(cardPlay.Card);
            List<CardPoolModel> allPools = [.. cardPlay.Card.Owner.UnlockState.CharacterCardPools];
            IEnumerable<CardModel> allCards = allPools
                .SelectMany(pool => pool.GetUnlockedCards(
                    cardPlay.Card.Owner.UnlockState,
                    cardPlay.Card.Owner.RunState.CardMultiplayerConstraint)).Where(c=>c.Type==CardType.Attack || c.Type==CardType.Skill || c.Type==CardType.Power);
            newcard = CardFactory.GetDistinctForCombat(
                cardPlay.Card.Owner,
                allCards,
                1,
                cardPlay.Card.Owner.RunState.Rng.CombatCardGeneration
            ).First();
        }
    }
    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (cardModel.Count>0 && newcard!= null)
        {
            foreach (var item in cardModel)
            {
                cardModel.Remove(item);
                CardPileAddResult? cardPileAddResult=await CardCmd.Transform(item,newcard);
                CardCmd.Enchant<Mix>(cardPileAddResult.Value.cardAdded,1);
            }
        }
    }
}