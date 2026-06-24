using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Reverse_Attribute_Field : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2),
        new BlockVar(6,ValueProp.Move)
    ];

    public Reverse_Attribute_Field() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        int atk=Owner.Creature.GetPowerAmount<StrengthPower>();
        int dex =Owner.Creature.GetPowerAmount<DexterityPower>();
        if (atk>=0)
        {
            await PowerCmd.Apply<Reverse_Attribute_Field_ATKDROP>(choiceContext, base.Owner.Creature,-(atk*2), base.Owner.Creature, null);
        }
        else
        {
            await PowerCmd.Apply<Reverse_Attribute_Field_ATKUP>(choiceContext, base.Owner.Creature,-(atk*2), base.Owner.Creature, null);
        }
        if (dex>=0)
        {
            await PowerCmd.Apply<Reverse_Attribute_Field_DEFDROP>(choiceContext, base.Owner.Creature,-(dex*2), base.Owner.Creature, null);
        }
        else
        {
            await PowerCmd.Apply<Reverse_Attribute_Field_DEFUP>(choiceContext, base.Owner.Creature,-(dex*2), base.Owner.Creature, null);
        }
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    protected override void OnUpgrade()
    {
        base.DynamicVars.Block.UpgradeValueBy(3);
    }
}