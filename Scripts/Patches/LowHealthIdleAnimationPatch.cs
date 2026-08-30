using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Patches;

/// <summary>
/// 残血动画回血补丁。
/// STS2 1.11 的治疗逻辑（CreatureCmd.Heal）只在
/// <see cref="NCreatureVisuals.IsPlayingIdleAnimation" /> 返回 true 时补发 "Idle" 触发器，
/// 而原版该方法按 Spine 当前动画名判断，非 Spine 的模组角色会得到 false，
/// 导致回血超过 25% 后仍停留在残血待机动画。
/// 此处让须玖莉（460亿年）的状态机处于 idle / low_health_loop 时也视为
/// "正在播放待机"，游戏便会补发 "Idle"，状态机即可按当前血量切回普通待机。
/// </summary>
public partial class LowHealthIdleAnimationPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_low_health_idle_animation";
    public static string Description =>
        "Let the vanilla heal flow re-fire the Idle trigger for the mod low-HP idle loop.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() =>
        [new(typeof(NCreatureVisuals), nameof(NCreatureVisuals.IsPlayingIdleAnimation))];

    private static void Postfix(NCreatureVisuals __instance, ref bool __result)
    {
        if (__result)
            return;

        // 只对须玖莉（460亿年）生效：登记表中不存在其它角色/模组的状态机
        if (!Suguri46bCharacter.TryGetCombatAnimMachine(__instance, out var machine) || machine == null)
            return;

        if (machine.Current is { Id: "idle" or "low_health_loop" })
            __result = true;
    }
}
