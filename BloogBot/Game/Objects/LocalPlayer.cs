using BloogBot.Game.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot.Game.Objects
{
    public class LocalPlayer : WoWPlayer
    {
        internal LocalPlayer(
            IntPtr pointer,
            ulong guid,
            ObjectType objectType)
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

        // OPCODES
        const int SET_FACING_OPCODE = 0xDA; // TBC

        readonly IDictionary<string, int[]> playerSpells = new Dictionary<string, int[]>();

        public WoWUnit Target { get; set; }

        bool turning;
        int totalTurns;
        int turnCount;
        float amountPerTurn;
        Position turningToward;

        public Class Class => (Class)MemoryManager.ReadByte((IntPtr)MemoryAddresses.LocalPlayerClass);

        public void AntiAfk() => MemoryManager.WriteInt((IntPtr)MemoryAddresses.LastHardwareAction, Environment.TickCount);

        public Position CorpsePosition => new Position(
            MemoryManager.ReadFloat((IntPtr)MemoryAddresses.LocalPlayerCorpsePositionX),
            MemoryManager.ReadFloat((IntPtr)MemoryAddresses.LocalPlayerCorpsePositionY),
            MemoryManager.ReadFloat((IntPtr)MemoryAddresses.LocalPlayerCorpsePositionZ));

        public void Face(Position pos)
        {
            // sometimes the client gets in a weird state and CurrentFacing is negative. correct that here.
            if (Facing < 0)
            {
                SetFacing((float)(Math.PI * 2) + Facing);
                return;
            }

            // if this is a new position, restart the turning flow
            if (turning && pos != turningToward)
            {
                ResetFacingState();
                return;
            }

            // return if we're already facing the position
            if (!turning && IsFacing(pos))
                return;

            if (!turning)
            {
                var requiredFacing = GetFacingForPosition(pos);
                float amountToTurn;
                if (requiredFacing > Facing)
                {
                    if (requiredFacing - Facing > Math.PI)
                    {
                        amountToTurn = -((float)(Math.PI * 2) - requiredFacing + Facing);
                    }
                    else
                    {
                        amountToTurn = requiredFacing - Facing;
                    }
                }
                else
                {
                    if (Facing - requiredFacing > Math.PI)
                    {
                        amountToTurn = (float)(Math.PI * 2) - Facing + requiredFacing;
                    }
                    else
                    {
                        amountToTurn = requiredFacing - Facing;
                    }
                }

                // if the turn amount is relatively small, just face that direction immediately
                if (Math.Abs(amountToTurn) < 0.05)
                {
                    SetFacing(requiredFacing);
                    ResetFacingState();
                    return;
                }

                turning = true;
                turningToward = pos;
                totalTurns = random.Next(2, 5);
                amountPerTurn = amountToTurn / totalTurns;
            }
            if (turning)
            {
                if (turnCount < totalTurns - 1)
                {
                    var twoPi = (float)(Math.PI * 2);
                    var newFacing = Facing + amountPerTurn;

                    if (newFacing < 0)
                        newFacing = twoPi + amountPerTurn + Facing;
                    else if (newFacing > Math.PI * 2)
                        newFacing = amountPerTurn - (twoPi - Facing);

                    SetFacing(newFacing);
                    turnCount++;
                }
                else
                {
                    SetFacing(GetFacingForPosition(pos));
                    ResetFacingState();
                }
            }
        }

        // Nat added this to see if he could test out the cleave radius which is larger than that isFacing radius
        public bool IsInCleave(Position position) => Math.Abs((GetFacingForPosition(position) - Facing)) < 3f;

        public void SetFacing(float facing)
        {
            if (ClientHelper.ClientVersion == ClientVersion.WotLK)
            {
                Functions.SetFacing(Pointer, facing);
            }
            else
            {
                Functions.SetFacing(IntPtr.Add(Pointer, MemoryAddresses.LocalPlayer_SetFacingOffset), facing);
                Functions.SendMovementUpdate(Pointer, SET_FACING_OPCODE);
            }
        }

        public void MoveToward(Position pos)
        {
            Face(pos);
            StartMovement(ControlBits.Front);
        }

        void ResetFacingState()
        {
            turning = false;
            totalTurns = 0;
            turnCount = 0;
            amountPerTurn = 0;
            turningToward = null;
            StopMovement(ControlBits.StrafeLeft);
            StopMovement(ControlBits.StrafeRight);
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

            Logger.LogVerbose($"StartMovement: {bits}");

            Functions.SetControlBit((int)bits, 1, Environment.TickCount);
        }

        public void StopAllMovement()
        {
            var bits = ControlBits.Front | ControlBits.Back | ControlBits.Left | ControlBits.Right | ControlBits.StrafeLeft | ControlBits.StrafeRight;

            StopMovement(bits);
        }

        public void StopMovement(ControlBits bits)
        {
            if (bits == ControlBits.Nothing)
                return;

            Logger.LogVerbose($"StopMovement: {bits}");
            Functions.SetControlBit((int)bits, 0, Environment.TickCount);
        }

        public void Jump() => Functions.Jump();

        // use this to determine whether you can use cannibalize
        public bool TastyCorpsesNearby =>
            ObjectManager.Units.Any(u =>
                u.Position.DistanceTo(Position) < 5
                && u.CreatureType.HasFlag(CreatureType.Humanoid | CreatureType.Undead)
            );

        public void Stand() => LuaCall("DoEmote(\"STAND\")");

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
                var result = LuaCallWithResults($"{{0}} = UnitIsGhost('player')");

                if (result.Length > 0)
                    return result[0] == "1";
                else
                    return false;
            }
        }

        public void SetTarget(ulong guid) => Functions.SelectObject(guid);

        public bool CanOverpower => MemoryManager.ReadInt((IntPtr)MemoryAddresses.LocalPlayerCanOverpower) > 0;

        public byte ComboPoints
        {
            get
            {
                var result = ObjectManager.Player.LuaCallWithResults($"{{0}} = GetComboPoints('target')");

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
            playerSpells.Clear();
            for (var i = 0; i < 1024; i++)
            {
                var currentSpellId = MemoryManager.ReadInt((IntPtr)(MemoryAddresses.LocalPlayerSpellsBase + 4 * i));
                if (currentSpellId == 0) break;
                var spell = Functions.GetSpellDBEntry(currentSpellId);

                if (playerSpells.ContainsKey(spell.Name))
                    playerSpells[spell.Name] = new List<int>(playerSpells[spell.Name])
                    {
                        currentSpellId
                    }.ToArray();
                else
                    playerSpells.Add(spell.Name, new[] { currentSpellId });
            }
        }

        public int GetSpellId(string spellName, int rank = -1)
        {
            int spellId;

            var maxRank = playerSpells[spellName].Length;
            if (rank < 1 || rank > maxRank)
                spellId = playerSpells[spellName][maxRank - 1];
            else
                spellId = playerSpells[spellName][rank - 1];

            return spellId;
        }

        public bool IsSpellReady(string spellName, int rank = -1)
        {
            if (!playerSpells.ContainsKey(spellName))
                return false;

            var spellId = GetSpellId(spellName, rank);

            return !Functions.IsSpellOnCooldown(spellId);
        }

        public int GetManaCost(string spellName, int rank = -1)
        {
            var spellId = GetSpellId(spellName, rank);
            return Functions.GetSpellDBEntry(spellId).Cost;
        }

        public bool KnowsSpell(string name) => playerSpells.ContainsKey(name);

        public bool MainhandIsEnchanted => LuaCallWithResults("{0} = GetWeaponEnchantInfo()")[0] == "1";

        public ulong GetBackpackItemGuid(int slot) => MemoryManager.ReadUlong(GetDescriptorPtr() + (MemoryAddresses.LocalPlayer_BackpackFirstItemOffset + (slot * 8)));

        public ulong GetEquippedItemGuid(EquipSlot slot) => MemoryManager.ReadUlong(IntPtr.Add(Pointer, (MemoryAddresses.LocalPlayer_EquipmentFirstItemOffset + ((int)slot - 1) * 0x8)));
        
        public void CastSpell(string spellName, ulong targetGuid)
        {
            var spellId = GetSpellId(spellName);
            Functions.CastSpellById(spellId, targetGuid);
        }

        public bool CanLoot(WoWUnit target) => Functions.CanLoot(target.Pointer);

        public bool InLosWith(Position position)
        {
            var i = Functions.Intersect(Position, position);
            return i.X == 0 && i.Y == 0 && i.Z == 0;
        }

        public bool CanRiposte
        {
            get
            {
                var results = LuaCallWithResults("{0} = IsUsableSpell(\"Riposte\")");
                if (results.Length > 0)
                    return results[0] == "1";
                else
                    return false;
            }
        }

        public void CastSpellAtPosition(string spellName, Position position)
        {
            return;
            // Functions.CastAtPosition(spellName, position);
        }
    }
}
