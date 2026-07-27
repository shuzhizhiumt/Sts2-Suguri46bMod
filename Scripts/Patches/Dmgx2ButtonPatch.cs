using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.GameActions;
using Suguri46b.Scripts.Powers;

namespace Suguri46b.Scripts.Patches;


public partial class Dmgx2ButtonPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_dmgx2_button";
    public static string Description => "Add Dmgx2 enchant button (multiplayer-safe via ManagedNetAction, no card selection UI).";
    public static ModPatchTarget[] GetTargets() => [new(typeof(NCombatUi), "_Ready")];
    public static bool IsCritical => false;

    public static Control? _dmgx2control;
    public static Button? _dmgx2button;

    private static void Postfix(NCombatUi __instance)
    {
        var Dmgx2 = __instance.GetNodeOrNull<Control>("Dmgx2Control");
        if (Dmgx2 == null)
        {
            _dmgx2control = GD.Load<PackedScene>("res://Suguri46b/scenes/dmgx2_button.tscn").Instantiate<Control>();
            _dmgx2control.Name = "Dmgx2Control";
            _dmgx2control.Visible = false;
            _dmgx2button = _dmgx2control.GetNode<Button>("Dmgx2Button");
            _dmgx2button.Pressed += OnDmgx2ButtonPressed;
            __instance.AddChild(_dmgx2control, false);
        }
        else if (_dmgx2control == null)
        {
            _dmgx2control = Dmgx2;
            _dmgx2button = _dmgx2control.GetNode<Button>("Dmgx2Button");
        }

        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState != null && _dmgx2control != null)
        {
            var localPlayer = LocalContext.GetMe(combatState);
            _dmgx2control.Visible = localPlayer?.Creature.GetPower<DoublePower>()?.Amount > 0;
        }
    }

    private static void OnDmgx2ButtonPressed()
    {
        var combatState = CombatManager.Instance.DebugOnlyGetState();
        if (combatState == null) return;

        var player = LocalContext.GetMe(combatState);
        if (player == null) return;

        // 检查手牌中是否有可附魔的攻击牌
        var hand = PileType.Hand.GetPile(player);
        if (hand == null) return;

        var hasValidTarget = hand.Cards.Any(
            c => c.Type == CardType.Attack && c.Enchantment == null);
        if (!hasValidTarget) return;

        // 入队 ManagedNetAction，选牌和附魔由 ExecutePayload 统一处理
        Dmgx2EnchantManagedAction.Request(player);
    }
}
