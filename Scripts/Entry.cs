using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;
using Suguri46b.Scripts.Cards;
using Suguri46b.Scripts.Relics;

namespace Suguri46b.Scripts;

[ModInitializer(nameof(Init))]
public class Entry
{
    public const string ModId = "Suguri46b";
    public static readonly Logger Logger = RitsuLibFramework.CreateLogger(ModId);

    public static void Init()
    {
        var harmony = new Harmony("com.example.suguri46b");
        harmony.PatchAll();
        var assembly = Assembly.GetExecutingAssembly();
        RitsuLibFramework.EnsureGodotScriptsRegistered(assembly, Logger);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);
        RitsuLibFramework.RegisterArchaicToothTranscendenceMapping<Accel_Hyper, Accelerator>();
        RitsuLibFramework.RegisterTouchOfOrobasRefinementMapping<Navi, Sumika>();
    }
}