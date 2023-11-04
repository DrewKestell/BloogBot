using System;
using System.Collections.Concurrent;

namespace RaidMemberBot.Helpers
{
    /// <summary>
    ///     A class to help with waiting
    /// </summary>
    public static class Wait
    {
        // internal dictionary which stores all items
        private static readonly ConcurrentDictionary<string, Item> Items = new ConcurrentDictionary<string, Item>();
        private static readonly object _lock = new object();

        /// <summary>
        ///     Adds a waiter with the specified name
        /// </summary>
        /// <remarks>
        ///     <code>bool res = Wait.For("Testing", 5000)</code><br />
        ///     If Testing wasnt queried before Testing will be noted down<br />
        ///     On the next execution 5000 will be checked against the time in ms elapsed since the item got noted down<br />
        ///     If the time since creation is bigger than 5000 it will return true. False otherwise.
        /// </remarks>
        /// <param name="parName">Name of the waiter</param>
        /// <param name="parMs">The anmount of ms to wait</param>
        /// <param name="parAutoReset">Should the waiter be resetted after elapsing the specified anmount of ms?</param>
        /// <returns>
        ///     True if the waiter exists longer than the specified ms
        /// </returns>
        public static bool For(string parName, int parMs, bool parAutoReset = true)
        {
            lock (_lock)
            {
                Item tmpItem;
                if (!Items.TryGetValue(parName, out tmpItem))
                {
                    tmpItem = new Item(parAutoReset);
                    Items.TryAdd(parName, tmpItem);
                    return false;
                }
                // the item exists! lets check when it got created
                bool elapsed = (DateTime.UtcNow - tmpItem.Added).TotalMilliseconds >= parMs;
                // the time passed in parMs elapsed since the item creation
                // remove the item and return true
                if (elapsed && tmpItem.AutoReset)
                {
                    Items.TryRemove(parName, out tmpItem);
                }
                return elapsed;
            }
        }

        internal static bool For2(string parName, int parMs, bool trueOnNonExist)
        {
            lock (_lock)
            {
                Item tmpItem;
                if (!Items.TryGetValue(parName, out tmpItem))
                {
                    tmpItem = new Item(true);
                    Items.TryAdd(parName, tmpItem);
                    return trueOnNonExist;
                }
                // the item exists! lets check when it got created
                bool elapsed = (DateTime.UtcNow - tmpItem.Added).TotalMilliseconds >= parMs;
                // the time passed in parMs elapsed since the item creation
                // remove the item and return true
                if (elapsed && tmpItem.AutoReset)
                {
                    Items.TryRemove(parName, out tmpItem);
                    // ReSharper disable once InvertIf
                    if (trueOnNonExist)
                    {
                        tmpItem = new Item(true);
                        Items.TryAdd(parName, tmpItem);
                    }
                }
                return elapsed;
            }
        }

        /// <summary>
        ///     Removes the specified item
        /// </summary>
        /// <param name="parName">Name of the waiter item</param>
        public static void Remove(string parName)
        {
            lock (_lock)
            {
                Item tmp;
                Items.TryRemove(parName, out tmp);
            }
        }

        /// <summary>
        ///     Remove all waiter items
        /// </summary>
        public static void RemoveAll()
        {
            lock (_lock)
            {
                Items.Clear();
            }
        }

        private class Item
        {
            // the date we asked for the item with name the first time
            internal readonly DateTime Added;
            // Should we auto reset after enough time elapsed?
            internal readonly bool AutoReset;
            // constructor
            internal Item(bool parAutoReset = true)
            {
                Added = DateTime.UtcNow;
                AutoReset = parAutoReset;
            }
        }
    }
}
