using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;
using Suguri46b.Scripts.Extensions;
using Godot;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Extension: ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Extension() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2)
        ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selectedCards = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, DynamicVars.Cards.IntValue),
            context: choiceContext,
            player: Owner,
            filter: card => card.Enchantment == null && RandomEnchantments.CanBeEnchanted(card) && (card.Type==CardType.Attack||card.Type==CardType.Skill||card.Type==CardType.Power),
            source: this);

        if (selectedCards == null)
        {
            return;
        }

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        foreach (var selectedCard in selectedCards)
        {
            var validEnchantments =RandomEnchantments.GetValidEnchantments(selectedCard);
            if (validEnchantments.Count == 0)
            {
                continue;
            }

            var chosenEnchantment = validEnchantments[rng.NextInt(validEnchantments.Count)];
            CardCmd.Enchant(chosenEnchantment, selectedCard, 1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}