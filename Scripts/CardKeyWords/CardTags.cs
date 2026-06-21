using MegaCrit.Sts2.Core.Entities.Cards;
using STS2RitsuLib.CardTags;
using STS2RitsuLib.Content;
using STS2RitsuLib.Interop.AutoRegistration;

namespace Suguri46b.Scripts.CardKeyWords;

[RegisterOwnedCardTag(nameof(Pudding))]
public class MyTags
{
    public static readonly CardTag Pudding = ModContentRegistry.GetQualifiedCardTagId(Entry.ModId, nameof(Pudding)).GetModCardTag();
}