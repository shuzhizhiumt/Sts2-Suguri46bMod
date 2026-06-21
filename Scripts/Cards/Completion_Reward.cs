using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Models.Capabilities;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Completion_Reward : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CalculatedVar("GainOJStar").WithMultiplier((CardModel card, Creature? _) => CombatManager.Instance.History.Entries.OfType<CardGeneratedEntry>().Count((CardGeneratedEntry c) => c.HappenedThisTurn(card.CombatState) && c.Creator == card.Owner)),
        new CalculationBaseVar(0),
        new CalculationExtraVar(7)
    ];

    public Completion_Reward() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SecondaryResourceCmd.Gain(Owner, ModResources.OJStarId,(int)((CalculatedVar)base.DynamicVars["GainOJStar"]).Calculate(cardPlay.Target));
    }
    protected override void OnUpgrade()
    {
        this.AddKeyword(MyKeywords.Norma_Check.GetModCardKeyword());
    }
}