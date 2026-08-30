using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Dangerous_Pudding : ModCardTemplate
{
    private const int energyCost = 2;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Common;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.webp"
    );

    public Dangerous_Pudding() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [
        HoverTipFactory.ForEnergy(this)
    ];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new EnergyVar(2)
    ];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        // 先结算能量获取，再施加危险布丁之力（该力量会封锁能量获取，必须后于卡牌自身的获取）
        var hand = PileType.Hand.GetPile(base.Owner);
        var attackCount = hand?.Cards.Count(c => c.Type == CardType.Attack) ?? 0;
        if (attackCount > 0)
        {
            await PlayerCmd.GainEnergy(attackCount*DynamicVars.Energy.BaseValue, base.Owner);
        }
        // 层数2 = 持续到"下回合"结束：打出当回合结束时减1，下回合结束时减1并移除
        await PowerCmd.Apply<Dangerous_PuddingPower>(choiceContext, base.Owner.Creature, 2, base.Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        base.EnergyCost.UpgradeBy(-1);
    }
}
