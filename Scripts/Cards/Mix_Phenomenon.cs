using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Enchantments;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Mix_Phenomenon : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private static readonly TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        HoverTipFactory.FromEnchantment<Mix>();
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];
    
    public Mix_Phenomenon() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selectedCards = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, DynamicVars.Cards.IntValue),
            context: choiceContext,
            player: Owner,
            filter: card => card.Enchantment == null && !card.HasModKeyword(CardKeyword.Unplayable),
            source: this);
        foreach (CardModel selectedCard in selectedCards)
        {
            CardCmd.Enchant<Mix>(selectedCard,1);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}
