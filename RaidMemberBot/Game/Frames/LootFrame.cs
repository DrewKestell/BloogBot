using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Game.Frames.FrameObjects;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Mem.Hooks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RaidMemberBot.Game.Frames
{
    /// <summary>
    ///     Represents a Loot-Frame
    /// </summary>
    public sealed class LootFrame
    {
        private static volatile LootFrame _instance;
        private static readonly object lockObject = new object();
        private static volatile bool _isOpen;
        private static volatile bool _abort;
        private static volatile bool _gotCoins;
        private readonly List<LootItem> _Items = new List<LootItem>();

        /// <summary>
        ///     Number of lootable items avaible
        /// </summary>
        public readonly int LootCount;

        private readonly ConcurrentDictionary<int, int> MissingIds = new ConcurrentDictionary<int, int>();

        private LootFrame()
        {
            lock (lockObject)
            {
                try
                {
                    var tmpGuid = ObjectManager.Instance.Player.CurrentLootGuid;
                    if (tmpGuid == 0)
                    {
                        Destroy();
                        return;
                    }
                    MissingIds.Clear();
                    _Items.Clear();
                    CacheCallbacks.Instance.OnNewItemCacheCallback += ItemCallback;
                    var _gotCoins = Coins != 0;
                    var list = ThreadSynchronizer.Instance.Invoke(() =>
                    {
                        var tmpItems = new List<LootItem>();
                        if (_gotCoins)
                            tmpItems.Add(new LootItemInterface());
                        for (var i = 0; i <= 15; i++)
                        {
                            var lootSlot = _gotCoins ? i + 1 : i;

                            var itemId = (0x00B7196C + i * 0x1c).ReadAs<int>();
                            if (itemId == 0) break;
                            var entry = ObjectManager.Instance.LookupItemCacheEntry(itemId,
                                PrivateEnums.ItemCacheLookupType.None);
                            if (!entry.HasValue)
                            {
                                MissingIds.TryAdd(itemId, lootSlot);
                            }
                            else
                            {
                                var item = new LootItemInterface(lootSlot, i, itemId, ref entry);
                                tmpItems.Add(item);
                            }
                        }
                        return tmpItems;
                    });
                    LootCount = list.Count;
                    if (MissingIds.Count == 0)
                    {
                        _Items = list;
                        MissingIds.Clear();
                        CacheCallbacks.Instance.OnNewItemCacheCallback -= ItemCallback;
                        return;
                    }
                    while (MissingIds.Count != 0)
                    {
                        var guidNow = ObjectManager.Instance.Player.CurrentLootGuid;
                        if (guidNow != tmpGuid || guidNow == 0)
                        {
                            Destroy();
                            return;
                        }
                        Task.Delay(1).Wait();
                    }
                    _Items.AddRange(list);
                    _Items = _Items.OrderBy(i => i.LootSlot).ToList();

                    CacheCallbacks.Instance.OnNewItemCacheCallback -= ItemCallback;
                    list.Clear();
                    var guidNow2 = ObjectManager.Instance.Player.CurrentLootGuid;
                    if (guidNow2 != 0) return;
                    Destroy();
                }
                catch (Exception)
                {
                    Destroy();
                }
            }
        }

        /// <summary>
        ///     List with all items we can loot
        /// </summary>
        public IReadOnlyList<LootItem> Items => _Items;


        /// <summary>
        ///     Access to the currently open frame
        /// </summary>
        /// <value>
        ///     The frame.
        /// </value>
        public static LootFrame Instance => _instance;

        /// <summary>
        ///     Tells whether a Loot-Frame is open or not
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is open; otherwise, <c>false</c>.
        /// </value>
        public static bool IsOpen
        {
            get
            {
                try
                {
                    if (ObjectManager.Instance.Player.CurrentLootGuid != 0) return _isOpen;
                }
                catch
                {
                    // ignored
                }
                return false;
            }
        }

        /// <summary>
        ///     Guid of the unit we are looting right now
        /// </summary>
        public ulong LootGuid => ObjectManager.Instance.Player.CurrentLootGuid;

        /// <summary>
        ///     Number of coins to loot from the unit
        /// </summary>
        public int Coins
        {
            get
            {
                var val = 0xB71BA0.ReadAs<int>();
                return val == -1 ? 0 : val;
            }
        }

        private void ItemCallback(int parItemId)
        {
            if (!MissingIds.ContainsKey(parItemId)) return;
            var entry = ObjectManager.Instance.LookupItemCacheEntry(parItemId, PrivateEnums.ItemCacheLookupType.None);
            var lootSlot = MissingIds[parItemId];
            _Items.Add(new LootItemInterface(lootSlot, _gotCoins ? lootSlot - 1 : lootSlot, parItemId, ref entry));
            int val;
            MissingIds.TryRemove(parItemId, out val);
        }

        /// <summary>
        ///     Loots the item at slot
        /// </summary>
        /// <param name="parSlotIndex">the slot</param>
        public void LootSlot(int parSlotIndex)
        {
            if (parSlotIndex < 0) return;
            if (parSlotIndex > 15) return;
            Functions.LootSlot(parSlotIndex);
        }

        /// <summary>
        ///     Loots all items
        /// </summary>
        public void LootAll()
        {
            Functions.LootAll();
        }

        /// <summary>
        ///     Closes the loot window
        /// </summary>
        public void Close()
        {
            Lua.Instance.Execute("CloseLoot()");
        }

        internal static void Create()
        {
            try
            {
                _abort = false;
                _isOpen = false;
                var tmp = new LootFrame();
                _instance = tmp;
                _isOpen = true;
                if (!_abort && ObjectManager.Instance.Player.CurrentLootGuid != 0) return;
                _isOpen = false;
                _instance = null;
            }
            catch
            {
                _isOpen = false;
                _instance = null;
            }
        }

        internal static void Destroy()
        {
            _abort = true;
            _isOpen = false;
            _instance = null;
        }

        private class LootItemInterface : LootItem
        {
            internal LootItemInterface(int lootSlotNumber, int memLootSlotNumber, int parItemId,
                ref ItemCacheEntry? tmpInfo) : base(lootSlotNumber, memLootSlotNumber, parItemId, ref tmpInfo)
            {
            }

            internal LootItemInterface()
            {
            }
        }
    }
}
