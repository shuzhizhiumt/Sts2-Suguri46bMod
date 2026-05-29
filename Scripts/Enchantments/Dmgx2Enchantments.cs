using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Enchantments;

[RegisterEnchantment]
public class Dmgx2Enchantment : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => false;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/ui/combat/dmgx2.png"
    );

    public override bool CanEnchant(CardModel card)
    {
        if (card.Type == CardType.Attack)
        {
            return true;
        }
        return false;
    }

    protected override void OnEnchant()
    {
        base.Card.EnergyCost.UpgradeBy(+base.Card.EnergyCost.GetWithModifiers(CostModifiers.None));
    }

    public override decimal EnchantDamageMultiplicative(decimal originalDamage, ValueProp props)
    {
        if (!props.IsPoweredAttack())
        {
            return 1;
        }
        return 2;
    }

    public override decimal EnchantBlockAdditive(decimal originalBlock)
    {
        return Amount;
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {

    }
}
