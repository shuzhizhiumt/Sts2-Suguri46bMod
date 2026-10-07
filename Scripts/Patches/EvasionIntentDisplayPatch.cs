using System.Collections.Generic;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.MonsterMoves.Intents;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.Powers;

namespace Suguri46b.Scripts.Patches;

/// <summary>
///     敌人攻击意图的伤害数字不应被「闪避」改写成 0：意图展示的是敌人的攻击力（默认伤害），
///     闪避只应在实际结算时把伤害归零。
///     因此在意图计算期间临时关闭闪避的归零逻辑并恢复。
///     安全性：AttackIntent.GetSingleDamage 及其调用方（GetTotalDamage / GetIntentLabel / GetTexture）
///     只用于意图文本、图标与动画，不参与实际伤害结算，因此不会影响闪避的真实效果。
///     注意：新增补丁必须在 Entry.cs 的 patcher.RegisterPatch 列表中注册。
/// </summary>
public class EvasionIntentDisplayPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_evasion_intent_display";
    public static string Description => "Keep enemy attack intent damage display unaffected by the Evasion power.";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() => [
        new(typeof(AttackIntent), nameof(AttackIntent.GetSingleDamage), new[]
        {
            typeof(IEnumerable<Creature>), typeof(Creature)
        })
    ];

    public static void Prefix(out bool __state)
    {
        __state = EvasionPower.SuppressNegation;
        EvasionPower.SuppressNegation = true;
    }

    public static void Postfix(bool __state)
    {
        EvasionPower.SuppressNegation = __state;
    }

    /// <summary>
    ///     兜底恢复：即使原方法抛异常（Postfix 不会执行），Finalizer 仍会运行，
    ///     避免 SuppressNegation 残留为 true 导致闪避永久失效。重复恢复是幂等的。
    /// </summary>
    public static void Finalizer(bool __state)
    {
        EvasionPower.SuppressNegation = __state;
    }
}
