using System.Collections.Generic;
using System.Threading.Tasks;
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
///     月夜之舞：每当你的[闪避]成功挡下一次攻击，向攻击者造成你原本会受到的伤害。
///     结算时机与原版烈焰屏障（FlameBarrierPower）一致——在伤害结算之后的
///     <see cref="AfterDamageReceived" /> 中反伤，避免在伤害修改阶段嵌套造成伤害。
///     层数即倍数（多次打出会放大反伤）。
/// </summary>
[RegisterPower]
public class Dance_in_the_Moonlit_NightPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    public override async Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target != base.Owner || dealer == null || dealer.IsDead)
        {
            return;
        }
        if (!props.HasFlag(ValueProp.Move))
        {
            return;
        }
        EvasionPower? evasion = base.Owner.GetPower<EvasionPower>();
        if (evasion == null || !evasion.TryConsumeEvasion(out decimal evaded, out Creature? attacker))
        {
            return;
        }
        // 记录里的攻击者应与本次伤害的攻击者一致，避免记录错配
        if (attacker != dealer)
        {
            return;
        }

        Flash();
        await CreatureCmd.Damage(choiceContext, dealer, evaded, ValueProp.Unpowered, base.Owner);
    }
}
