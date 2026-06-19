using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Subspace_TunnelPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
        new DynamicVar("GainOJStar",3)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>[
        HoverTipFactory.FromPower<Norma>()
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
            await SecondaryResourceCmd.Gain(Owner.Player, ModResources.OJStarId,Level*base.DynamicVars["GainOJStar"].IntValue*Amount);
        }
        else
        {
            await CardPileCmd.Draw(choiceContext,DynamicVars.Cards.IntValue,Owner.Player);
        }
    }
}