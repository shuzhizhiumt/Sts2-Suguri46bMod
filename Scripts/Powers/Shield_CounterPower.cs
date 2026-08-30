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

[RegisterPower]
public class Shield_CounterPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    // 本次格挡获取将被转化的量（在 ModifyBlockMultiplicative 中记录，AfterModifyingBlockAmount 中消费）
    private decimal? _pendingConvertedBlock;

    // 防止转化伤害触发自身格挡时递归
    private bool _converting;

    // 将获得格挡量归零（改为伤害），并记录将被转化的量。
    // 注意：卡牌数值预览（UpdateCardPreview）也会调用本方法（此时 cardPlay == null 且 cardSource != null），
    // 预览时不做转化，保持显示原始格挡数值（如"获得7点格挡"）。
    public override decimal ModifyBlockMultiplicative(Creature target, decimal block, ValueProp props, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (target != base.Owner)
        {
            return 1m;
        }
        if (block <= 0m)
        {
            return 1m;
        }
        if (_converting)
        {
            return 1m;
        }
        if (cardPlay == null && cardSource != null)
        {
            // 卡牌数值预览：不影响显示数值
            return 1m;
        }
        _pendingConvertedBlock = block;
        return 0m;
    }

    // 对所有敌人造成等量的伤害
    public override async Task AfterModifyingBlockAmount(decimal modifiedAmount, CardModel? cardSource, CardPlay? cardPlay)
    {
        if (_pendingConvertedBlock is not decimal amount || amount <= 0m)
        {
            return;
        }
        _pendingConvertedBlock = null;
        IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
        if (enemies.Count == 0)
        {
            return;
        }
        Flash();
        _converting = true;
        try
        {
            await CreatureCmd.Damage(new ThrowingPlayerChoiceContext(), enemies, amount, ValueProp.Unpowered, base.Owner);
        }
        finally
        {
            _converting = false;
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (participants.Contains(base.Owner))
        {
            _pendingConvertedBlock = null;
            await PowerCmd.Decrement(this);
        }
    }
}
