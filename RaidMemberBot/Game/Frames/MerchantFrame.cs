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
    ///     Represents the currently open Merchant-frame
    /// </summary>
    public sealed class MerchantFrame
    {
        private static volatile MerchantFrame _instance;
        private static readonly object lockObject = new object();
        private static volatile bool _isOpen;
        private static volatile bool _abort;


        private readonly ConcurrentDictionary<int, int> MissingIds = new ConcurrentDictionary<int, int>();


        private List<MerchantItem> _Items = new List<MerchantItem>();
        private ulong VenGuid;

        private MerchantFrame()
        {
            Console.WriteLine("Removing OnNewItem Callback");
            CacheCallbacks.Instance.OnNewItemCacheCallback -= ItemCallback;
            Console.WriteLine("Calling Refresh");
            Refresh();
        }

        /// <summary>
        ///     Access to the current Merchant-Frame-Object
        /// </summary>
        /// <value>
        ///     The frame.
        /// </value>
        public static MerchantFrame Instance => _instance;

        /// <summary>
        ///     Tells if there is an open Merchant-Frame right now
        /// </summary>
        /// <value>
        /// </value>
        public static bool IsOpen
        {
            get
            {
                try
                {
                    if (ObjectManager.Instance.Player.VendorGuid != 0) return _isOpen;
                }
                catch
                {
                    // ignored
                }
                return false;
            }
        }

        /// <summary>
        ///     Tells if the current Merchant-Frame offers a repair option
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance can repair; otherwise, <c>false</c>.
        /// </value>
        public bool CanRepair { get; private set; }

        /// <summary>
        ///     The total repair cost.
        /// </summary>
        /// <value>
        ///     The total repair cost.
        /// </value>
        public int TotalRepairCost { get; private set; }

        /// <summary>
        ///     A list of all items the Merchant-Frame is selling
        /// </summary>
        /// <value>
        ///     The items.
        /// </value>
        public IReadOnlyList<MerchantItem> Items => _Items;

        /// <summary>
        ///     The total number of items the Merchant-Frame is selling
        /// </summary>
        /// <value>
        ///     The total vendor items.
        /// </value>
        public int TotalVendorItems => 0xBDDFA8.ReadAs<int>();

        internal static void Create()
        {
            try
            {
                _abort = false;
                _isOpen = false;
                Console.WriteLine("Creating new MerchantFrame");
                MerchantFrame tmp = new MerchantFrame();
                _instance = tmp;
                _isOpen = true;
                if (!_abort && ObjectManager.Instance.Player.VendorGuid != 0)
                {
                    return;
                }
                Console.WriteLine("Abort: " + _abort);
                Console.WriteLine("VendorGuid: " + ObjectManager.Instance.Player.VendorGuid);
                _isOpen = false;
                _instance = null;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
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

        /// <summary>
        ///     Repairs all items belonging to the character
        /// </summary>
        public void RepairAll()
        {
            Lua.Instance.Execute("RepairAllItems()");
        }

        /// <summary>
        ///     Buys the item with the specified name
        /// </summary>
        /// <param name="parItemName">Name of the item.</param>
        /// <param name="parMultiplier">How many times to buy</param>
        public void BuyByName(string parItemName, int parMultiplier = 1)
        {
            foreach (MerchantItem x in Items)
            {
                if (x.Name != parItemName) continue;
                Functions.BuyVendorItem(x.VendorItemNumber, parMultiplier);
                Refresh();
                return;
            }
        }

        /// <summary>
        ///     Buys the item with the specified ID
        /// </summary>
        /// <param name="parItemId">The item ID</param>
        /// <param name="parMultiplier">How many times to buy</param>
        public void BuyById(int parItemId, int parMultiplier = 1)
        {
            foreach (MerchantItem x in Items)
            {
                if (x.ItemId != parItemId) continue;
                Functions.BuyVendorItem(x.VendorItemNumber, parMultiplier);
                Refresh();
                return;
            }
        }

        /// <summary>
        ///     Determines whether a specific item is avaible at the current Merchant-Frame
        /// </summary>
        /// <param name="parItemName">Name of the item</param>
        /// <returns></returns>
        public bool IsItemAvaible(string parItemName)
        {
            return Items.Any(x => x.Name == parItemName);
        }

        /// <summary>
        ///     Determines whether a specific item is avaible at the current Merchant-Frame
        /// </summary>
        /// <param name="parItemId">The item ID</param>
        /// <returns></returns>
        public bool IsItemAvaible(int parItemId)
        {
            return Items.Any(x => x.ItemId == parItemId);
        }

        /// <summary>
        ///     Vendors an item by guid
        /// </summary>
        /// <param name="guid">The guid</param>
        /// <param name="itemCount">The number of items to sell (not stacks)</param>
        public void VendorByGuid(ulong guid, uint itemCount = 1)
        {
            Functions.SellItem(itemCount, guid);
        }

        private void Refresh()
        {
            lock ("MerchantLock")
            {
                try
                {
                    VenGuid = ObjectManager.Instance.Player.VendorGuid;
                    if (VenGuid == 0)
                    {
                        Console.WriteLine("Vendor Guid is 0 ... destroying");
                        Destroy();
                        return;
                    }
                    MissingIds.Clear();
                    _Items.Clear();
                    List<MerchantItem> tmpItems = new List<MerchantItem>();
                    Console.WriteLine("Adding OnNewItem callback");
                    CacheCallbacks.Instance.OnNewItemCacheCallback += ItemCallback;
                    int tmpTotal = TotalVendorItems;
                    for (int i = 0; i < tmpTotal; i++)
                    {
                        if (ObjectManager.Instance.Player.VendorGuid != VenGuid || VenGuid == 0 ||
                            tmpTotal != TotalVendorItems)
                        {
                            Destroy();
                            return;
                        }
                        int itemNumber = i + 1;
                        int itemId = (0x00BDD11C + i * 0x1C).ReadAs<int>();
                        ItemCacheEntry? entry = ObjectManager.Instance.LookupItemCacheEntry(itemId,
                            PrivateEnums.ItemCacheLookupType.Vendor);
                        if (!entry.HasValue)
                        {
                            MissingIds.TryAdd(itemId, itemNumber);
                        }
                        else
                        {
                            MerchantItem item = new MerchantItemInterface(itemNumber, itemId, ref entry);
                            tmpItems.Add(item);
                        }
                    }
                    if (MissingIds.Count == 0)
                    {
                        _Items = tmpItems;
                        MissingIds.Clear();
                        CacheCallbacks.Instance.OnNewItemCacheCallback -= ItemCallback;
                    }
                    while (MissingIds.Count != 0)
                    {
                        if (ObjectManager.Instance.Player.VendorGuid != VenGuid || VenGuid == 0)
                        {
                            Destroy();
                            return;
                        }
                        Task.Delay(1).Wait();
                    }
                    _Items.AddRange(tmpItems);
                    _Items = _Items.OrderBy(i => i.VendorItemNumber).ToList();


                    string[] result = Lua.Instance.ExecuteWithResult(
                        "{0} = CanMerchantRepair() {1} = GetRepairAllCost()");
                    CanRepair = result[0] == "1";
                    TotalRepairCost = Convert.ToInt32(result[1]);

                    CacheCallbacks.Instance.OnNewItemCacheCallback -= ItemCallback;
                    tmpItems.Clear();
                    if (ObjectManager.Instance.Player.VendorGuid == VenGuid && VenGuid != 0) return;
                    Destroy();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Exception on Refresh: " + e);
                    Destroy();
                }
            }
        }

        private void ItemCallback(int parItemId)
        {
            if (!MissingIds.ContainsKey(parItemId)) return;
            ItemCacheEntry? entry = ObjectManager.Instance.LookupItemCacheEntry(parItemId, PrivateEnums.ItemCacheLookupType.Vendor);
            _Items.Add(new MerchantItemInterface(MissingIds[parItemId], parItemId, ref entry));
            int val;
            MissingIds.TryRemove(parItemId, out val);
        }

        private class MerchantItemInterface : MerchantItem
        {
            internal MerchantItemInterface(int parVendorItemNumber, int parItemId, ref ItemCacheEntry? entry)
                : base(parVendorItemNumber, parItemId, ref entry)
            {
            }
        }
    }
}
