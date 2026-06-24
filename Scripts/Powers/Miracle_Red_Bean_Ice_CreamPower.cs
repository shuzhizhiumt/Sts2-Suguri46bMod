using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Suguri46b.Scripts.Cards;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Miracle_Red_Bean_Ice_CreamPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<Miracle_Red_Bean_Ice_Cream>();

    public PowerAssetProfile AssetProfile => PowerAssetProfile.Empty;

    public string? CustomIconPath => $"res://Suguri46b/images/powers/{GetType().Name}.png";

    public string? CustomBigIconPath => $"res://Suguri46b/images/powers/{GetType().Name}.png";
}

