using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

/// <summary>
///     性格反转力场：交换你当前的[格挡]与[闪避]数值，然后两者各获得 5 点加成（升级后 7 点）。拥有[保留]。
///     交换与"各加同量"可交换次序，结果一致：
///     目标格挡 = 原闪避 + 加成，目标闪避 = 原格挡 + 加成。
/// </summary>
[RegisterCard(typeof(Suguri46bCardPool))]
public class Reverse_Attribute_Field : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );

    public Reverse_Attribute_Field() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Retain];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromPower<EvasionPower>()
    ];

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move),
        new PowerVar<EvasionPower>(5)
    ];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var creature = base.Owner.Creature;
        int block = creature.Block;
        int evasion = creature.GetPowerAmount<EvasionPower>();

        int targetBlock = evasion + base.DynamicVars.Block.IntValue;
        int targetEvasion = block + base.DynamicVars["EvasionPower"].IntValue;

        // 格挡：按差值增减
        int blockDelta = targetBlock - block;
        if (blockDelta > 0)
        {
            await CreatureCmd.GainBlock(creature, blockDelta, ValueProp.Move, cardPlay);
        }
        else if (blockDelta < 0)
        {
            await CreatureCmd.LoseBlock(choiceContext, creature, -blockDelta, null);
        }

        // 闪避：按差值增减（降到 0 时移除能力）
        int evasionDelta = targetEvasion - evasion;
        if (evasionDelta > 0)
        {
            await PowerCmd.Apply<EvasionPower>(choiceContext, creature, evasionDelta, creature, this);
        }
        else if (evasionDelta < 0)
        {
            EvasionPower? power = creature.GetPower<EvasionPower>();
            if (power != null)
            {
                await PowerCmd.ModifyAmount(choiceContext, power, evasionDelta, creature, this);
                EvasionPower? remaining = creature.GetPower<EvasionPower>();
                if (remaining != null && creature.GetPowerAmount<EvasionPower>() <= 0)
                {
                    await PowerCmd.Remove(remaining);
                }
            }
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2);
        DynamicVars["EvasionPower"].UpgradeValueBy(2);
    }
}
