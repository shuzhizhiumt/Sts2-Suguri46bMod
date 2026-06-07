using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using MegaCrit.Sts2.Core.Context;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Patches;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Extreme_AlterationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://Suguri46b/images/powers/Extreme_AlterationPower.png",
        BigIconPath: "res://Suguri46b/images/powers/Extreme_AlterationPower.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(1),
    ];
}