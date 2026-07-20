using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Context;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Patches;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Entities.Players;
using STS2RitsuLib.Combat.SecondaryResources;
using Suguri46b.Scripts.Resources;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Extraordinary_SpecsPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;
    public override PowerAssetProfile AssetProfile => new(
        IconPath:$"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.ForEnergy(this)
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("Additional_Payment",10),
        new EnergyVar(1)
    ];
    public override bool TryModifyEnergyCostInCombat(CardModel card, decimal originalCost, out decimal modifiedCost)
    {
        if (card.Owner==Owner.Player && !(card.EnergyCost.GetWithModifiers(CostModifiers.None) == 0) && !card.EnergyCost.CostsX && SecondaryResourceCmd.Get(Owner.Player, ModResources.OJStarId)>= base.DynamicVars["Additional_Payment"].IntValue)
        {
            return base.TryModifyEnergyCostInCombat(card, originalCost-1, out modifiedCost);
        }
        return base.TryModifyEnergyCostInCombat(card, originalCost, out modifiedCost);
    }
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.IsAutoPlay && cardPlay.Card.EnergyCost.GetWithModifiers(CostModifiers.None) != 0  && !cardPlay.Card.EnergyCost.CostsX && cardPlay.Card.Owner==Owner.Player && SecondaryResourceCmd.Get(Owner.Player, ModResources.OJStarId)>= base.DynamicVars["Additional_Payment"].IntValue)
        {
            await SecondaryResourceCmd.Lose(Owner.Player, ModResources.OJStarId, base.DynamicVars["Additional_Payment"].IntValue);
        }
    }
}