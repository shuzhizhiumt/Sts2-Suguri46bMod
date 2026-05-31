using System.Reflection.Emit;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Screens.CardSelection;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Patching.Models;
using STS2RitsuLib.Scaffolding.Godot.NodeAttachments;
using STS2RitsuLib.Screens;
using STS2RitsuLib.TopBar;
using STS2RitsuLib.Ui.Toast;
using static System.Net.Mime.MediaTypeNames;
using Suguri46b.Scripts.Enchantments;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using System.ComponentModel.Design.Serialization;

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
    private static async void OnDmgx2ButtonPressed()
    {

            CombatState combatState = CombatManager.Instance.DebugOnlyGetState();
            if (combatState == null)
                return;

            Player player = LocalContext.GetMe(combatState);
            if (player == null)
                return;
            CardSelectorPrefs prefs = new CardSelectorPrefs(new LocString("card_selection", "ADD_Dmgx2_Enchantment"),0,10);
            IEnumerable<CardModel> selectedCards = await CardSelectCmd.FromHand(
                prefs: prefs,
                context: new BlockingPlayerChoiceContext(),
                player: player,
                filter: card => card.Type == CardType.Attack && card.Enchantment==null,
                source: null);
            EnchantmentModel DmgEnchantment = ModelDb.Enchantment<Dmgx2Enchantment>().ToMutable();
            if (selectedCards == null)
                return;

            foreach (var card in selectedCards)
            {
                if (card == null)
                    continue;
                DmgEnchantment = ModelDb.Enchantment<Dmgx2Enchantment>().ToMutable();
			    CardCmd.Enchant(DmgEnchantment, card, 1);
            }
    }

    public static Control GetDmgx2Control() => _dmgx2control;
    public static Button GetDmgx2Button() => _dmgx2button;
}