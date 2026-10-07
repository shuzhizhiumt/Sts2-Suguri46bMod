using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

/// <summary>
///     燃烧：在你的回合开始时，
///     1) 获得 1 点力量并失去 1 点敏捷（按层数，升级后每回合 2/2）；
///     2) 若敏捷为负数，本回合额外获得"其相反数"的临时力量——**不论层数，只触发一次**。
/// </summary>
[RegisterPower]
public class BlazingPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    // 需要可叠加：升级会额外给 1 层（新效果按层数生效）
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [];

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != Owner.Player)
        {
            return;
        }
        Flash();

        // 原效果：敏捷为负时，获得其相反数的临时力量（与层数无关，只触发一次）
        int dexterity = player.Creature.GetPowerAmount<DexterityPower>();
        if (dexterity < 0)
        {
            await PowerCmd.Apply<BlazingPower_ATKUP>(
                choiceContext,
                player.Creature,
                -dexterity,
                player.Creature,
                null);
        }

        // 新增效果：每层获得 1 点力量、失去 1 点敏捷（永久）
        int stacks = (int)Amount;
        if (stacks > 0)
        {
            await PowerCmd.Apply<StrengthPower>(choiceContext, player.Creature, stacks, player.Creature, null);
            await PowerCmd.Apply<DexterityPower>(choiceContext, player.Creature, -stacks, player.Creature, null);
        }
    }
}
