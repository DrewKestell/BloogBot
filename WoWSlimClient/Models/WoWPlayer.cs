namespace WoWSlimClient.Models
{
    public class WoWPlayer(byte[] lowGuid, byte[] highGuid) : WoWUnit(lowGuid, highGuid, WoWObjectType.Player)
    {
        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");

        public uint GuildId { get; internal set; }
    }
}
