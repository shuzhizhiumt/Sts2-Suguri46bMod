using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.CardPools;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Extensions;

namespace Suguri46b.Scripts.Cards.Token;

[RegisterCard(typeof(TokenCardPool))]
public class Mio_Christmas_Cake : ModCardTemplate
{
    private const int energyCost = 0;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Token;
    private static TargetType CardTargetType => TheCustomTargetType.AnyPlayerTarget;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/tokens/{GetType().Name}.webp"
    );
    public Mio_Christmas_Cake() : base(energyCost, type, rarity, CardTargetType, shouldShowInCardLibrary)
    {

    }
    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust,MyKeywords.Sweets];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [

    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new HealVar(5)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var target in this.GetTargets(cardPlay.Target))
        {
            await CreatureCmd.Heal(target, base.DynamicVars.Heal.IntValue);
        }
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Heal.UpgradeValueBy(2);
    }
}