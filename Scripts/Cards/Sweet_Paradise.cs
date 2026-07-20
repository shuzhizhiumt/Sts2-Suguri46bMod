using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Cards.Token;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Sweet_Paradise : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Power;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;
	public override string[] AllPortraitPaths => new string[2]
	{
		GetPortraitPath(0),
		GetPortraitPath(1)
	};
	public override string PortraitPath => GetPortraitPath(IsUpgraded?1:0);
    private string GetPortraitPath(int witherLevel)
	{
		return $"res://Suguri46b/images/cards/{GetPortraitFilename(witherLevel)}.webp";
	}
    	private string GetPortraitFilename(int witherLevel)
    {
        if (witherLevel>=1)
        {
            return "Sweet_Paradise2";
        }
        else
        {
            return "Sweet_Paradise1";
        }
    }
    public Sweet_Paradise() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MyKeywords.Sweets),
        HoverTipFactory.FromCard<Sweet_Indulgence>(IsUpgraded)
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new PowerVar<Sweet_ParadisePower>(5),
        new CardsVar(1),
        new DynamicVar("GainOJStar", 5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        CardModel cardModel = base.CombatState.CreateCard<Sweet_Indulgence>(base.Owner);
        if (base.IsUpgraded)
        {
            CardCmd.Upgrade(cardModel);
        }
        CardCmd.PreviewCardPileAdd(await CardPileCmd.AddGeneratedCardToCombat(cardModel, PileType.Discard, base.Owner,CardPilePosition.Top), 1f);
        await PowerCmd.Apply<Sweet_ParadisePower>(choiceContext, base.Owner.Creature, base.DynamicVars["Sweet_ParadisePower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
    }
}
