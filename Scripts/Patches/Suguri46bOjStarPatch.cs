using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Nodes.Combat;


[HarmonyPatch(typeof(NCombatUi),"_Ready")]

public static class Suguri46bOJStarNodeInitPatch
{
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
			__instance.AddChild(_ojStarCounter,false);
			_ojStarCounterLabel = _ojStarCounter.GetNode<RichTextLabel>("OJStarValue");
		}
	}
	public static Control GetOJStarCounter() => _ojStarCounter;
	public static RichTextLabel GetOJStarCounterLabel() => _ojStarCounterLabel;
}

[HarmonyPatch(typeof(NCombatUi), "Activate")]
public static class Suguri46bOJStarNodeVisiblePatch
{
	private static void Postfix(CombatState state)
	{
		Player player = LocalContext.GetMe(state);
		Log.Info(">>>[Suguri46bMod]cjaracter.Id is "+ player.Character.Id.ToString());

		if (player.Character.Id.ToString()=="SUGURI46B_CHARACTER_SUGURI46B_CHARACTER")
		{
			Suguri46bOJStarNodeInitPatch.GetOJStarCounter().Visible=true;
		}
	}
}
