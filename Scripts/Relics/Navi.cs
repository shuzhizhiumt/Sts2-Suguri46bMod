using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.RelicPools;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Saves.Runs;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Relics;

[RegisterRelic(typeof(Suguri46bRelicPool))]
[RegisterCharacterStarterRelic(typeof(Suguri46bCharacter))]
public class Navi : ModRelicTemplate
{
	private bool ActivatedThisCombat = false;
	// 稀有度
	public override RelicRarity Rarity => RelicRarity.Starter;

	protected override IEnumerable<DynamicVar> CanonicalVars => [
		new EnergyVar(1)
	];

	public override RelicAssetProfile AssetProfile => new(
		IconPath: $"res://Suguri46b/images/relics/{GetType().Name}.png",
		IconOutlinePath: $"res://Suguri46b/images/relics/{GetType().Name}.png",
		BigIconPath: $"res://Suguri46b/images/relics/{GetType().Name}.png"
	);

	public override Task AfterRoomEntered(AbstractRoom room)
{
	
	if (room is CombatRoom)
	{
	
		ActivatedThisCombat = false;
	}
	return Task.CompletedTask;
}
   public override async Task AfterCardPlayed(PlayerChoiceContext choiceContext, CardPlay cardPlay)
	{
		if (
			CombatManager.Instance.IsInProgress        
			&& cardPlay.Card.Owner == base.Owner       
			&& cardPlay.Card.Type == CardType.Attack    
			&& !ActivatedThisCombat                    
		)
	{
		Flash();                                  
		await PlayerCmd.GainEnergy(1,base.Owner);       
		ActivatedThisCombat = true;               
	}
	}
}
