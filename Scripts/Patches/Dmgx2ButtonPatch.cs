using System.Reflection.Emit;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using STS2RitsuLib.Screens;
using STS2RitsuLib.TopBar;
using STS2RitsuLib.Ui.Toast;
using static System.Net.Mime.MediaTypeNames;

namespace Suguri46b.Scripts.Patches;

public partial class Dmgx2ButtonPatch : IPatchMethod
{
    public static string PatchId=>"suguri46b_dmgx2_button";
    public static string Description=>"Get Dmgx2Button";
    public static ModPatchTarget[] GetTargets()=>[new(typeof(NCombatUi),"_Ready")];
    public static bool IsCritical=>true;
    public static Control? _dmgx2control;
    public static Button? _dmgx2button;
    private static void Postfix(NCombatUi __instance)
    {
        var Dmgx2 = __instance.GetNodeOrNull<Control>("Dmgx2Control");
        if (Dmgx2 == null)
        {
            _dmgx2control = GD.Load<PackedScene>("res://Suguri46b/scenes/dmgx2_button.tscn").Instantiate<Control>();
            _dmgx2control.Name="Dmgx2Control";
            _dmgx2button=_dmgx2control.GetNode<Button>("Dmgx2Button");
            _dmgx2button.Pressed += OnDmgx2ButtonPressed;
            __instance.AddChild(_dmgx2control, false);
        }
    }
    private static void OnDmgx2ButtonPressed()
    {
        // 这里是按钮被按下时的逻辑
        // 你可以在这里执行任何你想要的操作，比如调用某个方法，修改某个变量等等
        GD.Print("Dmgx2 Button Pressed!");
        // 例如，你可以在这里禁用按钮
        _dmgx2button.Disabled = true;
    }
    
    public static Control GetDmgx2Control() => _dmgx2control;
    public static Button GetDmgx2Button() => _dmgx2button;
}
