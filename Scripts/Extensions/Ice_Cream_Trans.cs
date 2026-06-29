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

    // 每张待变换的卡 → 目标卡，一对一映射，支持同时多张
    private readonly Dictionary<CardModel, CardModel> pendingTransforms = [];

    public override Task BeforeCombatStart()
    {
        pendingTransforms.Clear();
        return base.BeforeCombatStart();
    }

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var card = cardPlay.Card;
        var owner = card.Owner;
        var combatState = card.CombatState;
        if (combatState == null)
            return;

        // Miracle_Red_Bean_Ice_Cream（支付了 ojstars）→ Magical_Revenge
        if (card.Title == combatState.CreateCard<Miracle_Red_Bean_Ice_Cream>(owner).Title
            && cardPlay.SecondaryResources().Activated("ojstars_charge")
            && !(card.Enchantment != null && card.Enchantment.GetType() == ModelDb.Enchantment<Mix>().GetType()))
        {
            pendingTransforms[card] = combatState.CreateCard<Magical_Revenge>(owner);
        }
        // Magical_Revenge → Miracle_Red_Bean_Ice_Cream
        else if (card.Title == combatState.CreateCard<Magical_Revenge>(owner).Title)
        {
            pendingTransforms[card] = combatState.CreateCard<Miracle_Red_Bean_Ice_Cream>(owner);
        }
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (pendingTransforms.Remove(card, out var target))
        {
            await CardCmd.Transform(card, target);
        }
    }
}
