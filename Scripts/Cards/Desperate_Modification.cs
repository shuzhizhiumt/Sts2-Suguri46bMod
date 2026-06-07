using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Enchantments;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Factories;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using System.Reflection;
using Suguri46b.Scripts.Enchantments;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Keywords;
using STS2RitsuLib.Scaffolding.Content;
using Suguri46b.Scripts.CardKeyWords;
using Suguri46b.Scripts.Extensions;
using Suguri46b.Scripts.Units;
using Godot;

namespace Suguri46b.Scripts.Cards;

[RegisterCard(typeof(Suguri46bCardPool))]
public class Extension: ModCardTemplate
{
    private const int energyCost = 1;
    private const CardType type = CardType.Skill;
    private const CardRarity rarity = CardRarity.Uncommon;
    private const TargetType targetType = TargetType.Self;
    private const bool shouldShowInCardLibrary = true;

    public override CardAssetProfile AssetProfile => new(
        PortraitPath: $"res://Suguri46b/images/cards/{GetType().Name}.png"
    );
    public Extension() : base(energyCost, type, rarity, targetType, shouldShowInCardLibrary)
    {
    }
    protected override IEnumerable<DynamicVar> CanonicalVars => [
        new CardsVar(2)];
    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var selectedCards = await CardSelectCmd.FromHand(
            prefs: new CardSelectorPrefs(new LocString("card_selection", "ADD_RANDOM_ENCHANTMENT"), 0, (int)DynamicVars.Cards.BaseValue),
            context: choiceContext,
            player: Owner,
            filter: card => card.Enchantment == null && CanBeEnchanted(card),
            source: this);

        if (selectedCards == null)
        {
            return;
        }

        var rng = Owner.RunState.Rng.CombatCardGeneration;
        foreach (var selectedCard in selectedCards)
        {
            var validEnchantments = GetValidEnchantments(selectedCard);
            if (validEnchantments.Count == 0)
            {
                continue;
            }

            var chosenEnchantment = validEnchantments[rng.NextInt(validEnchantments.Count)];
            GD.Print("[Debug]chosenEnchantment:"+chosenEnchantment);
            CardCmd.Enchant(chosenEnchantment, selectedCard, 1);
        }
    }

    private static IList<EnchantmentModel> GetValidEnchantments(CardModel card)
    {
        var valid = new List<EnchantmentModel>();
        foreach (var enchantmentType in GetAllEnchantmentTypes())
        {
            var enchantment = GetEnchantmentModel(enchantmentType, mutable: true);
            if (enchantment != null && enchantment.CanEnchant(card) && IsAllowedCardForEnchantment(enchantmentType, card))
            {
                valid.Add(enchantment);
            }
        }

        return valid;
    }

    private static IEnumerable<Type> GetAllEnchantmentTypes()
    {
        var enchantmentBaseType = typeof(EnchantmentModel);
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type[] types;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                types = ex.Types.Where(t => t != null).Cast<Type>().ToArray();
            }

            foreach (var type in types)
            {
                if (type == null || !type.IsClass || type.IsAbstract)
                {
                    continue;
                }

                if (!enchantmentBaseType.IsAssignableFrom(type))
                {
                    continue;
                }

                yield return type;
            }
        }
    }

    private static EnchantmentModel? GetEnchantmentModel(Type enchantmentType, bool mutable = false)
    {
        var method = typeof(ModelDb).GetMethod("Enchantment", BindingFlags.Public | BindingFlags.Static);
        if (method == null || !method.IsGenericMethodDefinition)
        {
            return null;
        }

        var generic = method.MakeGenericMethod(enchantmentType);
        var model = generic.Invoke(null, null) as EnchantmentModel;
        if (model == null)
        {
            return null;
        }

        if (!mutable)
        {
            return model;
        }

        var toMutable = typeof(EnchantmentModel).GetMethod("ToMutable", BindingFlags.Public | BindingFlags.Instance);
        if (toMutable == null)
        {
            return model;
        }

        return toMutable.Invoke(model, null) as EnchantmentModel ?? model;
    }

    private static bool CanBeEnchanted(CardModel card)
    {
        return GetAllEnchantmentTypes().Any(enchantmentType =>
        {
            var enchantment = GetEnchantmentModel(enchantmentType);
            return enchantment != null && enchantment.CanEnchant(card) && IsAllowedCardForEnchantment(enchantmentType, card);
        });
    }

    private static bool IsAllowedCardForEnchantment(Type enchantmentType, CardModel card)
    {
        if (IsExcludedEnchantment(enchantmentType))
        {
            return false;
        }

        if (IsInkyEnchantment(enchantmentType))
        {
            return card.Type == CardType.Attack;
        }
        return true;
    }

    private static bool IsExcludedEnchantment(Type enchantmentType)
    {
        var name = enchantmentType.Name;
        return name.Equals("Goopy", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Imbued", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Clone", StringComparison.OrdinalIgnoreCase)
            || name.Equals("DeprecatedEnchantment", StringComparison.OrdinalIgnoreCase)
            || name.Equals("MockFreeEnchantment", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsInkyEnchantment(Type enchantmentType)
    {
        var name = enchantmentType.Name;
        return name.Equals("Inky", StringComparison.OrdinalIgnoreCase);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Cards.UpgradeValueBy(1);
    }
}