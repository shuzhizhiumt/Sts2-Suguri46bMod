using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Subspace_TunnelPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/powers/Subspace_TunnelPower.png",
        BigIconPath: "res://Suguri46b/images/powers/Subspace_TunnelPower.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new DynamicVar("GainOJStar",3)
    ];

    public bool _isOdd;

    public override async Task AfterPowerAmountChanged(PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        int Level=Owner?.GetPower<Norma>()?.Amount ?? 0;

        if(Level % 2 == 1)
        {
            _isOdd=true;
        }
        else
        {
            _isOdd=false;
        }
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if(cardPlay.Card.Enchantment == null)
        {
            return;
        }
        if(_isOdd)
        {
            int Level=Owner?.GetPower<Norma>()?.Amount ?? 0;
            await PlayerCmdExtensions.GainOJStar(Level*base.DynamicVars["GainOJStar"].BaseValue*Amount,Owner.Player);
        }
        else
        {
            await CardPileCmd.Draw(choiceContext,(int)DynamicVars.Cards.BaseValue*Amount,Owner.Player);
        }
    }
}