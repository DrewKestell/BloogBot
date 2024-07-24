namespace WoWSlimClient.Models
{
    public class WoWLocalPet(byte[] lowGuid, byte[] highGuid) : WoWUnit(lowGuid, highGuid)
    {
        public void Attack()
        {

        }

        public void FollowPlayer()
        {

        }

        public bool IsHappy()
        {
            return true;
        }

        public bool CanUse(string parPetSpell)
        {
            return true;
        }

        public void Cast(string parPetSpell)
        {
            
        }
    }
}
