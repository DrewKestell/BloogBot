using System;
using System.Collections.Concurrent;

namespace BloogBot
{
    public static class Wait
    {
        static readonly ConcurrentDictionary<string, Item> Items = new ConcurrentDictionary<string, Item>();
        static readonly object _lock = new object();
        
        public static bool For(string parName, int parMs, bool trueOnNonExist = false)
        {
            lock (_lock)
            {
                if (!Items.TryGetValue(parName, out Item tmpItem))
                {
                    tmpItem = new Item();
                    Items.TryAdd(parName, tmpItem);
                    return trueOnNonExist;
                }
                var elapsed = (DateTime.UtcNow - tmpItem.Added).TotalMilliseconds >= parMs;
                if (elapsed)
                {
                    Items.TryRemove(parName, out tmpItem);
                }
                return elapsed;
            }
        }

        public static void Remove(string parName)
        {
            lock (_lock)
            {
                Items.TryRemove(parName, out Item tmp);
            }
        }
        
        public static void RemoveAll()
        {
            lock (_lock)
            {
                Items.Clear();
            }
        }

        class Item
        {
            internal DateTime Added { get; } = DateTime.UtcNow;
        }
    }
}
