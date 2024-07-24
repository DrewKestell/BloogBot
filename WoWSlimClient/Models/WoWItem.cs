


namespace WoWSlimClient.Models
{
    public class WoWItem(byte[] lowGuid, byte[] highGuid, WoWObjectType objectType = WoWObjectType.Item) : WoWGameObject(lowGuid, highGuid, objectType)
    {
        public int ItemId { get; set; }

        public int StackCount { get; set; }
        public int MaxDurability { get; set; }
        public int RequiredLevel { get; set; }

        public void Use()
        {

        }

        public ItemQuality Quality { get; set; } = ItemQuality.Poor;

        public int Durability  { get; set; }

        public int DurabilityPercentage => (int)((double)Durability / MaxDurability * 100);
    }
}
