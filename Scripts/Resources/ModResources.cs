using Godot;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Models.Events;
using STS2RitsuLib;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Combat.SecondaryResources;
using Suguri46b.Scripts.Units;

namespace Suguri46b.Scripts.Resources;
public static class ModResources
{
    public static SecondaryResourceDefinition OJStarDefinition { get; private set; } = null!;
    public static string OJStarId { get; private set; } = string.Empty;
    public static void Register()
    {
        var registry = RitsuLibFramework.GetSecondaryResourceRegistry(Entry.ModId);

        OJStarDefinition = registry.Register("ojstar", new SecondaryResourceDefinition(
            defaultAmount: 0,
            baseMaxAmount: null,
            turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
            persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
            smallIconPath: "res://Suguri46b/images/resources/oj_star.png",
            largeIconPath: "res://Suguri46b/images/resources/oj_star.png"
        ));
        OJStarId = OJStarDefinition.Id;

        registry.RegisterCombatUi(
        "ojstar_combat_counter",
            parent =>
            {
                var row = NSecondaryResourceCounter.Create(OJStarDefinition, new SecondaryResourceCounterStyle
                {
                    FontSize = 32,
                    AmountLabelOffset=new Vector2(22, 22),
                    PositiveColor = Colors.Cyan,
                    FormatAmount = (amount, max) => amount.ToString(),
                    IconStyle = SecondaryResourceIconStyle.Default with
                    {
                        Size = new Vector2(90, 90),
                        HoverTip = SecondaryResourceHoverTipStyle.Default,
                    },
                });
                var energyCounter = parent.GetNode<Control>("%EnergyCounterContainer");
                row.Position = energyCounter.Position + new Vector2(70, -45);
                return row;
            },
            ctx => ctx.Node.Bind(ctx.Player)
        );

        registry.RegisterCardUi(
            "ojstar_card_ui",
            parent =>
            {
                var ui = NSecondaryResourceCardCostUi.Create(OJStarId, new SecondaryResourceCardCostUiStyle
                {
                    ReserveVanillaStarCostSlot=true,
                    IconSize = new Vector2(48,48),
                    FontSize = 24,
                });
                var energyIcon = parent.GetNode<TextureRect>("%EnergyIcon");
                ui.Position = energyIcon.Position + new Vector2(-10, 40);
                return ui;
            },
            ctx => ctx.Node.Refresh(ctx)
        );
        registry.AlwaysShowInCombatUiForCharacter<Suguri46bCharacter>(OJStarDefinition.LocalId);
        }
}