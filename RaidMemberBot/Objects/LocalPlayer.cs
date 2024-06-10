using System;
using System.Linq;
using ObjectManager = RaidMemberBot.Game.Statics.ObjectManager;
using Functions = RaidMemberBot.Mem.Functions;
using static RaidMemberBot.Constants.Enums;
using RaidMemberBot.Mem;
using System.Collections.Generic;
using RaidMemberBot.Constants;

namespace RaidMemberBot.Objects
{
    public class LocalPlayer : WoWPlayer
    {
        internal LocalPlayer(
            IntPtr pointer,
            ulong guid,
            WoWObjectTypes objectType)
            : base(pointer, guid, objectType)
        {
            RefreshSpells();
        }

        readonly Random random = new Random();

        // WARRIOR
        const string BattleStance = "Battle Stance";
        const string BerserkerStance = "Berserker Stance";
        const string DefensiveStance = "Defensive Stance";

        // DRUID
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";

        const string WandLuaScript = "if IsCurrentAction(72) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsCurrentAction(72) ~= nil then CastSpellByName('Shoot') end";

        const string AutoAttackLuaScript = "if IsCurrentAction(72) == nil then CastSpellByName('Attack') end";
        const string TurnOffAutoAttackLuaScript = "if IsCurrentAction(72) ~= nil then CastSpellByName('Attack') end";

        // OPCODES
        const int SET_FACING_OPCODE = 0xDA; // TBC

        public readonly IDictionary<string, int[]> PlayerSpells = new Dictionary<string, int[]>();
        public readonly List<int> PlayerSkills = new List<int>();
        public new ulong TargetGuid => MemoryManager.ReadUlong(Offsets.Player.TargetGuid, true);
        public WoWUnit Target => (WoWUnit)ObjectManager.Objects.FirstOrDefault(x => x.Guid == TargetGuid);

        public bool TargetInMeleeRange => Functions.LuaCallWithResult("{0} = CheckInteractDistance(\"target\", 3)")[0] == "1";

        public Class Class => (Class)MemoryManager.ReadByte((IntPtr)MemoryAddresses.LocalPlayerClass);
        public string Race => Functions.LuaCallWithResult("{0} = UnitRace('player')")[0];

        public Position CorpsePosition => new Position(
            MemoryManager.ReadFloat((IntPtr)MemoryAddresses.LocalPlayerCorpsePositionX),
            MemoryManager.ReadFloat((IntPtr)MemoryAddresses.LocalPlayerCorpsePositionY),
            MemoryManager.ReadFloat((IntPtr)MemoryAddresses.LocalPlayerCorpsePositionZ));

        public void Face(Position pos)
        {
            if (pos == null) return;

            // sometimes the client gets in a weird state and CurrentFacing is negative. correct that here.
            if (Facing < 0)
            {
                SetFacing((float)(Math.PI * 2) + Facing);
                return;
            }
            SetFacing(GetFacingForPosition(pos));
            return;
            //if this is a new position, restart the turning flow
            //if (turning && pos != turningToward)
            //{
            //    ResetFacingState();
            //    return;
            //}

            //// return if we're already facing the position
            //if (!turning && IsFacing(pos))
            //    return;

            //if (!turning)
            //{
            //    var requiredFacing = GetFacingForPosition(pos);
            //    float amountToTurn;
            //    if (requiredFacing > Facing)
            //    {
            //        if (requiredFacing - Facing > Math.PI)
            //        {
            //            amountToTurn = -((float)(Math.PI * 2) - requiredFacing + Facing);
            //        }
            //        else
            //        {
            //            amountToTurn = requiredFacing - Facing;
            //        }
            //    }
            //    else
            //    {
            //        if (Facing - requiredFacing > Math.PI)
            //        {
            //            amountToTurn = (float)(Math.PI * 2) - Facing + requiredFacing;
            //        }
            //        else
            //        {
            //            amountToTurn = requiredFacing - Facing;
            //        }
            //    }

            //    // if the turn amount is relatively small, just face that direction immediately
            //    if (Math.Abs(amountToTurn) < 0.05)
            //    {
            //        SetFacing(requiredFacing);
            //        ResetFacingState();
            //        return;
            //    }

            //    turning = true;
            //    turningToward = pos;
            //    amountPerTurn = amountToTurn / 2;
            //}
            //if (turning)
            //{
            //    if (turnCount < 1)
            //    {
            //        var twoPi = (float)(Math.PI * 2);
            //        var newFacing = Facing + amountPerTurn;

            //        if (newFacing < 0)
            //            newFacing = twoPi + amountPerTurn + Facing;
            //        else if (newFacing > Math.PI * 2)
            //            newFacing = amountPerTurn - (twoPi - Facing);

            //        SetFacing(newFacing);
            //        turnCount++;
            //    }
            //    else
            //    {
            //        SetFacing(GetFacingForPosition(pos));
            //        ResetFacingState();
            //    }
            //}
        }

        // Nat added this to see if he could test out the cleave radius which is larger than that isFacing radius
        public bool IsInCleave(Position position) => Math.Abs(GetFacingForPosition(position) - Facing) < 3f;

        public void SetFacing(float facing)
        {
            Functions.SetFacing(IntPtr.Add(Pointer, MemoryAddresses.LocalPlayer_SetFacingOffset), facing);
            Functions.SendMovementUpdate(Pointer, SET_FACING_OPCODE);
        }

        public void MoveToward(Position pos)
        {
            Face(pos);
            StartMovement(ControlBits.Front);
        }

        public void Turn180()
        {
            var newFacing = Facing + Math.PI;
            if (newFacing > (Math.PI * 2))
                newFacing -= Math.PI * 2;
            SetFacing((float)newFacing);
        }

