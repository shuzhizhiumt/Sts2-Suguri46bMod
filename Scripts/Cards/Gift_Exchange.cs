using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
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
        var combatState = base.CombatState;
        if (combatState == null)
            return;

        var players = combatState.PlayerCreatures
            .Where(c => c.IsAlive && c.IsPlayer)
            .Select(c => c.Player)
            .Where(p => p != null)
            .Cast<Player>()
            .ToList();

        if (players.Count <= 1)
            return;

        var prefs = new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1);
        var selections = new List<(Player owner, CardModel card)>();

        // 每位玩家依次从手牌中选择一张牌
        foreach (var player in players)
        {
            var card = (await CardSelectCmd.FromHand(
                prefs: prefs,
                context: choiceContext,
                player: player,
                filter: null,
                source: this)).FirstOrDefault();

            if (card != null)
                selections.Add((player, card));
        }

        if (selections.Count == 0)
            return;

        // 每张选中的牌随机给予另一位玩家
        var rng = base.Owner.RunState.Rng.CombatTargets;
        foreach (var (owner, card) in selections)
        {
            var teammates = combatState.GetTeammatesOf(owner.Creature)
                .Where(c => c.IsAlive && c.IsPlayer && c.Player != owner)
                .ToList();

            if (teammates.Count > 0)
            {
                var targetCreature = rng.NextItem(teammates);
                if (targetCreature?.Player != null)
                    await CardPileCmd.GiveToAnotherPlayer(card, targetCreature.Player, PileType.Hand, CardPilePosition.Random);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
