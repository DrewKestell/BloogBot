namespace WoWSlimClient.Models
{
    public class WoWPlayer : WoWUnit
    {
        public bool IsEating => HasBuff("Food");

        public bool IsDrinking => HasBuff("Drink");
    }
}
