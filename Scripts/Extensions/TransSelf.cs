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
        if (card.Enchantment == null || card.Enchantment.GetType() != ModelDb.Enchantment<Mix>().GetType())
            return;

        var owner = card.Owner;
        List<CardPoolModel> allPools = [.. owner.UnlockState.CharacterCardPools];
        IEnumerable<CardModel> allCards = allPools
            .SelectMany(pool => pool.GetUnlockedCards(
                owner.UnlockState,
                owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Type == CardType.Attack || c.Type == CardType.Skill || c.Type == CardType.Power);

        var newcard = CardFactory.GetDistinctForCombat(
            owner,
            allCards,
            1,
            owner.RunState.Rng.CombatCardGeneration
        ).First();

        pendingTransforms[card] = newcard;
    }

    public override async Task AfterCardChangedPiles(CardModel card, PileType oldPileType, AbstractModel? clonedBy)
    {
        if (pendingTransforms.Remove(card, out var target))
        {
            var result = await CardCmd.Transform(card, target);
            if (result.HasValue)
                CardCmd.Enchant<Mix>(result.Value.cardAdded, 1);
        }
    }
}
