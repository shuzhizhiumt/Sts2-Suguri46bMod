using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Extensions;

namespace Suguri46b.Scripts.Powers;

/// <summary>
///     奈奈子的浮游炮：每回合开始时，选择至多 Amount 张手牌，为每张添加一个随机[附魔]。
///     卡牌的「额外支付 7 星星」在打出时结算（能力层数 +1），因此每回合额外多选 1 张。
/// </summary>
[RegisterPower]
public class Nanako_BitPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }
        int count = (int)Amount;
        if (count <= 0)
        {
            return;
        }

        CardPile hand = PileType.Hand.GetPile(player);
        List<CardModel> candidates = hand.Cards
            .Where(c => c.Enchantment == null && RandomEnchantments.CanBeEnchanted(c))
            .ToList();
        if (candidates.Count == 0)
        {
            return;
        }

        Flash();
        // source 传 null：能力不是"正在执行的模型"，传能力会让原版把选牌托盘清理挂到永不触发的事件上
        IEnumerable<CardModel> selected = await CardSelectCmd.FromHand(
            choiceContext,
            player,
            new CardSelectorPrefs(
                new LocString("card_selection", "NANAKO_BIT_ENCHANT"),
                0,
                System.Math.Min(count, candidates.Count)),
            card => card.Enchantment == null && RandomEnchantments.CanBeEnchanted(card),
            null!);

        var rng = player.RunState.Rng.CombatCardGeneration;
        foreach (CardModel card in selected.ToList())
        {
            IList<EnchantmentModel> valid = RandomEnchantments.GetValidEnchantments(card);
            if (valid.Count == 0)
            {
                continue;
            }
            CardCmd.Enchant(valid[rng.NextInt(valid.Count)], card, 1);
        }
    }
}
