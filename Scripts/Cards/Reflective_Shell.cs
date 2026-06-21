using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Reflective_Shell : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public override IEnumerable<CardKeyword> CanonicalKeywords=>[MyKeywords.Additional_Payment];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(8,ValueProp.Move),
        new PowerVar<ReflectPower>(1),
        new DynamicVar("Additional_Payment",5),
    ];
    public Reflective_Shell() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
        this.SecondaryResourceUses()
        .SpendIfAvailable("ojstars_charge", ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
    }
    protected override bool ShouldGlowGoldInternal => SecondaryResourceCmd.Get(Owner, ModResources.OJStarId)>= base.DynamicVars["Additional_Payment"].BaseValue;
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var ledger = cardPlay.SecondaryResources();
        int blockGains = (!ledger.Activated("ojstars_charge")) ? 1 : 2;
		for (int i = 0; i < blockGains; i++)
		{
			await CreatureCmd.GainBlock(base.Owner.Creature, base.DynamicVars.Block, cardPlay);
		}
        await PowerCmd.Apply<ReflectPower>(choiceContext, base.Owner.Creature, base.DynamicVars["ReflectPower"].BaseValue, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
    }
}