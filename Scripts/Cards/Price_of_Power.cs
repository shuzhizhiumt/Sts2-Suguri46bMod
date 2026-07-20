using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Price_of_Power : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Price_of_Power() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
        .SpendIfAvailable("ojstars_charge", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Additional_Payment",7),
        new CardsVar(3)
    ];
    protected override bool ShouldGlowGoldInternal => SecondaryResourceCmd.Get(Owner, ModResources.OJStarId) >= base.DynamicVars["Additional_Payment"].BaseValue;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardModel? selectedCard = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), context: choiceContext, player: Owner, filter: (CardModel c) =>c.Type == CardType.Attack || c.Type == CardType.Skill ||c.Type == CardType.Power, source: this)).FirstOrDefault();
        if (selectedCard==null)
        {
            return;
        }
        List<CardPoolModel> otherPools = [.. base.Owner.UnlockState.CharacterCardPools];
        IEnumerable<CardModel> candidateCards;
        var ledger = cardPlay.SecondaryResources();
        if (ledger.Activated("ojstars_charge"))
        {
            candidateCards = from c in otherPools.SelectMany(c => c.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
                where c.Type == selectedCard.Type && (selectedCard.Rarity == CardRarity.Basic || selectedCard.Rarity == CardRarity.Common?c.Rarity == CardRarity.Uncommon:selectedCard.Rarity ==CardRarity.Uncommon || selectedCard.Rarity ==CardRarity.Rare?c.Rarity == CardRarity.Rare:c.Rarity==CardRarity.Ancient)
                select c;
        }
        else
        {
            candidateCards = from c in otherPools.SelectMany(c => c.GetUnlockedCards(base.Owner.UnlockState, base.Owner.RunState.CardMultiplayerConstraint))
                where c.Type == selectedCard.Type && (selectedCard.Rarity == CardRarity.Basic?c.Rarity == CardRarity.Common:c.Rarity == selectedCard.Rarity)
                select c;
        }
        List<CardModel> choices =  CardFactory.GetDistinctForCombat(base.Owner, candidateCards, 3, base.Owner.RunState.Rng.CombatCardGeneration).ToList();

        CardModel? chosenCard = await CardSelectCmd.FromChooseACardScreen(choiceContext, choices, Owner, canSkip: false);
        if (chosenCard==null)
        {
            return;
        }
        await CardCmd.Transform(selectedCard, chosenCard);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Additional_Payment"].UpgradeValueBy(-2);
    }
}
