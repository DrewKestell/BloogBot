using BotRunner.Constants;
using PathfindingService.Models;

namespace BotRunner.Interfaces
{
    public interface ILocalPlayer : IWoWPlayer
    {
        Position CorpsePosition { get; }
        bool InGhostForm { get; }
        bool IsCursed { get; }
        bool IsPoisoned { get; }
        int ComboPoints { get; }
        int ChannelingId { get; }
        bool IsDiseased { get; }
        string CurrentStance { get; }
        bool HasMagicDebuff { get; }
        bool TastyCorpsesNearby { get; }
        bool CanRiposte { get; }
        bool MainhandIsEnchanted { get; }

        void DoEmote(Emote emote);
        void DoEmote(TextEmote emote);
        void Face(Position position);
        ulong GetBackpackItemGuid(int parSlot);
        ulong GetEquippedItemGuid(EquipSlot slot);
        uint GetManaCost(string healingTouch);
        void MoveToward(Position position, float facing);
        void RefreshSkills();
        void RefreshSpells();
        void RetrieveCorpse();
        void SetTarget(ulong guid);
        void StartMeleeAttack();
        void StartMovement(ControlBits bits);
        void SetFacing(float facing);
        void StartRangedAttack();
        void StopAllMovement();
        void StopMovement(ControlBits bits);
        void StopWand();
        bool IsSpellReady(string spellName);
        void CastSpell(string spellName, int rank = -1, bool castOnSelf = false);
        void CastSpell(uint spellId, int rank = -1, bool castOnSelf = false);
        void StartWand();
        void Turn180();
        void Jump();
    }
}
