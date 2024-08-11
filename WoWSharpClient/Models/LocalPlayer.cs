using BotRunner.Constants;
using BotRunner.Interfaces;
using PathfindingService.Models;
using WoWSharpClient.Manager;


namespace WoWSharpClient.Models
{
    public class LocalPlayer(byte[] lowGuid, byte[] highGuid) : Player(lowGuid, highGuid)
    {
        private readonly Random random = new();

        // WARRIOR
        private const string BattleStance = "Battle Stance";
        private const string BerserkerStance = "Berserker Stance";
        private const string DefensiveStance = "Defensive Stance";

        // DRUID
        private const string BearForm = "Bear Form";
        private const string CatForm = "Cat Form";

        public readonly IDictionary<string, int[]> PlayerSpells = new Dictionary<string, int[]>();
        public readonly List<int> PlayerSkills = [];
        public Unit Target => null;

        public bool TargetInMeleeRange { get; set; }

        public Class Class { get; set; }
        public string Race { get; set; }

        public Position CorpsePosition => new(0, 0, 0);

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
        }

        // Nat added this to see if he could test out the cleave radius which is larger than that isFacing radius
        public bool IsInCleave(Position position) => Math.Abs(GetFacingForPosition(position) - Facing) < 3f;

        public void SetFacing(float facing)
        {

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


        }

        public void Jump()
        {
            StopMovement(ControlBits.Jump);
            StartMovement(ControlBits.Jump);
        }

        // use this to determine whether you can use cannibalize

        public void Stand()
        {

        }

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
                return false;
            }
        }

        public void SetTarget(ulong guid)
        {

        }

        private ulong ComboPointGuid { get; set; }

        public byte ComboPoints
        {
            get
            {
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

        public void ReleaseCorpse() { }

        public void RetrieveCorpse() { }

        public void RefreshSpells()
        {

        }

        public void RefreshSkills()
        {

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
            return true;
        }

        public int GetManaCost(string spellName, int rank = -1)
        {
            return 0;
        }

        public bool KnowsSpell(string name) => PlayerSpells.ContainsKey(name);

        public bool MainhandIsEnchanted => true;

        public ulong GetBackpackItemGuid(int slot) => 0;

        public ulong GetEquippedItemGuid(EquipSlot slot) => 0;

        public Item GetEquippedItem(EquipSlot slot) => null;

        public bool CanRiposte
        {
            get
            {
                return false;
            }
        }

        public void StartAttack()
        {

        }
    }
}
