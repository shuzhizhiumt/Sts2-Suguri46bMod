using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
[RegisterCharacterStarterCard(typeof(Suguri46bCharacter), 1)]
public class Observer_of_Eternity : ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2)
    ];
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.FromKeyword(CardKeyword.Retain),
    ];

    public Observer_of_Eternity() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        List<CardPoolModel> allPools = [.. base.Owner.UnlockState.CharacterCardPools];
        IEnumerable<CardModel> AttackCards = allPools
            .SelectMany(pool => pool.GetUnlockedCards(
                base.Owner.UnlockState,
                base.Owner.RunState.CardMultiplayerConstraint))
            .Where(c => c.Type == CardType.Attack && c.Rarity == CardRarity.Common);

        List<CardModel> gainCards = [.. CardFactory.GetDistinctForCombat(
            base.Owner,
            AttackCards,
            2,
            base.Owner.RunState.Rng.CombatCardGeneration
        )];

        foreach (var card in gainCards)
        {
            if (base.IsUpgraded)
            {
                CardCmd.Upgrade(card);
            }
            CardCmd.ApplyKeyword(card, CardKeyword.Retain);
            await CardPileCmd.AddGeneratedCardToCombat(card, PileType.Hand, base.Owner);
        }
    }

    protected override void OnUpgrade()
    {
    }
}