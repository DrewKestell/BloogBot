using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWCorpse : IWoWGameObject
    {
        HighGuid OwnerGuid { get; }
        uint GhostTime { get; }
        CorpseType Type { get; }
        float Angle { get; }
        CorpseFlags CorpseFlags { get; }
        uint Guild { get; }
        uint[] Items { get; }
        byte[] Bytes2 { get; }
        byte[] Bytes1 { get; }

        bool IsBones();
        bool IsPvP();
    }
}