using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Present_Thief : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public Present_Thief() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
            .SpendIfAvailable("ojstars_charge", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Additional_Payment];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<Present_ThiefPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Additional_Payment", 7)
    ];

    protected override bool ShouldGlowGoldInternal => SecondaryResourceCmd.Get(Owner, ModResources.OJStarId) >= base.DynamicVars["Additional_Payment"].BaseValue;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);

        CardModel? selected = (await CardSelectCmd.FromCombatPile(
            context: choiceContext,
            pile: PileType.Draw.GetPile(Owner),
            player: Owner,
            prefs: new CardSelectorPrefs(new LocString("card_selection", "PRESENT_THIEF"), 1),
            filter: null)).FirstOrDefault();
        if (selected == null)
        {
            return;
        }

        if (cardPlay.SecondaryResources().Activated("ojstars_charge"))
        {
            // 额外支付：立刻加入手牌
            await CardPileCmd.Add(selected, PileType.Hand);
        }
        else
        {
            // 延迟到下回合开始时加入手牌
            Present_ThiefPower? power = await PowerCmd.Apply<Present_ThiefPower>(choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
            power?.Enqueue(selected);
        }
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
