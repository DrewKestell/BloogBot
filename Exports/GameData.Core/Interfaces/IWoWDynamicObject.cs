using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWDynamicObject : IWoWObject
    {
        HighGuid Caster { get; }
        byte[] Bytes { get; }
        uint SpellId { get; }
        float Radius { get; }
    }
}
