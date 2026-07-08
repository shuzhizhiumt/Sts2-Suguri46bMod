using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(EventCardPool))]
public class Star_Blasting_Fuse : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Event;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Star_Blasting_Fuse() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Exhaust)
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(10, ValueProp.Move),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("CalculatedHits").WithMultiplier((CardModel card, Creature? _) => GetInvisible_Bomb(card.Owner).Count())
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardModel> list = GetInvisible_Bomb(base.Owner).ToList();
        int attacksandstatusCount = (int)((CalculatedVar)base.DynamicVars["CalculatedHits"]).Calculate(cardPlay.Target);
        foreach (CardModel item in list)
        {
            await CardCmd.Exhaust(choiceContext, item);
        }
        await DamageCmd.Attack(base.DynamicVars.Damage.BaseValue)
            .WithHitCount(attacksandstatusCount) 
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(5);
    }

    private static IEnumerable<CardModel> GetInvisible_Bomb(Player owner)
    {
        return owner.PlayerCombatState.AllCards.Where((CardModel c) => c.Tags.Any(t => t == MyTags.Invisible_Bomb) && c.Pile.Type != PileType.Exhaust);
    }
}
