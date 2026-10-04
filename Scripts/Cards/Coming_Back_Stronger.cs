using System.Collections.Generic;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

/// <summary>
///     越挫越勇：造成伤害，可[额外支付]星星以先施加[易伤]。
///     带[重复]关键词：重复(2) 起伤害增加（累计）。
/// </summary>
[RegisterCard(typeof(Suguri46bCardPool))]
public class Coming_Back_Stronger : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Coming_Back_Stronger() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
        .SpendIfAvailable("ojstars_charge", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<VulnerablePower>()
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Repeat];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(9, ValueProp.Move),
        new DynamicVar("ExtraDamage",5),
        new DynamicVar("Additional_Payment",3),
        new PowerVar<VulnerablePower>(1),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RepeatCount").WithMultiplier((CardModel card, Creature? _) => RepeatCount.ThisCardRepeatCount(card)),
        // 重复(2) 已达成后的累计伤害加成：(已打出次数/2) × ExtraDamage
        ModCardVars.Computed("BonusDamage", 5, card => RepeatCount.ThisCardRepeatCount(card) / 2 * DynamicVars["ExtraDamage"].IntValue)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ledger = cardPlay.SecondaryResources();
        if (ledger.Activated("ojstars_charge"))
        {
            await PowerCmd.Apply<VulnerablePower>(choiceContext, cardPlay.Target, base.DynamicVars.Vulnerable.BaseValue, base.Owner.Creature, this);
        }
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
             .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }

    // 重复(2)：伤害增加（累计，与其他重复卡牌一致）
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (cardSource != this)
        {
            return 0;
        }
        return RepeatCount.ThisCardRepeatCount(this) / 2 * DynamicVars["ExtraDamage"].IntValue;
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2);
        DynamicVars.Vulnerable.UpgradeValueBy(1);
    }
}
