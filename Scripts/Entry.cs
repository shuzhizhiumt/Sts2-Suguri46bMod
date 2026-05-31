using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Audio;
using STS2RitsuLib.Interop;
using STS2RitsuLib.Patching.Core;
using Suguri46b.Scripts.Cards;
using Suguri46b.Scripts.Patches;
using Suguri46b.Scripts.Relics;

namespace Suguri46b.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Suguri46b";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var patcher = RitsuLibFramework.CreatePatcher(ModId, "core-patches");
        patcher.RegisterPatch<Suguri46bOJStarNodeInitPatch>();
        patcher.RegisterPatch<Suguri46bOJStarNodeVisiblePatch>();
        patcher.RegisterPatch<Dmgx2ButtonPatch>();
        if (!patcher.PatchAll())
            throw new InvalidOperationException("Critical patches failed.");
        var assembly = Assembly.GetExecutingAssembly();
        FmodStudioDeferredBankRegistration.RegisterBank("res://Suguri46b/audios/desktop/Suguri46b.bank");
        FmodStudioDeferredBankRegistration.RegisterStudioGuidMappings("res://Suguri46b/audios/GUIDs.txt");
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Accel_Hyper, Accelerator>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<Navi, Sumika>();
    }
}