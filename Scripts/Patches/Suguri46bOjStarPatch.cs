using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Suguri46b.scenes;


namespace Suguri46b.Scripts.Patches;
public  class Suguri46bOJStarNodeInitPatch : IPatchMethod
{
    public static string PatchId=>"suguri46b_ojstar_node_init";
    public static string Description=>"Get OJStarNode";
    public static bool IsCritical=>true;

    public static ModPatchTarget[] GetTargets()=>[new(typeof(NCombatUi),"_Ready")];
    private static Control? _ojStarCounter;
    public static RichTextLabel? _ojStarCounterLabel;
    private static void Postfix(NCombatUi __instance)
    {
        Log.Info(">>>[Suguri46bMod]Initing OJStarCounter");
        var ojStar = __instance.GetNodeOrNull<Control>("OJStarCounter");
        if (ojStar == null)
        {
            _ojStarCounter = GD.Load<PackedScene>("res://Suguri46b/scenes/suguri46b_oj_star_counter.tscn").Instantiate<Control>();
            _ojStarCounter.Name = "OJStarCounter";
            __instance.AddChild(_ojStarCounter, false);
            _ojStarCounterLabel = _ojStarCounter.GetNode<RichTextLabel>("OJStarValue");
        }
    }
    public static Control GetOJStarCounter() => _ojStarCounter;
    public static RichTextLabel GetOJStarCounterLabel() => _ojStarCounterLabel;
}

public  class Suguri46bOJStarNodeVisiblePatch : IPatchMethod
{
    public static string PatchId=>"suguri46b_ojstar_node_visible";
    public static string Description=>"Visible OJStarNode";
    public static bool IsCritical=>true;

    public static ModPatchTarget[] GetTargets()=>[new(typeof(NCombatUi),"Activate")];
    private static void Postfix(CombatState state)
    {
        Player player = LocalContext.GetMe(state);
        Log.Info(">>>[Suguri46bMod]cjaracter.Id is " + player.Character.Id.ToString());
        if (player.Character.Id.ToString() == "CHARACTER.SUGURI46B_CHARACTER_SUGURI46B_CHARACTER")
        {
            Suguri46bOJStarNodeInitPatch.GetOJStarCounter().Visible = true;
        }
    }
}
