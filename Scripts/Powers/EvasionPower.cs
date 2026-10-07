using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

/// <summary>
///     闪避：不会受到低于层数的[攻击伤害]（<see cref="ValueProp.Move" />）。
///     一回合后清空——与 Intangible 相同的时长处理：撑过敌方回合，
///     在该敌方回合结束时整个移除（而非逐层递减）。
///     每次成功闪避都会记录被挡下的伤害与攻击者，供「月夜之舞」等效果在伤害结算后取用。
/// </summary>
[RegisterPower]
public class EvasionPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    // 本次伤害结算中被闪避的量与攻击者（每次伤害修改开始时重置）
    private decimal? _evadedAmount;
    private Creature? _evadedAttacker;

    // 本次伤害事件是否是"未成功闪避"的攻击（伤害 ≥ 层数，未被归零）
    private bool _undodgedThisEvent;

    /// <summary>
    ///     为 true 时本次伤害计算不执行闪避归零，仅用于"纯显示"的伤害计算
    ///     （如敌人攻击意图的数字）。由 EvasionIntentDisplayPatch 在意图计算期间临时置位。
    /// </summary>
    public static bool SuppressNegation { get; set; }

    /// <summary>
    ///     只对攻击伤害生效：伤害量低于层数时直接归零（在进入格挡结算之前，因此格挡也不会被消耗）；
    ///     伤害 ≥ 层数（未成功闪避）时记录该事件，命中后失去层数。
    /// </summary>
    public override decimal ModifyDamageAdditive(Creature? target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        // 每次伤害计算先清空上一次的记录，避免预览等流程留下过期数据
        _evadedAmount = null;
        _evadedAttacker = null;
        _undodgedThisEvent = false;

        if (SuppressNegation)
        {
            return 0m;
        }
        if (target != base.Owner)
        {
            return 0m;
        }
        if (!props.HasFlag(ValueProp.Move))
        {
            return 0m;
        }
        if (amount <= 0m)
        {
            return 0m;
        }
        if (amount >= Amount)
        {
            // 未成功闪避：不归零，但命中后要失去层数
            _undodgedThisEvent = true;
            return 0m;
        }

        _evadedAmount = amount;
        _evadedAttacker = dealer;
        return -amount;
    }

    /// <summary>伤害被闪避时给出反馈（与原版 Intangible 相同）。</summary>
    public override Task AfterModifyingDamageAmount(CardModel? cardSource)
    {
        Flash();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     未成功闪避的攻击命中后失去层数（即使伤害被格挡吸收也算未闪避成功）。
    /// </summary>
    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || !_undodgedThisEvent)
        {
            return;
        }
        _undodgedThisEvent = false;
        await LoseStacks(choiceContext);
    }

    /// <summary>取出并清除最近一次成功闪避的记录（被挡下的伤害量与攻击者）。</summary>
    public bool TryConsumeEvasion(out decimal amount, out Creature? attacker)
    {
        amount = _evadedAmount ?? 0m;
        attacker = _evadedAttacker;
        _evadedAmount = null;
        _evadedAttacker = null;
        return amount > 0m;
    }

    /// <summary>
    ///     失去层数：默认为失去全部层数（移除能力）；
    ///     拥有诺玛 5 层时改为只失去一半（向下取整），层数不足 2 时仍然清空。
    /// </summary>
    private async Task LoseStacks(PlayerChoiceContext choiceContext)
    {
        bool halfOnly = base.Owner.GetPower<Norma>() is { Norma5: true };
        if (halfOnly)
        {
            int lose = (int)(Amount / 2m);
            if (lose > 0)
            {
                await PowerCmd.ModifyAmount(choiceContext, this, -lose, base.Owner, null);
                return;
            }
        }
        await PowerCmd.Remove(this);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            _evadedAmount = null;
            _evadedAttacker = null;
            _undodgedThisEvent = false;
            await LoseStacks(choiceContext);
        }
    }
}
