using GameData.Core.Models;

namespace GameData.Core.Interfaces
{
    public interface IWoWLocalPlayer : IWoWPlayer
    {
        Position CorpsePosition { get; }
        bool InGhostForm { get; }
        bool IsCursed { get; }
        bool IsPoisoned { get; }
        int ComboPoints { get; }
        bool IsDiseased { get; }
        string CurrentStance { get; }
        bool HasMagicDebuff { get; }
        bool TastyCorpsesNearby { get; }
        bool CanRiposte { get; }
        bool MainhandIsEnchanted { get; }
        uint Copper { get; }
        bool IsAutoAttacking { get; }
        bool CanResurrect { get; }
        bool InBattleground { get; }
        bool HasQuestTargets { get; }
    }
}
