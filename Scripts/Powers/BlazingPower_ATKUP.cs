using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Suguri46b.Scripts.Cards;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class BlazingPower_ATKUP : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<Blazing>();

    public PowerAssetProfile AssetProfile => PowerAssetProfile.Empty;

    public string? CustomIconPath => $"res://Suguri46b/images/powers/ATK_UP.png";

    public string? CustomBigIconPath => $"res://Suguri46b/images/powers/ATK_UP.png";
}

