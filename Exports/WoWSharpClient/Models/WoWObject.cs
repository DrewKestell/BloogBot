using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWObject(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.GameObj)
    : BaseWoWObject<WoWObject>(highGuid, objectType)
    {
        public override WoWObject Clone()
        {
            var clone = new WoWObject(this.HighGuid, this.ObjectType);
            clone.CopyFrom(this);
            return clone;
        }
    }
}
