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
    static IDictionary<string,int> AllCardsRepeatCount=new Dictionary<string,int>();

    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (!cardPlay.Card.HasModKeyword(MyKeywords.Repeat))
        {
            return;
        }
        if (!AllCardsRepeatCount.ContainsKey(cardPlay.Card.Title))
        {
            AllCardsRepeatCount.Add(cardPlay.Card.Title,0);
        }
        AllCardsRepeatCount[cardPlay.Card.Title]++;
    }
    public static int ThisCardRepeatCount(CardPlay cardPlay)
    {
        return AllCardsRepeatCount.ContainsKey(cardPlay.Card.Title)?AllCardsRepeatCount[cardPlay.Card.Title]:0;
    }
}