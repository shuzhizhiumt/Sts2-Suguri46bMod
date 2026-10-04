using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

/// <summary>
///     部件扩张：将自己变化为[卡组]中随机 1 张[攻击牌]的复制品。
///     [额外支付] 10 星星：改为从[卡组]中选择 1 张牌变化。
/// </summary>
[RegisterCard(typeof(Suguri46bCardPool))]
public class Extension : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Extension() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
        .SpendIfAvailable("ojstars_charge", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Additional_Payment];
    protected override bool ShouldGlowGoldInternal => SecondaryResourceCmd.Get(Owner, ModResources.OJStarId) >= base.DynamicVars["Additional_Payment"].BaseValue;
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Additional_Payment",10)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        CardPile deck = PileType.Deck.GetPile(base.Owner);
        CardModel? target;
        if (cardPlay.SecondaryResources().Activated("ojstars_charge"))
        {
            // 额外支付 10 星星：从卡组中选择 1 张牌作为变化结果
            List<CardModel> candidates = deck.Cards.ToList();
            if (candidates.Count == 0)
            {
                return;
            }
            IEnumerable<CardModel> picked = await CardSelectCmd.FromSimpleGrid(
                choiceContext,
                candidates,
                base.Owner,
                new CardSelectorPrefs(new LocString("card_selection", "EXTENSION_TRANS"), 1));
            target = picked.FirstOrDefault();
        }
        else
        {
            // 随机 1 张攻击牌
            target = base.Owner.RunState.Rng.CombatCardGeneration.NextItem(
                deck.Cards.Where(c => c.Type == CardType.Attack).ToList());
        }

        if (target == null || base.CombatState == null)
        {
            return;
        }

        // 变化为该牌的复制品：延迟到出牌结算结束（牌堆变更）后执行，避免打断出牌流程
        TransSelf.QueueTransform(this, base.CombatState.CloneCard(target));
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
