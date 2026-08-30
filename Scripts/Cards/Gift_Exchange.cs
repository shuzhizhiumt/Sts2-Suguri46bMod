using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
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
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        // 所有存活的玩家（包括自己）
        List<Player> players = base.CombatState.Players
            .Where(p => p != null && p.Creature != null && p.Creature.IsAlive)
            .ToList();

        // 第一步：每位玩家选择手中一张牌（全部选完再统一送出）
        List<(CardModel Card, Player From)> chosen = [];
        foreach (Player player in players)
        {
            CardModel? selected = (await CardSelectCmd.FromHand(
                context: choiceContext,
                player: player,
                prefs: new CardSelectorPrefs(new LocString("card_selection", "GIFT_EXCHANGE"), 1),
                filter: null,
                source: this)).FirstOrDefault();
            if (selected != null)
            {
                chosen.Add((selected, player));
            }
        }

        // 第二步：每张选中的牌交给随机的一名其他玩家
        foreach ((CardModel card, Player from) in chosen)
        {
            List<Player> others = players.Where(p => p != from).ToList();
            if (others.Count == 0)
            {
                continue;
            }
            Player? receiver = base.Owner.RunState.Rng.CombatTargets.NextItem(others);
            if (receiver == null)
            {
                continue;
            }
            await CardPileCmd.GiveToAnotherPlayer(card, receiver, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
