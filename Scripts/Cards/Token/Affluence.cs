using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Networking.ManagedActions;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Ui.Toast;
using Suguri46b.Scripts.GameActions;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Cards.Token;

[RegisterCard(typeof(TokenCardPool))]
public class Affluence : ModCardTemplate, IModRightClickableCard
{
    private const int energyCost = 2;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Token;
    private const TargetType targetType = TargetType.AllEnemies;
    private const bool shouldShowInCardLibrary = true;
    private const int MaxUpgrade = 3;

    public override int MaxUpgradeLevel => MaxUpgrade;

    public override string Title => $"{TitleLocString.GetFormattedText()}({CurrentUpgradeLevel + 1})";

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/tokens/{GetType().Name}.webp"
    );

    public Affluence() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
		HoverTipFactory.FromPower<Norma>()
    ];
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain,CardKeyword.Exhaust];
  // —— Dynamic vars ——
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(1),
        new DynamicVar("Level", 1),
        new GoldVar(30),
        ModCardVars.ComputedDamage("ExtraDamage",8,(card, target) => DynamicVars["ExtraDamage"].BaseValue*base.Owner.Creature.GetPowerAmount<Norma>())
    ];
    // —— IModRightClickableCard ——
    bool IModRightClickableModel.CanHandleRightClickLocal(ModRightClickContext context)
    {
        // 本地预检：只检查稳定的 UI 事实，决定是否发网络请求
        if (Pile?.Type == PileType.Hand
            && CombatManager.Instance.IsInProgress
            && LocalContext.IsMe(base.Owner)
            && CurrentUpgradeLevel < MaxUpgrade)
        {
            return true;
        }
        return false;
    }
    bool IModRightClickableModel.CanExecuteRightClick(ModRightClickExecutionContext context)
    {
        // 不再需要此守卫，因为真正的验证在 AffluenceUpgradeAction.ExecutePayload 中进行
        // 保留最小验证：卡牌还在手、未升满
        if (Pile?.Type == PileType.Hand
            && CombatManager.Instance.IsInProgress
            && CurrentUpgradeLevel < MaxUpgrade)
        {
            return true;
        }
        return false;
    }
    async Task IModRightClickableModel.OnRightClick(ModRightClickExecutionContext context)
    {
        // 不在 OnRightClick 中直接修改状态！
        // 改为入队一个 ManagedNetAction，该 Action 通过 STS2 的 ActionQueue
        // 在所有客户端同步执行，避免状态分歧。
        var cost = (CurrentUpgradeLevel + 1) * 10;
        var payload = new AffluenceUpgradePayload(
            OwnerNetId: base.Owner.NetId,
            CardEntryId: base.Id.Entry,
            Cost: cost);
        RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            AffluenceUpgradeAction.Descriptor,
            payload,
            base.Owner.NetId);
        await Task.CompletedTask;
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hits = CurrentUpgradeLevel + 1;
        await DamageCmd.Attack(DynamicVars.ComputeDynamicValue("ExtraDamage"))
            .WithHitCount(hits)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(base.CombatState)
            .Execute(choiceContext);
    }

    public override async Task AfterDamageGiven(PlayerChoiceContext choiceContext, Creature? dealer, DamageResult result, ValueProp props, Creature target, CardModel? cardSource)
    {
        if (cardSource == this && result.UnblockedDamage > 0)
        {
            await PlayerCmd.GainGold(DynamicVars.Gold.IntValue, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Gold.UpgradeValueBy(5);
        base.DynamicVars["Level"].UpgradeValueBy(1);
    }
}
