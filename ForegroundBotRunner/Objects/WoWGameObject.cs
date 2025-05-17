using GameData.Core.Enums;
using GameData.Core.Models;

namespace ForegroundBotRunner.Objects
{
    public class WoWGameObject : WoWObject
    {
        internal WoWGameObject(
            nint pointer,
            HighGuid guid,
            WoWObjectType objectType)
            : base(pointer, guid, objectType)
        {
        }
    }
}
