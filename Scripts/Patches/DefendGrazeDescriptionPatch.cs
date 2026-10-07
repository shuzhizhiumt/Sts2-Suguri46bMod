using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.Cards;

namespace Suguri46b.Scripts.Patches;

/// <summary>
///     「擦弹」模式的卡面描述切换：<see cref="CardModel.Description" /> 决定描述文本的本地化键，
///     在这里换成擦弹的键，后续原版格式化流程（动态变量、升级预览、InCombat 等）完全不受影响。
///     注意：新增补丁必须在 Entry.cs 的 patcher.RegisterPatch 列表中注册。
/// </summary>
public class DefendGrazeDescriptionPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_defend_graze_description";
    public static string Description => "Use the Graze description while Suguri46b_Defend is in Graze mode.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() => [
        new(typeof(CardModel), "get_Description", System.Type.EmptyTypes)
    ];

    public static void Postfix(CardModel __instance, ref LocString __result)
    {
        if (__instance is Suguri46b_Defend defend && defend.IsGrazeMode)
        {
            __result = new LocString("cards", Suguri46b_Defend.GrazeDescriptionKey);
        }
    }
}
