using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;

namespace Suguri46b.Scripts.Powers;

/// <summary>
///     融化的记忆：每回合结束时，按你手牌中带有[遗忘]的牌数量，
///     对所有敌人造成伤害，并获得等量的格挡。
///     使用 <see cref="BeforeSideTurnEnd"/> 而非 AfterSideTurnEnd：
///     该钩子在 BeforeFlush（遗忘牌被消耗）之前触发，因此能统计到本回合结束时仍在手牌中的遗忘牌。
/// </summary>
[RegisterPower]
public class Melting_MemoriesPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1, ValueProp.Unpowered),
        new BlockVar(1, ValueProp.Unpowered),
    ];

    public override async Task BeforeSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side != CombatSide.Player || !participants.Contains(base.Owner))
        {
            return;
        }
        var player = base.Owner.Player;
        if (player == null)
        {
            return;
        }

        int forgetCount = PileType.Hand.GetPile(player).Cards
            .Count(c => c.HasModKeyword(MyKeywords.Forget));
        if (forgetCount <= 0)
        {
            return;
        }

        decimal damage = DynamicVars.Damage.IntValue  * forgetCount;
        decimal block = DynamicVars.Block.IntValue * forgetCount;

        Flash();
        IReadOnlyList<Creature> enemies = base.CombatState.HittableEnemies;
        if (enemies.Count > 0)
        {
            await CreatureCmd.Damage(choiceContext, enemies, damage, ValueProp.Unpowered, base.Owner);
        }
        await CreatureCmd.GainBlock(base.Owner, block, ValueProp.Unpowered, null);
    }
}
