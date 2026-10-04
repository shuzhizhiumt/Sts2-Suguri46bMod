using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Combat.History.Entries;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Cards.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class All_Guns_at_the_Ready : ModCardTemplate
{
    private const int energyCost = 3;
    private const CardType type = CardType.Attack;
    private const CardRarity rarity = CardRarity.Rare;
    private static readonly TargetType targetType = TargetType.AnyEnemy;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );
    public All_Guns_at_the_Ready() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {

    }

    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new DamageVar(1,ValueProp.Move),
        new RepeatVar(1),
        // 本场战斗中每打出过一张攻击牌，此牌就额外造成 Repeat 次伤害
        ModCardVars.Computed("ExtraRepeat",1,card=>CombatManager.Instance.History.CardPlaysFinished.Count((CardPlayFinishedEntry e)=>e.CardPlay.Card.Type == CardType.Attack && e.CardPlay.Player == base.Owner)*DynamicVars.Repeat.IntValue)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.IntValue)
            .WithHitCount(DynamicVars["ExtraRepeat"].IntValue)
            .FromCard(this,cardPlay)
            .Targeting(cardPlay.Target!)
            .Execute(choiceContext);
    }
    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1);
    }
}
