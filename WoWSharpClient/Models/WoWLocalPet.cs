using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWLocalPet(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Unit) : WoWUnit(highGuid, objectType), IWoWLocalPet
    {
        public void Attack()
        {
            throw new NotImplementedException();
        }

        public bool CanUse(string consumeShadows)
        {
            throw new NotImplementedException();
        }

        public void Cast(string consumeShadows)
        {
            throw new NotImplementedException();
        }

        public void FollowPlayer()
        {
            throw new NotImplementedException();
        }

        public bool IsHappy()
        {
            throw new NotImplementedException();
        }
    }
}
