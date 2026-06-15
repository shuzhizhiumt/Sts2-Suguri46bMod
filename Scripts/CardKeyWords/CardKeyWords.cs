using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Suguri46b.Scripts.CardKeyWords;
[RegisterOwnedCardKeyword(nameof(Additional_Payment))]
[RegisterOwnedCardKeyword(nameof(Norma_Check))]

public class MyKeywords
{
    public static readonly string Additional_Payment = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Additional_Payment));
    public static readonly string Norma_Check = ModContentRegistry.GetQualifiedKeywordId(Entry.ModId, nameof(Norma_Check));
}