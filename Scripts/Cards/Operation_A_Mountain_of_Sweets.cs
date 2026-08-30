using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
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
public class Operation_A_Mountain_of_Sweets : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Operation_A_Mountain_of_Sweets() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
            .SpendIfAvailable("ojstars_charge", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [
        CardKeyword.Exhaust, MyKeywords.Additional_Payment
    ];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MyKeywords.Sweets)
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(3),
        new DynamicVar("Additional_Payment", 10),
        new EnergyVar(1)
    ];

    protected override bool ShouldGlowGoldInternal => SecondaryResourceCmd.Get(Owner, ModResources.OJStarId) >= base.DynamicVars["Additional_Payment"].BaseValue;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        IEnumerable<CardModel> drawn = await CardPileCmd.Draw(choiceContext, DynamicVars.Cards.IntValue, Owner);
        List<CardModel> drawnList = drawn.ToList();

        // 丢弃其中的非攻击牌（甜点牌除外）
        List<CardModel> toDiscard = drawnList
            .Where(c => c.Type != CardType.Attack && !c.HasModKeyword(MyKeywords.Sweets))
            .ToList();
        if (toDiscard.Count > 0)
        {
            await CardCmd.Discard(choiceContext, toDiscard);
        }

        // 额外支付：抽上来的攻击牌耗能在本回合（或打出前）-1
        if (cardPlay.SecondaryResources().Activated("ojstars_charge"))
        {
            int discount = -DynamicVars.Energy.IntValue;
            foreach (CardModel attack in drawnList.Where(c => c.Type == CardType.Attack))
            {
                attack.EnergyCost.AddThisTurnOrUntilPlayed(discount);
            }
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars["Additional_Payment"].UpgradeValueBy(-2);
    }
}
