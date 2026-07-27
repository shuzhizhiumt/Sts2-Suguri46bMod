using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Gift_Exchange : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.AllAllies;
    private const bool shouldShowInCardLibrary = true;
    public override CardMultiplayerConstraint MultiplayerConstraint => CardMultiplayerConstraint.MultiplayerOnly;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Gift_Exchange() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        // Get all teammate players (non-dead, with non-empty hand)
        var teamPlayers = base.CombatState.GetTeammatesOf(base.Owner.Creature)
            .Select(t => t.Player)
            .Where(p => p != null && !p.Creature.IsDead && PileType.Hand.GetPile(p!).Cards.Count > 0)
            .Select(p => p!)
            .ToList();

        if (teamPlayers.Count <= 1)
            return;

        // Randomly pick one card from each teammate's hand (deterministic RNG, multiplayer-safe)
        var selections = new Dictionary<Player, CardModel>();
        foreach (var player in teamPlayers)
        {
            var hand = PileType.Hand.GetPile(player).Cards;
            int idx = base.Owner.RunState.Rng.CombatCardSelection.NextInt(hand.Count);
            selections[player] = hand[idx];
        }

        if (selections.Count <= 1)
            return;

        // Deterministic sort by NetId, then cyclic exchange: A→B, B→C, ..., Z→A
        var players = selections.Keys.OrderBy(p => p.NetId).ToList();
        for (int i = 0; i < players.Count; i++)
        {
            var giver = players[i];
            var receiver = players[(i + 1) % players.Count];
            await CardPileCmd.GiveToAnotherPlayer(selections[giver], receiver, PileType.Discard);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
