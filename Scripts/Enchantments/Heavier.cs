using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Enchantments;

[RegisterEnchantment]
public class Heavier : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => false;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/enchantment/Heavier.png"
    );

    public override bool CanEnchant(CardModel card)
    {
        return true;
    }

    protected override void OnEnchant()
    {
        Card.EnergyCost.UpgradeBy(1);
        Card.AddKeyword(CardKeyword.Retain);
    }
}
