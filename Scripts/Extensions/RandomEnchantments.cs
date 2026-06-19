using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;

namespace Suguri46b.Scripts.Extensions;

public static class RandomEnchantments
{
    public static IList<EnchantmentModel> GetValidEnchantments(CardModel card)
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

    public static IEnumerable<Type> GetAllEnchantmentTypes()
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

    public static EnchantmentModel? GetEnchantmentModel(Type enchantmentType, bool mutable = false)
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

    public static bool CanBeEnchanted(CardModel card)
    {
        return GetAllEnchantmentTypes().Any(enchantmentType =>
        {
            var enchantment = GetEnchantmentModel(enchantmentType);
            return enchantment != null && enchantment.CanEnchant(card) && IsAllowedCardForEnchantment(enchantmentType, card);
        });
    }

    public static bool IsAllowedCardForEnchantment(Type enchantmentType, CardModel card)
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

    public static bool IsExcludedEnchantment(Type enchantmentType)
    {
        var name = enchantmentType.Name;
        return name.Equals("Goopy", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Imbued", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Clone", StringComparison.OrdinalIgnoreCase)
            || name.Equals("DeprecatedEnchantment", StringComparison.OrdinalIgnoreCase)
            || name.Equals("MockFreeEnchantment", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsInkyEnchantment(Type enchantmentType)
    {
        var name = enchantmentType.Name;
        return name.Equals("Inky", StringComparison.OrdinalIgnoreCase);
    }
}