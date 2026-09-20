using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Patching.Models;
using Suguri46b.Scripts.CardKeyWords;

namespace Suguri46b.Scripts.Patches;

/// <summary>
/// 「遗忘」关键词的效果弃牌改道：单卡进弃牌堆时直接改送消耗堆，
/// 因此不会播放"进入弃牌堆"的动画，也不会触发弃牌相关效果。
/// 覆盖率说明：
///   - 单卡路径（CardCmd.DiscardAndDraw 等效果弃牌、回合末手中效果牌）→ 本补丁
///   - 回合末弃置整手牌（批量 Add）→ ForgetKeywordHandler.BeforeFlush 提前消耗
///   - 兜底 → ForgetKeywordHandler.AfterCardChangedPiles
/// 注意：新增补丁必须在 Entry.cs 的 patcher.RegisterPatch 列表中注册。
/// </summary>
public class ForgetDiscardRedirectPatch : IPatchMethod
{
    public static string PatchId => "suguri46b_forget_discard_redirect";
    public static string Description => "Redirect cards with the Forget keyword from the discard pile to the exhaust pile (single-card pile adds).";
    public static bool IsCritical => false;

    public static ModPatchTarget[] GetTargets() => [
        new(typeof(CardPileCmd), "Add", new[]
        {
            typeof(CardModel), typeof(CardPile), typeof(CardPilePosition), typeof(AbstractModel), typeof(bool)
        })
    ];

    public static void Prefix(CardModel card, ref CardPile newPile)
    {
        if (card?.Owner == null)
        {
            return;
        }
        if (newPile == null || newPile.Type != PileType.Discard)
        {
            return;
        }
        if (!card.HasModKeyword(MyKeywords.Forget))
        {
            return;
        }
        // 直接改道到消耗堆：不播弃牌动画、不触发弃牌效果
        newPile = PileType.Exhaust.GetPile(card.Owner);
    }
}
