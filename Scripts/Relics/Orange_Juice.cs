using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Rooms;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
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
    private bool giveNorma;
    public override async Task AfterPlayerTurnStart(PlayerChoiceContext choiceContext, Player player)
    {
		int currentTurn = base.Owner.Creature.CombatState.RoundNumber;
        await PlayerCmdExtensions.GainOJStar(currentTurn,base.Owner);
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
