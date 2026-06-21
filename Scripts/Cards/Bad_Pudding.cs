using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Bad_Pudding : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Bad_Pudding() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override HashSet<CardTag> CanonicalTags => [
        MyTags.Pudding
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new CardsVar(1)
        ];
    
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PlayerCmd.GainEnergy(DynamicVars.Energy.IntValue,cardPlay.Card.Owner);
        CardPile pile = PileType.Hand.GetPile(base.Owner);
		CardModel cardModel2 = base.Owner.RunState.Rng.CombatCardSelection.NextItem(pile.Cards);
		if (cardModel2 != null)
		{
			await CardCmd.Discard(choiceContext, cardModel2);
		}
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Energy.UpgradeValueBy(1);
    }
}
