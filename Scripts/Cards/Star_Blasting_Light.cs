using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Star_Blasting_Light : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.RandomEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(5, ValueProp.Move),
        new CalculationBaseVar(0),
		new CalculationExtraVar(1),
        new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => GetAttacksAndStatuses(card.Owner).Count())
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust),
    ];
    public Star_Blasting_Light() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
		List<CardModel> list = GetAttacksAndStatuses(base.Owner).ToList();
		int attacksandstatusCount = (int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target);
		foreach (CardModel item in list)
		{
			await CardCmd.Exhaust(choiceContext, item);
		}
		await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue).WithHitCount(attacksandstatusCount).FromCard(this)
			.TargetingRandomOpponents(base.CombatState)
			.Execute(choiceContext);
    }
	private static IEnumerable<CardModel> GetAttacksAndStatuses(Player owner)
	{
		return owner.PlayerCombatState.AllCards.Where((CardModel c) => (c.Type == CardType.Attack || c.Type == CardType.Status) && c.Pile.Type != PileType.Exhaust);
	}
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3);
    }
}