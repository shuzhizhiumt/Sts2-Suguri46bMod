using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using STS2RitsuLib.Scaffolding.Cards.HandOutline;
using STS2RitsuLib.Combat.Rewards;
using Suguri46b.Scripts.Cards;
using Suguri46b.Scripts.Cards.Token;
using Suguri46b.Scripts.GameActions;
using STS2RitsuLib.Combat.SecondaryResources;
using Suguri46b.Scripts.Patches;
using Suguri46b.Scripts.Relics;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Rewards;

namespace Suguri46b.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Suguri46b";
    public static readonly MegaCrit.Sts2.Core.Logging.Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "core-patches");
        patcher.RegisterPatch<Dmgx2ButtonPatch>();
        patcher.RegisterPatch<ExtremeAlterationTransformPatch>();
        patcher.RegisterPatch<CostsEnergyOrStarsPatch>();
        patcher.RegisterPatch<WarpControlShouldAllowFreeTravelPatch>();
        patcher.RegisterPatch<WarpControlAfterRoomEnteredPatch>();
        patcher.RegisterPatch<LowHealthIdleAnimationPatch>();
        patcher.RegisterPatch<ForgetDiscardRedirectPatch>();
        if (!patcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed.");
        var warpControlRewardDef = ModRewardRegistry.For(ModId).RegisterOwned(
            WarpControlReward.RewardStem,
            (save, player, json) => new WarpControlReward(player, save.RewardType));
        WarpControlReward.RegisteredRewardType = warpControlRewardDef.RewardType;
        ModCardHandOutlineRegistry.Register<Affluence>(
            ModCardHandOutlineSwitchRule<Affluence>.Fixed(
                when: card => card.Pile?.Type == PileType.Hand
                    && card.CurrentUpgradeLevel < 3
                    && SecondaryResourceCmd.Get(card.Owner, ModResources.OJStarId) >= (card.CurrentUpgradeLevel + 1) * 10
                    && card.Owner?.PlayerCombatState?.Energy >= 1,
                color: Colors.Green,
                priority: 0));
        ModResources.Register();
        AffluenceUpgradeAction.Register();
        Dmgx2EnchantManagedAction.Register();
        var assembly = Assembly.GetExecutingAssembly();
        FmodStudioDeferredBankRegistration.RegisterBank("res://Suguri46b/audios/desktop/Suguri46b.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://Suguri46b/audios/GUIDs.txt");
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Accel_Hyper, Accelerator>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<Navi, Sumika>();
        
    }
}