using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Enchantments;

[RegisterEnchantment]
public class Mix : ModEnchantmentTemplate
{
    public override bool HasExtraCardText => false;

    public override EnchantmentAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/enchantment/{GetType().Name}.png"
    );

    public override bool CanEnchant(CardModel card)
    {
        if (!card.HasModKeyword(CardKeyword.Unplayable))
        {
            return true;
        }
        return false;
    }

    protected override void OnEnchant()
    {
        
    }
    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
    }
}
