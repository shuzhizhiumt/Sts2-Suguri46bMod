using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Units;

public class Suguri46bPotionPool : TypeListPotionPoolModel
{
    // 描述中使用的能量图标。大小为24x24。
     public override string? TextEnergyIconPath => "res://Suguri46b/images/energy_Suguri46b.png";
    // tooltip和卡牌左上角的能量图标。大小为74x74。
    public override string? BigEnergyIconPath => "res://Suguri46b/images/energy_Suguri46b_big.png";

    public override string EnergyColorName => "Suguri46b";
}