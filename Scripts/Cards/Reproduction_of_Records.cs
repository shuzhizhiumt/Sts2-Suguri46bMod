using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class  Reproduction_of_Records: ModCardTemplate
{
    private const int energyCost = 3;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Rare;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2)
    ];

    public Reproduction_of_Records() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var discardPile = PileType.Discard.GetPile(base.Owner);
        int takeCount = base.DynamicVars.Cards.IntValue;
        var topCards = discardPile.Cards.TakeLast(takeCount).ToList();
        foreach (var card in topCards)
        {
            CardModel clonedCard = card.CreateClone();
            clonedCard.AddKeyword(CardKeyword.Exhaust);
            clonedCard.SetToFreeThisCombat();
            await CardPileCmd.AddGeneratedCardToCombat(
                clonedCard, 
                PileType.Hand, 
                base.Owner
            );
        }
	}

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}