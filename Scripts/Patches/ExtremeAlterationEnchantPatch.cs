using System;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.Powers;

namespace Suguri46b.Scripts.Patches;

/// <summary>
/// 魔改（Extreme_Alteration）追加效果：战斗中卡牌获得附魔后，其拥有者获得格挡。
/// 与 ExtremeAlterationTransformPatch 相同模式：框架没有"卡牌获得附魔"的全局钩子，
/// 因此在 CardCmd.Enchant 的真实应用路径上挂 Postfix。
/// 注意：新增补丁必须在 Entry.cs 的 patcher.RegisterPatch 列表中注册。
/// </summary>
public class ExtremeAlterationEnchantPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_extreme_alteration_enchant";
    public static string Description => "Gain block when a card is enchanted if the owner has Extreme Alteration power.";
    public static bool IsCritical => true;

    public static ModPatchTarget[] GetTargets() => [
        new(typeof(CardCmd), "Enchant", new[] { typeof(EnchantmentModel), typeof(CardModel), typeof(decimal) })
    ];

    public static void Postfix(EnchantmentModel enchantment, CardModel card, decimal amount, ref EnchantmentModel? __result)
    {
        try
        {
            // 附魔失败（CanEnchant 校验不通过）时不结算
            if (__result == null)
            {
                return;
            }
            var owner = card?.Owner;
            if (owner?.Creature == null)
            {
                return;
            }
            var power = owner.Creature.GetPower<Extreme_AlterationPower>();
            if (power == null)
            {
                return;
            }
            decimal stacks = power.Amount;
            if (stacks <= 0m)
            {
                return;
            }
            decimal block = 4m * stacks;
            Task task = CreatureCmd.GainBlock(owner.Creature, block, ValueProp.Unpowered, null);
            _ = task.ContinueWith(
                static t => GD.PrintErr($"[Suguri46b][EnchantPatch] GainBlock faulted: {t.Exception}"),
                TaskContinuationOptions.OnlyOnFaulted);
        }
        catch (Exception e)
        {
            GD.PrintErr($"[Suguri46b][EnchantPatch] exception: {e}");
        }
    }
}
