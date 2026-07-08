using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Enchantments;

namespace Suguri46b.Scripts.Extensions;

[RegisterSingleton]
public class RepeatCount : HookedSingletonModel
{
    public RepeatCount() : base(HookType.Combat)
    {
    }
    static IDictionary<string,int> AllCardsRepeatCount = new Dictionary<string,int>();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.Card.HasModKeyword(MyKeywords.Repeat))
        {
            return;
        }
        var key = cardPlay.Card.Id.Entry;
        if (!AllCardsRepeatCount.ContainsKey(key))
        {
            AllCardsRepeatCount.Add(key, 0);
        }
        AllCardsRepeatCount[key]++;
    }
    public override Task BeforeCombatStart()
    {
        AllCardsRepeatCount.Clear();
        return base.BeforeCombatStart();
    }
    public static int ThisCardRepeatCount(CardModel card)
    {
        if (card == null||!card.HasModKeyword(MyKeywords.Repeat))
        {
            return 0;
        }
        
        var key = card.Id.Entry;
        return AllCardsRepeatCount.ContainsKey(key) ? AllCardsRepeatCount[key] : 0;
    }
}