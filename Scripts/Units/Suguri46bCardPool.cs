using Godot;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Utils;
namespace Suguri46b.Scripts.Units;

public class Suguri46bCardPool : TypeListCardPoolModel
{
    public override string Title => "Suguri46b";
    public override string EnergyColorName => "Suguri46b";

    public override string? TextEnergyIconPath => "res://Suguri46b/images/energy_Suguri46b.png";
    public override string? BigEnergyIconPath => "res://Suguri46b/images/energy_Suguri46b_big.png";

    public override Color DeckEntryCardColor => new(0.702f, 0.24f, 1f);
    public override Color EnergyOutlineColor => new(0.702f, 0.24f, 1f);
    private static readonly Material? _poolFrameMaterial = MaterialUtils.CreateRgbShaderMaterial(0.702f, 0.24f, 1f);
    public override Material? PoolFrameMaterial => _poolFrameMaterial;

    public override bool IsColorless => false;
}