        // the client will NOT send a packet to the server if a key is already pressed, so you're safe to spam this
        public void StartMovement(ControlBits bits)
        {
            if (bits == ControlBits.Nothing)
                return;

            Functions.SetControlBit((int)bits, 1, Environment.TickCount);
        }

        public void StopAllMovement()
        {
            if (MovementFlags != MovementFlags.MOVEFLAG_NONE)
            {
                var bits = ControlBits.Front | ControlBits.Back | ControlBits.Left | ControlBits.Right | ControlBits.StrafeLeft | ControlBits.StrafeRight;

                StopMovement(bits);
            }
        }

        public void StopMovement(ControlBits bits)
        {
            if (bits == ControlBits.Nothing)
                return;

            Functions.SetControlBit((int)bits, 0, Environment.TickCount);
        }

        public void Jump()
        {
            StopMovement(ControlBits.Jump);
            StartMovement(ControlBits.Jump);
        }

        // use this to determine whether you can use cannibalize
        public bool TastyCorpsesNearby =>
            ObjectManager.Units.Any(u =>
                u.Position.DistanceTo(Position) < 5
                && u.CreatureType.HasFlag(CreatureType.Humanoid | CreatureType.Undead)
            );

        public void Stand() => Functions.LuaCall("DoEmote(\"STAND\")");

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

        public void SetTarget(ulong guid)
        {
            Functions.SetTarget(guid);
        }

        ulong ComboPointGuid { get; set; }

        public byte ComboPoints
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

        public bool IsDiseased => GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Disease);

        public bool IsCursed => GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Curse);

        public bool IsPoisoned => GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Poison);

        public bool HasMagicDebuff => GetDebuffs(LuaTarget.Player).Any(t => t.Type == EffectType.Magic);

        public void ReleaseCorpse() => Functions.ReleaseCorpse(Pointer);

        public void RetrieveCorpse() => Functions.RetrieveCorpse();

        public void RefreshSpells()
        {
            PlayerSpells.Clear();
            for (var i = 0; i < 1024; i++)
            {
                var currentSpellId = MemoryManager.ReadInt((IntPtr)(MemoryAddresses.LocalPlayerSpellsBase + 4 * i));
                if (currentSpellId == 0) break;

                string name;
                var spellsBasePtr = MemoryManager.ReadIntPtr((IntPtr)0x00C0D788);
                var spellPtr = MemoryManager.ReadIntPtr(spellsBasePtr + currentSpellId * 4);

                var spellNamePtr = MemoryManager.ReadIntPtr(spellPtr + 0x1E0);
                name = MemoryManager.ReadString(spellNamePtr);

                if (PlayerSpells.ContainsKey(name))
                    PlayerSpells[name] = new List<int>(PlayerSpells[name])
                    {
                        currentSpellId
                    }.ToArray();
                else
                    PlayerSpells.Add(name, new[] { currentSpellId });
            }
        }

        public void RefreshSkills()
        {
            PlayerSkills.Clear();
            var skillPtr1 = MemoryManager.ReadIntPtr(IntPtr.Add(Pointer, 8));
            var skillPtr2 = IntPtr.Add(skillPtr1, 0xB38);

            var maxSkills = MemoryManager.ReadInt((IntPtr)0x00B700B4);
            for (var i = 0; i < maxSkills + 12; i++)
            {
                var curPointer = IntPtr.Add(skillPtr2, i * 12);

                var id = (Skills)MemoryManager.ReadShort(curPointer);
                if (!Enum.IsDefined(typeof(Skills), id))
                {
                    continue;
                }

                PlayerSkills.Add((short)id);
            }
        }

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

            if (parId >= MemoryManager.ReadUint((IntPtr)(0x00C0D780 + 0xC)) || parId <= 0)
                return 0;

            var entryPtr = MemoryManager.ReadIntPtr((IntPtr)((uint)(MemoryManager.ReadUint((IntPtr)(0x00C0D780 + 8)) + parId * 4)));
            return MemoryManager.ReadInt((entryPtr + 0x0080));

        }

        public bool KnowsSpell(string name) => PlayerSpells.ContainsKey(name);

        public bool MainhandIsEnchanted => Functions.LuaCallWithResult("{0} = GetWeaponEnchantInfo()")[0] == "1";

        public ulong GetBackpackItemGuid(int slot) => MemoryManager.ReadUlong(GetDescriptorPtr() + (MemoryAddresses.LocalPlayer_BackpackFirstItemOffset + (slot * 8)));

        public ulong GetEquippedItemGuid(EquipSlot slot) => MemoryManager.ReadUlong(IntPtr.Add(Pointer, MemoryAddresses.LocalPlayer_EquipmentFirstItemOffset + ((int)slot - 1) * 0x8));

        public WoWItem GetEquippedItem(EquipSlot slot) => ObjectManager.Items.FirstOrDefault(x => x.Guid == GetEquippedItemGuid(slot));

        public bool CanRiposte
        {
            get
            {
                if (PlayerSpells.ContainsKey("Riposte"))
                {
                    var results = Functions.LuaCallWithResult("{0}, {1} = IsUsableSpell('Riposte')");
                    if (results.Length > 0)
                        return results[0] == "1";
                    else
                        return false;
                }
                return false;
            }
        }

        public void StartAttack()
        {
            if (!IsCasting && (Class == Class.Warlock || Class == Class.Mage || Class == Class.Priest))
            {
                Functions.LuaCall(WandLuaScript);
            }
            else if (Class != Class.Hunter)
            {
                Functions.LuaCall(AutoAttackLuaScript);
            }
        }
    }
}
