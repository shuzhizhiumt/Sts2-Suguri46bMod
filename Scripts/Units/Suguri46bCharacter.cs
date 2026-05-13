using Godot;
using MegaCrit.Sts2.Core.Entities.Characters;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Content;
using STS2RitsuLib.Scaffolding.Godot;
using STS2RitsuLib.Scaffolding.Visuals.StateMachine;

namespace Suguri46b.Scripts.Units;

[RegisterCharacter]
public class Suguri46bCharacter : ModCharacterTemplate<Suguri46bCardPool, Suguri46bRelicPool, Suguri46bPotionPool>, IModCreatureVisualsFactory, IModCreatureCombatAnimationStateMachineFactory
{
    // 角色名称颜色
    public override Color NameColor => new(0.8987582f, 0.32446608f, 0.9800934f, 1f);
    // 能量图标轮廓颜色
    public override Color EnergyLabelOutlineColor => new(0.8987582f, 0.32446608f, 0.9800934f, 1f);
    // 地图绘制颜色
    public override Color MapDrawingColor => new(0.8987582f, 0.32446608f, 0.9800934f, 1f);

    // 人物性别（男女中立）
    public override CharacterGender Gender => CharacterGender.Feminine;

    // 初始血量和金币
    public override int StartingHp => 80;
    public override int StartingGold => 99;

    public override CharacterAssetProfile AssetProfile => CharacterAssetProfiles.Merge(
        CharacterAssetProfiles.Ironclad(),
        new(
            Scenes: new(
                // 人物模型tscn路径。
                VisualsPath: "res://Suguri46b/scenes/suguri46b_character.tscn",
                // 能量表盘tscn路径。
                EnergyCounterPath: "res://Suguri46b/scenes/suguri46b_energy_counter.tscn",
                // 商店人物场景。
                MerchantAnimPath: "res://Suguri46b/scenes/suguri46b_merchant.tscn",
                // 篝火休息场景。
                RestSiteAnimPath: "res://Suguri46b/scenes/suguri46b_rest_site.tscn"
            ),
            Ui: new(
                // 人物头像路径。
                IconTexturePath: "res://Suguri46b/images/ui/top_panel/character_icon_suguri46b.png",
                // 人物头像-锁定状态路径。
                // 人物头像2号。
                IconPath: "res://Suguri46b/scenes/suguri46b_icon.tscn",
                // 人物选择背景。
                CharacterSelectBgPath: "res://Suguri46b/scenes/suguri46b_bg.tscn",
                // 人物选择图标。
                CharacterSelectIconPath: "res://Suguri46b/images/packed/character_select/char_select_suguri46b.png",
                // 人物选择图标-锁定状态。
                //CharacterSelectLockedIconPath: "res://Suguri46b/images/suguri46_00_03.png"
                // 人物选择过渡动画。
                // CharacterSelectTransitionPath: "res://materials/transitions/ironclad_transition_mat.tres",
                // 地图上的角色标记图标、表情轮盘上的角色头像
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
            // CharacterSelectSfx: null,
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
    public override float AttackAnimDelay => 0f;
    public override float CastAnimDelay => 0f;
    public ModAnimStateMachine? TryCreateCombatAnimationStateMachine(Node visualsRoot)
    {
        var builder = ModAnimStateMachineBuilder.Create();
        builder.AddState("idle", loop: true).AsInitial();
        builder.AddState("attack", loop: false).WithNext("idle");
        builder.AddState("hit", loop: false).WithNext("idle");
        builder.AddState("cast", loop: false).WithNext("idle");
        builder.AddState("die", loop: false);
        builder
            // 待机 ←→ 攻击
            .AddBranch("idle", "Attack", "attack")
            .AddBranch("idle", "Hit", "hit")
            .AddBranch("idle", "Cast", "cast")
            .AddAnyState("Dead", "die")
            .AddAnyState("Idle", "idle");

        return builder.BuildForVisualsRoot(visualsRoot);
    }
    public override bool RequiresEpochAndTimeline => false;

    // 自动转换人物场景，让你不需要手动挂脚本。复制即可。
    protected override NCreatureVisuals? TryCreateCreatureVisuals() => RitsuGodotNodeFactories.CreateFromScenePath<NCreatureVisuals>(AssetProfile.Scenes!.VisualsPath!);

    // 初始卡组，或者在卡牌类上用RegisterCharacterStarterCard就不用写这个
    // protected override IEnumerable<StartingDeckEntry> StartingDeckEntries => [
    //     new(typeof(Suguri46bCard), 5)
    // ];

    // 初始遗物，或者在遗物类上用RegisterCharacterStarterRelic就不用写这个
    // protected override IEnumerable<Type> StartingRelicTypes => [
    //     typeof(Akabeko)
    // ];


    // 攻击建筑师的攻击特效列表
    public override List<string> GetArchitectAttackVfx() => [
        // "vfx/vfx_attack_blunt",
        // "vfx/vfx_heavy_blunt",
        // "vfx/vfx_attack_slash",
        // "vfx/vfx_bloody_impact",
        // "vfx/vfx_rock_shatter"
    ];
}