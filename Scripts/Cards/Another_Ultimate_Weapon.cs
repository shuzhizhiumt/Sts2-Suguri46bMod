using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Extensions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Random;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Another_Ultimate_Weapon : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private static readonly TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );
    public Another_Ultimate_Weapon() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new DynamicVar("LoseOJStar", 10)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int count= (int)Math.Floor((double) Owner.PlayerCombatState.GetOJStarTotal() / base.DynamicVars["LoseOJStar"].IntValue)+base.DynamicVars.Cards.IntValue;
        CardPile DiscardPile = PileType.Discard.GetPile(base.Owner);
        IEnumerable<CardModel> enumerable = DiscardPile.Cards.Where((CardModel c) => c.Type == CardType.Attack && !c.Keywords.Contains(CardKeyword.Unplayable)).ToList().StableShuffle(base.Owner.RunState.Rng.Shuffle)
			.Take(count);
        foreach (CardModel item in enumerable)
		{
			if (!CombatManager.Instance.IsOverOrEnding)
			{
				if (item.TargetType == TargetType.AnyEnemy)
				{
					Creature target = base.Owner.RunState.Rng.CombatTargets.NextItem(base.CombatState.HittableEnemies);
					await CardCmd.AutoPlay(choiceContext, item, target);
				}
				else
				{
					await CardCmd.AutoPlay(choiceContext, item, null);
				}
				continue;
			}
			break;
		}
        await PlayerCmdExtensions.LoseOJStar(Owner.PlayerCombatState.GetOJStarTotal(),Owner);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Cards.UpgradeValueBy(1);
    }
}
