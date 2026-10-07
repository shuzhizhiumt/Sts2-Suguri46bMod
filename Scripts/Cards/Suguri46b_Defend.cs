using System.Collections.Generic;
using System.Threading.Tasks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.HoverTips;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interactions.RightClick;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.Powers;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Cards;

/// <summary>
///     防御：获得格挡。
///     右键（<see cref="IModRightClickableCard" />，由 RitsuLib 处理多人同步）可切换为「擦弹」：
///     擦弹获得[闪避]。切换只是本牌的模式标记，**不经过 CardCmd.Transform**，因此不算"变化牌"。
/// </summary>
[RegisterCard(typeof(Suguri46bCardPool))]
[RegisterCharacterStarterCard(typeof(Suguri46bCharacter), 4)]
public class Suguri46b_Defend : ModCardTemplate, IModRightClickableCard
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Basic;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    // 擦弹模式的文本与卡图键
    public const string GrazeTitleKey = "SUGURI46B_CARD_SUGURI46B_DEFEND_GRAZE.title";
    public const string GrazeDescriptionKey = "SUGURI46B_CARD_SUGURI46B_DEFEND_GRAZE.description";
    private const string GrazePortraitPath = "res://Suguri46b/images/cards/Suguri46b_Defend_Graze.png";

    /// <summary>当前是否处于「擦弹」模式（仅本次战斗的卡牌实例有效）。</summary>
    public bool IsGrazeMode { get; private set; }

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );
    public Suguri46b_Defend() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override HashSet<CardTag> CanonicalTags => [CardTag.Defend];
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new BlockVar(5, ValueProp.Move),
        new PowerVar<EvasionPower>(5)
    ];

    /// <summary>
    ///     悬停时提示"另一形态"的卡牌样式（防御 ↔ 擦弹）。
    ///     预览卡是本牌的展示用副本（模式取反、同步升级状态），由原版 <see cref="HoverTipFactory.FromCard(CardModel, bool)" />
    ///     渲染成卡牌样式，因此卡名/卡图/描述/数值都与真正切换后的卡面一致。
    /// </summary>
    protected override IEnumerable<IHoverTip> AdditionalHoverTips
    {
        get
        {
            CardModel? preview = CreateOtherModePreview();
            if (preview == null)
            {
                yield break;
            }
            yield return HoverTipFactory.FromCard(preview);
        }
    }

    /// <summary>创建"另一形态"的展示用副本；失败时返回 null（不显示提示）。</summary>
    private CardModel? CreateOtherModePreview()
    {
        try
        {
            // 用 MutableClone 做展示副本：不会像 CreateClone 那样注册进战斗卡表，
            // 与原版 HoverTipFactory.FromCard(upgrade) 生成升级预览的做法一致
            if (MutableClone() is not Suguri46b_Defend preview)
            {
                return null;
            }
            if (IsUpgraded && !preview.IsUpgraded)
            {
                preview.UpgradeInternal();
                preview.FinalizeUpgradeInternal();
            }
            preview.IsGrazeMode = !IsGrazeMode;
            return preview;
        }
        catch
        {
            return null;
        }
    }

    // 擦弹模式：卡名与卡图随之切换（描述由 DefendGrazeDescriptionPatch 切换）
    public override string Title
    {
        get
        {
            if (!IsGrazeMode)
            {
                return base.Title;
            }
            string title = new LocString("cards", GrazeTitleKey).GetFormattedText();
            if (!IsUpgraded)
            {
                return title;
            }
            return MaxUpgradeLevel > 1 ? $"{title}+{CurrentUpgradeLevel}" : title + "+";
        }
    }

    public override string PortraitPath => IsGrazeMode ? GrazePortraitPath : base.PortraitPath;

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.TriggerAnim(base.Owner.Creature, "Cast", base.Owner.Character.CastAnimDelay);
        if (IsGrazeMode)
        {
            await PowerCmd.Apply<EvasionPower>(
                choiceContext,
                base.Owner.Creature,
                DynamicVars["EvasionPower"].IntValue,
                base.Owner.Creature,
                this);
            return;
        }
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
    }

    // ---------- 右键切换 ----------

    /// <summary>仅手牌中的（战斗）卡牌可以切换。</summary>
    public bool CanHandleRightClickLocal(ModRightClickContext context)
    {
        return context.Model is CardModel card && card.Pile?.Type == PileType.Hand;
    }

    public Task OnRightClick(ModRightClickExecutionContext context)
    {
        if (Pile?.Type != PileType.Hand)
        {
            return Task.CompletedTask;
        }
        // 只翻转模式标记：不产生新卡、不经过变化流程（因此不计入"变化牌"）
        IsGrazeMode = !IsGrazeMode;
        RefreshHandVisuals();
        return Task.CompletedTask;
    }

    /// <summary>
    ///     刷新手牌中的卡面（卡名、卡图、描述、数值预览）。
    ///     若本牌正被悬停，同时重建悬停提示——否则"另一形态"的提示不会跟随右键切换更新，
    ///     必须把鼠标移出再移入才会变（重建等价于一次移出再移入）。
    /// </summary>
    private void RefreshHandVisuals()
    {
        NPlayerHand? hand = NPlayerHand.Instance;
        NCard? node = hand?.GetCard(this);
        if (node == null)
        {
            return;
        }
        node.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);

        // 只有当前真正被悬停的手牌才重建提示，避免凭空弹出提示
        var holder = hand?.GetCardHolder(this);
        if (holder == null || hand?.FocusedHolder != holder)
        {
            return;
        }
        // 与原版 NCardHolder.CreateHoverTips 完全相同的做法：移除旧提示集后按当前卡面重建
        NHoverTipSet.Remove(holder);
        NHoverTipSet.CreateAndShow(holder, HoverTips)?.SetAlignmentForCardHolder(holder);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(3);
        DynamicVars["EvasionPower"].UpgradeValueBy(2);
    }
}
