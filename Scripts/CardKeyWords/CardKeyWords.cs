using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Suguri46b.Scripts.CardKeyWords;
[RegisterOwnedCardKeyword(nameof(Additional_Payment))]
[RegisterOwnedCardKeyword(nameof(Norma_Check))]
[RegisterOwnedCardKeyword(nameof(Repeat))]
[RegisterOwnedCardKeyword(nameof(Trigger))]
[RegisterOwnedCardKeyword(nameof(Sweets),CardDescriptionPlacement = ModKeywordCardDescriptionPlacement.BeforeCardDescription)]


public class MyKeywords
{
    public static readonly CardKeyword Additional_Payment = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Additional_Payment)).GetModCardKeyword();
    public static readonly CardKeyword Norma_Check = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Norma_Check)).GetModCardKeyword();
    public static readonly CardKeyword Repeat = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Repeat)).GetModCardKeyword();
    public static readonly CardKeyword Trigger = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Trigger)).GetModCardKeyword();
    public static readonly CardKeyword Sweets = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Sweets)).GetModCardKeyword();

}