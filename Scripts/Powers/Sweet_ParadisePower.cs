using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Resources;

namespace Suguri46b.Scripts.Powers;

[RegisterPower]
public class Sweet_ParadisePower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;
    public override PowerStackType StackType => PowerStackType.Counter;
    public override PowerAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/powers/{GetType().Name}.png"
    );
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DynamicVar("GainOJStar",1)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(MyKeywords.Sweets)
    ];
    public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        if (cardPlay.Card.HasModKeyword(MyKeywords.Sweets) && cardPlay.Card.Owner.Creature==Owner)
        {
            await SecondaryResourceCmd.Gain(cardPlay.Card.Owner, ModResources.OJStarId,Amount);
        }
    }
}