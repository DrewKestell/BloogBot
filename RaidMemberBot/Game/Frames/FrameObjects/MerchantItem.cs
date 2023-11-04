
using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Mem;

namespace RaidMemberBot.Game.Frames.FrameObjects
{
    /// <summary>
    ///     Represents an item buyable at a vendor
    /// </summary>
    public abstract class MerchantItem
    {
        internal MerchantItem(int parVendorItemNumber, int parItemId, ref ItemCacheEntry? tmpInfo)
        {
            VendorItemNumber = parVendorItemNumber;
            // ReSharper disable once PossibleInvalidOperationException
            ItemId = parItemId;
            // ReSharper disable once PossibleInvalidOperationException
            Info = tmpInfo.Value;
            int ptr = 0xBDD118 + 0x1C * (parVendorItemNumber - 1);
            Price = (ptr + 0x10).ReadAs<int>();
            Quantity = (ptr + 0x18).ReadAs<int>();
            NumAvaible = (ptr + 0xC).ReadAs<int>();

            //Lua.Instance.Execute("_, _, zzPrice, zzQuantity, zzNumAvailable, zzCanUse = GetMerchantItemInfo(" +
            //               VendorItemNumber + ");");
            //Price = Convert.ToInt32(Functions.GetText("zzPrice"));
            //Quantity = Convert.ToInt32(Functions.GetText("zzQuantity"));
            //NumAvaible = Math.Abs(Convert.ToInt32(Functions.GetText("zzNumAvailable")));
            CanUse = Functions.CanUseItem(ItemId, PrivateEnums.ItemCacheLookupType.Vendor);
            //Functions.GetText("zzCanUse") == "1";
        }

        /// <summary>
        ///     The index of the item inside the Merchant Frame
        /// </summary>
        /// <value>
        ///     The vendor item number.
        /// </value>
        public int VendorItemNumber { get; }

        /// <summary>
        ///     The ID of the item
        /// </summary>
        /// <value>
        ///     The item identifier.
        /// </value>
        public int ItemId { get; }

        /// <summary>
        ///     The name of the item
        /// </summary>
        /// <value>
        ///     The name.
        /// </value>
        public string Name => Info.Name;

        /// <summary>
        ///     The price for the item in copper
        /// </summary>
        /// <value>
        ///     The price.
        /// </value>
        public int Price { get; private set; }

        /// <summary>
        ///     The number of item we buy with one (Arrows: 200x, Food: 5x etc.)
        /// </summary>
        /// <value>
        ///     The quantity.
        /// </value>
        public int Quantity { get; private set; }

        /// <summary>
        ///     The number of avaible Items
        /// </summary>
        /// <value>
        ///     The number avaible.
        /// </value>
        public int NumAvaible { get; private set; }

        /// <summary>
        ///     Can our character use the item being sold?
        /// </summary>
        /// <value>
        /// </value>
        public bool CanUse { get; private set; }

        /// <summary>
        ///     Information about the Merchant Item
        /// </summary>
        /// <value>
        ///     The information.
        /// </value>
        public ItemCacheEntry Info { get; }
    }
}
