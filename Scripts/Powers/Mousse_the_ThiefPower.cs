using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Mousse_the_ThiefPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );

    private readonly HashSet<CardModel> _redirected = [];

    public override async Task AfterAutoPostPlayPhaseEntered(PlayerChoiceContext choiceContext, Player player)
    {
        if (player != base.Owner.Player)
        {
            return;
        }
        CardPile hand = PileType.Hand.GetPile(base.Owner.Player);
        for (int i = 0; i < Amount; i++)
        {
            List<CardModel> items = hand.Cards
            .Where(c => !c.Keywords.Contains(CardKeyword.Unplayable))
            .ToList();
            CardModel? card = base.Owner.Player.RunState.Rng.Shuffle.NextItem(items);
            if (card == null)
            {
                return;
            }
            _redirected.Add(card);
            await CardCmd.AutoPlay(choiceContext, card, null);
        }
    }

    // 以此效果打出的牌进入抽牌堆（而非弃牌堆）
    public override CardLocation ModifyCardPlayResultLocation(CardModel card, bool isAutoPlay, ResourceInfo resources, CardLocation cardLocation)
    {
        if (_redirected.Remove(card) && !card.HasModKeyword(CardKeyword.Exhaust))
        {
            return cardLocation with { pileType = PileType.Draw, position = CardPilePosition.Random };
        }
        return cardLocation;
    }
}
