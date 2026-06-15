using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Enchantments;

[RegisterEnchantment]
public class Feather : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => false;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/enchantment/Feather.png"
    );

    public override bool CanEnchant(CardModel card)
    {
        return true;
    }

    protected override void OnEnchant()
    {
        if (Card.EnergyCost.GetAmountToSpend() > 0)
        {
            Card.EnergyCost.UpgradeBy(-1);
        }
        Card.AddKeyword(CardKeyword.Exhaust);
    }
}
