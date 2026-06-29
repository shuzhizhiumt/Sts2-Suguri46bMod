using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Sealed_Memories : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Sealed_Memories() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GainOJStar", 5),
        new BlockVar(7, ValueProp.Move)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selectedCards = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "REMOVE_ENCHANTMENTS"), 0, int.MaxValue),
            context: choiceContext,
            player: Owner,
            filter: card => card.Enchantment != null,
            source: this);

        if (selectedCards == null)
        {
            return;
        }

        int removedCount = 0;
        foreach (var card in selectedCards)
        {
            if (card.Enchantment != null)
            {
                CardCmd.ClearEnchantment(card);
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            for (int i = 0; i < removedCount; i++)
            {
                await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
            }
            await SecondaryResourceCmd.Gain(Owner, ModResources.OJStarId, removedCount * base.DynamicVars["GainOJStar"].IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["GainOJStar"].UpgradeValueBy(2);
    }
}
