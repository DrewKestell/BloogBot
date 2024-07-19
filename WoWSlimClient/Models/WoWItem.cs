
using static WoWSlimClient.Models.Enums;

namespace WoWSlimClient.Models
{
    public class WoWItem : WoWObject
    {

        public int ItemId { get; set; }

        public int StackCount { get; set; }

        public ItemCacheInfo Info { get; }

        public void Use()
        {

        }

        public ItemQuality Quality => Info.Quality;

        public int Durability  { get; set; }

        public int DurabilityPercentage => (int)((double)Durability / Info.MaxDurability * 100);
    }
}
