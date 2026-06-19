using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Resources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Relics;

[RegisterRelic(typeof(Suguri46bRelicPool))]
[RegisterCharacterStarterRelic(typeof(Suguri46bCharacter))]
public class Orange_Juice : ModRelicTemplate
{

    // 稀有度
    public override RelicRarity Rarity => RelicRarity.None;
    public override RelicAssetProfile AssetProfile => new(
        IconPath: $"res://Suguri46b/images/relics/{GetType().Name}.png",
        IconOutlinePath: $"res://Suguri46b/images/relics/{GetType().Name}.png",
        BigIconPath: $"res://Suguri46b/images/relics/{GetType().Name}.png"
    );
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>[
        HoverTipFactory.FromPower<Norma>()
        ];
    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        int currentTurn = base.Owner.Creature.CombatState.RoundNumber;
        if (participants.Contains(base.Owner.Creature))
        {
            Flash();
            await SecondaryResourceCmd.Gain(Owner, ModResources.OJStarId,currentTurn);
        }
    }
    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if(room is CombatRoom)
        {
            Flash();
            await PowerCmd.Apply<Norma>(new ThrowingPlayerChoiceContext(), base.Owner.Creature,1, base.Owner.Creature, null);
        }    
    }
}
