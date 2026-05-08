using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostMageBot
{
    static class FrostMageConsumables
    {
        const int MinimumConsumableCount = 2;

        // Names of the conjured food items available to mages. Sorted from lowest level to highest.
        internal static readonly string[] ConjuredFoodNames =
        {
            "Conjured Muffin",
            "Conjured Bread",
            "Conjured Rye",
            "Conjured Pumpernickel",
            "Conjured Sourdough",
            "Conjured Sweet Roll",
            "Conjured Cinnamon Roll",
            "Conjured Croissant"
        };

        // Names of the conjured drink items available to mages. Sorted from lowest level to highest.
        internal static readonly string[] ConjuredDrinkNames =
        {
            "Conjured Water",
            "Conjured Fresh Water",
            "Conjured Purified Water",
            "Conjured Spring Water",
            "Conjured Mineral Water",
            "Conjured Sparkling Water",
            "Conjured Crystal Water",
            "Conjured Mountain Spring Water",
            "Conjured Glacier Water"
        };

        internal static Selection<TItem> SelectItem<TItem>(
            IEnumerable<TItem> items,
            Func<TItem, string> getName,
            Func<TItem, int> getStackCount,
            IEnumerable<string> configuredNames,
            IEnumerable<string> conjuredNames)
        {
            var configuredNameList = configuredNames?.ToArray() ?? new string[0];
            var conjuredNameList = conjuredNames?.ToArray() ?? new string[0];
            var matchingItems = (items ?? Enumerable.Empty<TItem>())
                .Select(item => new
                {
                    Item = item,
                    Name = getName(item),
                    StackCount = Math.Max(0, getStackCount(item))
                })
                .Where(i => IsConfiguredItemName(i.Name, configuredNameList) || IsConjuredItemName(i.Name, conjuredNameList))
                .ToList();

            var selected = matchingItems
                .OrderByDescending(i => IsConfiguredItemName(i.Name, configuredNameList))
                .ThenByDescending(i => i.StackCount)
                .FirstOrDefault();

            return new Selection<TItem>(
                selected == null ? default(TItem) : selected.Item,
                matchingItems.Sum(i => i.StackCount));
        }

        internal static string[] GetConfiguredNames(string setting) =>
            string.IsNullOrWhiteSpace(setting)
                ? new string[0]
                : setting.Split('|')
                    .Select(name => name.Trim())
                    .Where(name => !string.IsNullOrEmpty(name))
                    .ToArray();

        static bool IsConfiguredItemName(string itemName, IEnumerable<string> configuredNames)
        {
            if (string.IsNullOrEmpty(itemName))
                return false;

            return configuredNames.Any(name =>
                string.Equals(itemName, name, StringComparison.OrdinalIgnoreCase) ||
                itemName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        static bool IsConjuredItemName(string itemName, IEnumerable<string> conjuredNames)
        {
            if (string.IsNullOrEmpty(itemName))
                return false;

            return conjuredNames.Any(name => string.Equals(itemName, name, StringComparison.OrdinalIgnoreCase));
        }

        internal sealed class Selection<TItem>
        {
            public Selection(TItem item, int count)
            {
                Item = item;
                Count = count;
            }

            public TItem Item { get; }

            public int Count { get; }

            public bool HasEnough => Count > MinimumConsumableCount;
        }
    }
}
