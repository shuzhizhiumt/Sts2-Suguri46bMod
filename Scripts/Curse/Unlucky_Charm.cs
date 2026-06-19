using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Curse;

[RegisterCard(typeof(CurseCardPool))]
public class Unlucky_Charm : ModCardTemplate
{
    private const int energyCost = -1;
    private const CardType type = CardType.Curse;
    private const CardRarity rarity = CardRarity.Curse;
    private const TargetType targetType = TargetType.None;
    private const bool shouldShowInCardLibrary = true;
    public override int MaxUpgradeLevel => 0;
    public override bool HasTurnEndInHandEffect => true;
    public Unlucky_Charm() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }
    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Unplayable];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>[
        HoverTipFactory.FromPower<Norma>()
        ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("LoseOJStar", 10)
    ];
    private int _level;
    private int Level
    {
        get
        {
            return _level;
        }
        set
        {
            AssertMutable();
            _level=value;
        }
    }
    public override Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player)
		{
			return Task.CompletedTask;
		}
        Level = Owner?.Creature.GetPower<Norma>()?.Amount ?? 0;
        return Task.CompletedTask;
    }
    protected override async Task OnTurnEndInHand(PlayerChoiceContext choiceContext)
    {
        await SecondaryResourceCmd.Lose(Owner, ModResources.OJStarId,Level*base.DynamicVars["LoseOJStar"].IntValue);
    }

}