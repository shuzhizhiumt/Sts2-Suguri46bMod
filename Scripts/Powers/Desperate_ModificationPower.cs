using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Content.Patches;
using Suguri46b.Scripts.Cards;

namespace Suguri46b.Scripts.Powers;

/// <summary>
///     自暴自弃的改造：本回合获得的力量（回合结束时失效）。
///     与模组内 ATK_UP 等同一模式——继承原版 <see cref="TemporaryStrengthPower"/>，
///     由其负责施加/撤销力量；来源模型指向本卡以正确显示能力名称与提示。
/// </summary>
[RegisterPower]
public class Desperate_ModificationPower : TemporaryStrengthPower, IModPowerAssetOverrides
{
    public override AbstractModel OriginModel => ModelDb.Card<Desperate_Modification>();

    public PowerAssetProfile AssetProfile => PowerAssetProfile.Empty;

    public string? CustomIconPath => $"res://Suguri46b/images/powers/{GetType().Name}.png";

    public string? CustomBigIconPath => $"res://Suguri46b/images/powers/{GetType().Name}.png";
}
