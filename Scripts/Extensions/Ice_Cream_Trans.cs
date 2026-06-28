using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Models;
using Suguri46b.Scripts.Cards;
using Suguri46b.Scripts.Cards.Token;
using Suguri46b.Scripts.Enchantments;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Extensions;

[RegisterSingleton]
public class Ice_Cream_Trans : HookedSingletonModel
{
    public Ice_Cream_Trans() : base(HookType.Combat)
    {
    }
    private readonly List<CardModel> cardModels= new();
    private CardModel? newcard;

    private bool IsTrans;
    public override Task BeforeCombatStart()
    {
        cardModels.Clear();
        newcard=null;
        IsTrans=false;
        return base.BeforeCombatStart();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.Title == cardPlay.Card.CombatState.CreateCard<Miracle_Red_Bean_Ice_Cream>(cardPlay.Card.Owner).Title && cardPlay.SecondaryResources().Activated("ojstars_charge") && !(cardPlay.Card.Enchantment != null && cardPlay.Card.Enchantment.GetType() == ModelDb.Enchantment<Mix>().GetType()))
        {
            cardModels.Add(cardPlay.Card);
            newcard = cardPlay.Card.CombatState.CreateCard<Magical_Revenge>(cardPlay.Card.Owner);
        }
        if (cardPlay.Card.Title == cardPlay.Card.CombatState.CreateCard<Magical_Revenge>(cardPlay.Card.Owner).Title)
        {
            cardModels.Add(cardPlay.Card);
            newcard = cardPlay.Card.CombatState.CreateCard<Miracle_Red_Bean_Ice_Cream>(cardPlay.Card.Owner);
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {

        if (cardModels.Count>0 && newcard!= null && !IsTrans)
        {
            IsTrans= true;
            foreach (var item in cardModels)
            {
                await CardCmd.Transform(item,newcard);
            }
            cardModels.Clear();
            newcard=null;
            IsTrans= false;
        }
    }
}