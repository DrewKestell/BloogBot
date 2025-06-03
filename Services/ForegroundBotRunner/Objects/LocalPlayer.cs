using ForegroundBotRunner.Mem;
using GameData.Core.Enums;
using GameData.Core.Interfaces;
using GameData.Core.Models;
using static GameData.Core.Constants.Spellbook;
using Functions = ForegroundBotRunner.Mem.Functions;

namespace ForegroundBotRunner.Objects
{
    public class LocalPlayer : WoWPlayer, IWoWLocalPlayer
    {
        internal LocalPlayer(nint pointer, HighGuid guid, WoWObjectType objectType)
            : base(pointer, guid, objectType) { }

        public readonly IDictionary<string, int[]> PlayerSpells = new Dictionary<string, int[]>();
        public readonly List<int> PlayerSkills = [];
        public new ulong TargetGuid => MemoryManager.ReadUlong(Offsets.Player.TargetGuid, true);

        public static bool TargetInMeleeRange =>
            Functions.LuaCallWithResult("{0} = CheckInteractDistance(\"target\", 3)")[0] == "1";

        public new Class Class => (Class)MemoryManager.ReadByte(MemoryAddresses.LocalPlayerClass);
        public new Race Race =>
            Enum.GetValues(typeof(Race))
                .Cast<Race>()
                .FirstOrDefault(v =>
                    v.GetDescription() == Functions.LuaCallWithResult("{0} = UnitRace('player')")[0]
                );

        public Position CorpsePosition =>
            new(
                MemoryManager.ReadFloat(MemoryAddresses.LocalPlayerCorpsePositionX),
                MemoryManager.ReadFloat(MemoryAddresses.LocalPlayerCorpsePositionY),
                MemoryManager.ReadFloat(MemoryAddresses.LocalPlayerCorpsePositionZ)
            );

        public string CurrentStance
        {
            get
            {
                if (Buffs.Any(b => b.Name == BattleStance))
                    return BattleStance;

                if (Buffs.Any(b => b.Name == DefensiveStance))
                    return DefensiveStance;

                if (Buffs.Any(b => b.Name == BerserkerStance))
                    return BerserkerStance;

                return "None";
            }
        }

        public bool InGhostForm
        {
            get
            {
                var result = Functions.LuaCallWithResult("{0} = UnitIsGhost('player')");

                if (result.Length > 0)
                    return result[0] == "1";
                else
                    return false;
            }
        }

        private ulong ComboPointGuid { get; set; }

        public int ComboPoints
        {
            get
            {
                var result = Functions.LuaCallWithResult("{0} = GetComboPoints('target')");

                if (result.Length > 0)
                    return Convert.ToByte(result[0]);
                else
                    return 0;
            }
        }

        public string CurrentShapeshiftForm
        {
            get
            {
                if (HasBuff(BearForm))
                    return BearForm;

                if (HasBuff(CatForm))
                    return CatForm;

                return "Human Form";
            }
        }

        public bool IsDiseased =>
            GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Disease);

        public bool IsCursed => GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Curse);

        public bool IsPoisoned =>
            GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Poison);

        public bool HasMagicDebuff =>
            GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Magic);

        public int GetSpellId(string spellName, int rank = -1)
        {
            int spellId;

            var maxRank = PlayerSpells[spellName].Length;
            if (rank < 1 || rank > maxRank)
                spellId = PlayerSpells[spellName][maxRank - 1];
            else
                spellId = PlayerSpells[spellName][rank - 1];

            return spellId;
        }

        public bool IsSpellReady(string spellName, int rank = -1)
        {
            if (!PlayerSpells.ContainsKey(spellName))
                return false;

            var spellId = GetSpellId(spellName, rank);

            return !Functions.IsSpellOnCooldown(spellId);
        }

        public int GetManaCost(string spellName, int rank = -1)
        {
            var parId = GetSpellId(spellName, rank);

            if (parId >= MemoryManager.ReadUint(0x00C0D780 + 0xC) || parId <= 0)
                return 0;

            var entryPtr = MemoryManager.ReadIntPtr(
                (nint)(uint)(MemoryManager.ReadUint(0x00C0D780 + 8) + parId * 4)
            );
            return MemoryManager.ReadInt(entryPtr + 0x0080);
        }

        public bool KnowsSpell(string name) => PlayerSpells.ContainsKey(name);

        public bool MainhandIsEnchanted =>
            Functions.LuaCallWithResult("{0} = GetWeaponEnchantInfo()")[0] == "1";

        public bool CanRiposte
        {
            get
            {
                if (PlayerSpells.ContainsKey("Riposte"))
                {
                    var results = Functions.LuaCallWithResult(
                        "{0}, {1} = IsUsableSpell('Riposte')"
                    );
                    if (results.Length > 0)
                        return results[0] == "1";
                    else
                        return false;
                }
                return false;
            }
        }

        public bool TastyCorpsesNearby => throw new NotImplementedException();

        public uint Copper => throw new NotImplementedException();

        public bool IsAutoAttacking => throw new NotImplementedException();

        public bool CanResurrect => throw new NotImplementedException();
        public bool InBattleground => throw new NotImplementedException();
        public bool HasQuestTargets => throw new NotImplementedException();
    }
}
