using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;

namespace Suguri46b.Scripts.CardKeyWords;
[RegisterOwnedCardKeyword(nameof(Additional_Payment))]

public class MyKeywords
{
    public static readonly string Additional_Payment = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Additional_Payment));
}