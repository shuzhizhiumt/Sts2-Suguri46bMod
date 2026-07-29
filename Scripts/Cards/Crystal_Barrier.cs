using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Crystal_Barrier : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );

    public Crystal_Barrier() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
            .SpendIfAvailable("crystal_extra", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [MyKeywords.Additional_Payment];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(10, ValueProp.Move),
        new DynamicVar("Additional_Payment", 10),
    ];

    protected override bool ShouldGlowGoldInternal =>
        SecondaryResourceCmd.Get(Owner, ModResources.OJStarId) >= 10;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        decimal blockAmount = await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);

        var ledger = cardPlay.SecondaryResources();
        if (ledger.Activated("crystal_extra"))
        {
            var power = await PowerCmd.Apply<ToricToughnessPower>(
                choiceContext, base.Owner.Creature, 1, base.Owner.Creature, this);
            power?.SetBlock(blockAmount);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
    }
}
