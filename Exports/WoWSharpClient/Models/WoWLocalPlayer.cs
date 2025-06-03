using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;

namespace WoWSharpClient.Models
{
    public class WoWLocalPlayer(HighGuid highGuid, WoWObjectType objectType = WoWObjectType.Player)
        : WoWPlayer(highGuid, objectType),
            IWoWLocalPlayer
    {
        public Position CorpsePosition => throw new NotImplementedException();
        public bool InGhostForm => throw new NotImplementedException();
        public bool IsCursed => throw new NotImplementedException();
        public bool IsPoisoned => throw new NotImplementedException();
        public int ComboPoints => throw new NotImplementedException();
        public bool IsDiseased => throw new NotImplementedException();
        public string CurrentStance => throw new NotImplementedException();
        public bool HasMagicDebuff => throw new NotImplementedException();
        public bool TastyCorpsesNearby => throw new NotImplementedException();
        public bool CanRiposte => throw new NotImplementedException();
        public bool MainhandIsEnchanted => throw new NotImplementedException();
        public uint Copper => throw new NotImplementedException();
        public bool IsAutoAttacking => throw new NotImplementedException();
        public bool CanResurrect => throw new NotImplementedException();
        public bool InBattleground => throw new NotImplementedException();
        public bool HasQuestTargets => throw new NotImplementedException();

        public override WoWLocalPlayer Clone()
        {
            var clone = new WoWLocalPlayer(this.HighGuid, this.ObjectType);
            clone.CopyFrom(this);
            return clone;
        }

        public override void CopyFrom(WoWObject sourceBase)
        {
            base.CopyFrom(sourceBase);
            // No additional fields to copy yet
        }
    }
}
