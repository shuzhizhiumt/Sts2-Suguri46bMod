using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Enchantments;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class All_Guns_at_the_Ready : ModCardTemplate
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private static readonly TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public All_Guns_at_the_Ready() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Repeat];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1,ValueProp.Move),
        new RepeatVar(1),
        new DynamicVar("IncreaseDamage",1),
        ModCardVars.Computed("ExtraRepeat",1,card=>CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e)=>e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Player == base.Owner)*DynamicVars.Repeat.IntValue),
        new CalculationBaseVar(0),
        new CalculationExtraVar(1),
        new CalculatedVar("RepeatCount").WithMultiplier((CardModel card, Creature? _) => RepeatCount.ThisCardRepeatCount(card)),
        // 重复(2) 已达成后的累计伤害加成：(已打出次数/2) × IncreaseDamage
        ModCardVars.Computed("RepeatDamage", 1, card => RepeatCount.ThisCardRepeatCount(card) / 2 * DynamicVars["IncreaseDamage"].IntValue)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .WithHitCount(DynamicVars["ExtraRepeat"].IntValue)
            .FromCard(this,cardPlay)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        // 用 cardSource 判定（预览时 cardPlay 为 null，不能用 cardPlay.Card）
        if (cardSource != this)
        {
            return 0;
        }
        return RepeatCount.ThisCardRepeatCount(this)/2*DynamicVars["IncreaseDamage"].IntValue;
    }
    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
