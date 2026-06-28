using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace Suguri46b.Scripts.Units;

[RegisterCharacter]
public class Suguri46bCharacter : ModCharacterTemplate<Suguri46bCardPool, Suguri46bRelicPool, Suguri46bPotionPool>
{
    public override Color NameColor => new(0.702f, 0.24f, 1f);
    public override Color EnergyLabelOutlineColor => new(0.702f, 0.24f, 1f);
    public override Color MapDrawingColor => new(0.702f, 0.24f, 1f);

    public override CharacterGender Gender => CharacterGender.Feminine;

    public override int StartingHp => 60;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                VisualsPath: "res://Suguri46b/scenes/suguri46b_character.tscn",
                EnergyCounterPath: "res://Suguri46b/scenes/suguri46b_energy_counter.tscn",
                MerchantAnimPath: "res://Suguri46b/scenes/suguri46b_merchant.tscn",
                RestSiteAnimPath: "res://Suguri46b/scenes/suguri46b_rest_site.tscn"
            ),
            VisualCues: ModVisualCues.CueSet()
            .Single("idle", "res://Suguri46b/images/units/suguri46_00_00.png")
            .Single("hit", "res://Suguri46b/images/units/suguri46_00_02.png",0.5f)
            .Sequence("attack", seq => seq
                .Frame("res://Suguri46b/images/units/suguri46_00_01.png", 0.5f)
                .Frame("res://Suguri46b/images/units/suguri46_00_03.png", 0.4f))
            .Single("dead", "res://Suguri46b/images/units/suguri46_00_04.png")
            .Single("cast", "res://Suguri46b/images/units/suguri46_00_05.png",0.5f)
            .Single("relaxed", "res://Suguri46b/images/units/suguri46_00_00.png")
            .Build(), 
            Ui: new(
                IconTexturePath: "res://Suguri46b/images/ui/top_panel/character_icon_suguri46b.png",
                IconPath: "res://Suguri46b/scenes/suguri46b_icon.tscn",
                CharacterSelectBgPath: "res://Suguri46b/scenes/suguri46b_bg.tscn",
                CharacterSelectIconPath: "res://Suguri46b/images/packed/character_select/char_select_suguri46b.png",
                MapMarkerPath: "res://Suguri46b/images/ui/top_panel/character_icon_suguri46b.png"
            ),
            Vfx: new(
            // 卡牌拖尾场景。
            // TrailPath: "res://scenes/vfx/card_trail_ironclad.tscn"
            ),
            Audio: new(
            // 攻击音效
            // AttackSfx: null,
            // 施法音效
            // CastSfx: null,
            // 死亡音效
            // DeathSfx: null,
            // 角色选择音效
            CharacterSelectSfx: "event:/suguri46b/sfx/select_character_suguri46b"
            // 过渡音效
            // CharacterTransitionSfx: "event:/sfx/ui/wipe_ironclad"
            ),
            Multiplayer: new(
            // 多人模式-手指。
            // ArmPointingTexturePath: null,
            // 多人模式剪刀石头布-石头。
            // ArmRockTexturePath: null,
            // 多人模式剪刀石头布-布。
            // ArmPaperTexturePath: null,
            // 多人模式剪刀石头布-剪刀。
            // ArmScissorsTexturePath: null
            )));
    // 攻击和施法动画延迟，以对齐动画
    public override float AttackAnimDelay => 0.1f;
    public override float CastAnimDelay => 0.1f;
    protected override ModAnimStateMachine? SetupCustomCombatAnimationStateMachine(
        Node visualsRoot,
        CharacterModel character)
    {
        return ModAnimStateMachines.StandardCue(
            visualsRoot,
            character,
            "idle",
            "dead",false,
            "hit",false,
            "attack",false,
            "cast",false,
            "relaxed",false,
            VisualCues);
            
    }
    public override bool RequiresEpochAndTimeline => false;
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>("res://Suguri46b/scenes/suguri46b_character.tscn");

    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        // "vfx/vfx_attack_blunt",
        // "vfx/vfx_heavy_blunt",
        // "vfx/vfx_attack_slash",
        // "vfx/vfx_bloody_impact",
        // "vfx/vfx_rock_shatter"
    ];
    
}