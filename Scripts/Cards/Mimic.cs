using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;
using STS2RitsuLib.Combat.HandSize;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Extensions;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Mimic : ModCardTemplate, IMaxHandSizeModifier
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );

    public Mimic() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {

    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (PileType.Hand.GetPile(base.Owner).Cards.Count<=1)
        {
            return;
        }
        CardModel cardModel = (await CardSelectCmd.FromHand(prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), context: choiceContext, player: base.Owner, filter: null, source: this)).FirstOrDefault();
        if (cardModel != null)
        {
    		IEnumerable<CardModel> choices = PileType.Hand.GetPile(base.Owner).Cards.Where(c=>c!=cardModel).ToList().StableShuffle(base.Owner.RunState.Rng.CombatCardSelection).Take(base.DynamicVars.Cards.IntValue);
            if (choices == null)
            {
                return;
            }
            CardModel selectcard = await CardSelectCmd.FromChooseACardScreen(choiceContext, (IReadOnlyList<CardModel>)choices, Owner, canSkip: false);
            if (selectcard != null)
            {
    			CardModel selectcardCopy = selectcard.CreateClone();
                await CardCmd.Transform(cardModel,selectcardCopy);
            }
        }
    }
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}