using STS2RitsuLib.Networking.ManagedActions;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using Suguri46b.Scripts.Enchantments;
using MegaCrit.Sts2.Core.Entities.Multiplayer;

namespace Suguri46b.Scripts.GameActions;

/// <summary>
/// 通过 RitsuLib ManagedNetAction 在多人模式下同步执行 Dmgx2 附魔选择。
/// </summary>
public static class Dmgx2EnchantManagedAction
{
    // 空消息：此 Action 不需要传输额外数据（玩家 NetId 由框架自动管理）
    public readonly record struct Message;

    private static RitsuLibManagedNetActionDescriptor<Message>? _descriptor;
    public static RitsuLibManagedNetActionDescriptor<Message> Descriptor =>
        _descriptor ?? throw new InvalidOperationException("Dmgx2EnchantManagedAction not registered.");

    public static void Register()
    {
        _descriptor = new RitsuLibManagedNetActionDescriptor<Message>(
            ModuleId: Entry.ModId,
            ActionKey: "DMGX2_ENCHANT",
            Serialize: _ => [],                    // 空消息，无需序列化
            Deserialize: _ => new Message(),
            Execute: ExecuteDmgx2Enchant,
            ActionType: GameActionType.CombatPlayPhaseOnly);
    }

    /// <summary>
    /// 请求执行 Dmgx2 附魔。自动处理单人/多人模式。
    /// </summary>
    public static bool Request(Player player)
    {
        return RitsuLibManagedNetActions.Request(
            RunManager.Instance,
            Descriptor,
            new Message(),
            player.NetId);
    }

    private static async Task ExecuteDmgx2Enchant(RitsuLibManagedNetActionContext<Message> ctx)
    {
        var prefs = new CardSelectorPrefs(CardSelectorPrefs.EnchantSelectionPrompt, 0, CardPile.MaxCardsInHand);
        var selectedCards = await CardSelectCmd.FromHand(
            prefs: prefs,
            context: ctx.PlayerChoiceContext,
            player: ctx.Player,
            filter: card => card.Type == CardType.Attack && card.Enchantment == null,
            source: null);

        if (selectedCards == null)
            return;

        foreach (var card in selectedCards)
        {
            if (card == null)
                continue;
            var enchantment = ModelDb.Enchantment<Dmgx2Enchantment>().ToMutable();
            CardCmd.Enchant(enchantment, card, 1);
        }
    }
}
