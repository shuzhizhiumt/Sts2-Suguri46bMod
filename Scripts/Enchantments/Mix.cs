using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
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

    /// <summary>
    ///     获得「混成化」的牌同时获得[保留]：回合结束时留在手中，
    ///     从而触发 TransSelf 的"被保留时再次变化"效果。
    /// </summary>
    protected override void OnEnchant()
    {
        base.Card.AddKeyword(CardKeyword.Retain);
    }

    public override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay? cardPlay)
    {
    }
}
