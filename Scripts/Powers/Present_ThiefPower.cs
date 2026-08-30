using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Present_ThiefPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    // 待下回合开始加入手牌的牌（多次打出可排队）
    private readonly Queue<CardModel> _pending = new();

    public void Enqueue(CardModel card)
    {
        _pending.Enqueue(card);
    }

    // 参考原版 ForegoneConclusionPower：在"下回合开始抽牌之前"交货，
    // 此时偷取的牌必然还在抽牌堆中
    public override async Task BeforeHandDraw(Player player, PlayerChoiceContext choiceContext, ICombatState combatState)
    {
        if (player != base.Owner.Player)
        {
            return;
        }
        while (_pending.Count > 0)
        {
            CardModel card = _pending.Dequeue();
            if (card.Pile?.Type == PileType.Draw)
            {
                await CardPileCmd.Add(card, PileType.Hand);
            }
        }
        await PowerCmd.Remove(this);
    }
}
