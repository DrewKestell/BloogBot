using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Represents a spell manager
    /// </summary>
    public sealed class Spellbook
    {
        private const int ShootBowId = 2480;
        private const int ShootCrossbowId = 7919;
        private const int AutoShotId = 75;
        private const int WandId = 5019;
        private const int AttackId = 6603;

        private const string AutoShot = "Auto Shot";
        private const string BowShoot = "Shoot Bow";
        private const string CrossBowShoot = "Shoot Crossbow";
        private const string Wand = "Shoot";
        private static Lazy<Spellbook> _instance = new Lazy<Spellbook>(() => new Spellbook());

        /// <summary>
        ///     Holds blacklisted spells
        /// </summary>
        internal static Dictionary<string, SpellBlacklistItem> SpellBlacklist =
            new Dictionary<string, SpellBlacklistItem>();

        private readonly IReadOnlyDictionary<string, uint[]> PlayerSpells;


        private Spellbook()
        {
            Dictionary<string, uint[]> tmpPlayerSpells = new Dictionary<string, uint[]>();
            const uint currentPlayerSpellPtr = 0x00B700F0;
            uint index = 0;
            while (index < 1024)
            {
                uint currentSpellId = (currentPlayerSpellPtr + 4 * index).ReadAs<uint>();
                if (currentSpellId == 0) break;
                uint entryPtr = ((0x00C0D780 + 8).ReadAs<uint>() + currentSpellId * 4).ReadAs<uint>();

                uint entrySpellId = entryPtr.ReadAs<uint>();
                uint namePtr = (entryPtr + 0x1E0).ReadAs<uint>();
                string name = namePtr.ReadString();

#if DEBUG
                Console.WriteLine(entrySpellId + " " + name);
#endif

                if (tmpPlayerSpells.ContainsKey(name))
                {
                    List<uint> tmpIds = new List<uint>();
                    tmpIds.AddRange(tmpPlayerSpells[name]);
                    tmpIds.Add(entrySpellId);
                    tmpPlayerSpells[name] = tmpIds.ToArray();
                }
                else
                {
                    uint[] ranks = { entrySpellId };
                    tmpPlayerSpells.Add(name, ranks);
                }
                index += 1;
            }
            PlayerSpells = tmpPlayerSpells;
        }

        /// <summary>
        ///     Access to the characters spell manager
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Spellbook Instance => _instance.Value;


        /// <summary>
        ///     Tells if we are shapeshifted
        /// </summary>
        public bool IsShapeShifted
        {
            get
            {
                LocalPlayer player = ObjectManager.Instance.Player;
                if (player == null) return false;
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (player.Class)
                {
                    case ClassId.Druid:
                        return (player.PlayerBytes & 0x000FEE00) != 0x0000EE00;
                }
                return false;
            }
        }


        /// <summary>
        ///     Updates the spellbook.
        /// </summary>
        public static void UpdateSpellbook()
        {
            _instance = new Lazy<Spellbook>(() => new Spellbook());
        }

        /// <summary>
        ///     Gets the name of a spell
        /// </summary>
        /// <param name="parId">The spell ID</param>
        /// <returns>The spell name</returns>
        public string GetName(int parId)
        {
            if (parId >= (0x00C0D780 + 0xC).ReadAs<uint>() ||
                parId <= 0)
                return "";
            uint entryPtr = ((uint)((0x00C0D780 + 8).ReadAs<uint>() + parId * 4)).ReadAs<uint>();
            uint namePtr = (entryPtr + 0x1E0).ReadAs<uint>();
            return namePtr.ReadString();
        }

        public int GetId(string parName, int parRank = -1)
        {
            if (!PlayerSpells.ContainsKey(parName)) return 0;
            int maxRank = PlayerSpells[parName].Length;
            if (parRank < 1 || parRank > maxRank)
                return (int)PlayerSpells[parName][maxRank - 1];
            return (int)PlayerSpells[parName][parRank - 1];
        }

        /// <summary>
        ///     Cast a spell by name
        /// </summary>
        /// <param name="parName">Name of the spell</param>
        /// <param name="parRank">Rank of the spell</param>
        public void Cast(string parName, int parRank = -1)
        {

            if (string.Equals(parName, BowShoot, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Wait.For2("20SpallSpam", 300, true)) return;
                const string useBow = "UseAction(20)";
                Lua.Instance.Execute(useBow);
                return;
            }
            if (string.Equals(parName, CrossBowShoot, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!Wait.For2("21SpallSpam", 300, true)) return;
                const string useCrossbow = "UseAction(21)";
                Lua.Instance.Execute(useCrossbow);
                return;
            }


            string spellEscaped = parName.Replace("'", "\\'");
            string rankText = parRank != -1 ? $"(Rank {parRank})" : "";
            string spellCastString = $"CastSpellByName('{spellEscaped}{rankText}')";
            Lua.Instance.Execute(spellCastString);
        }

        /// <summary>
        ///     Trys to cast the specified spell and blacklist it for the usage by CastWait for a specified timeframe
        /// </summary>
        /// <param name="parName">Name of the spell</param>
        /// <param name="parBlacklistForMs">The time in ms from now where CastWait wont be able to try casting this spell again</param>
        /// <param name="parRank">Rank of the spell</param>
        public void CastWait(string parName, int parBlacklistForMs, int parRank = -1)
        {
            string currentCast = ObjectManager.Instance.Player.CastingAsName;
            //If we are casting do nothing
            if (currentCast != "")
                return;

            if (SpellBlacklist.ContainsKey(parName))
            {
                //If the spell is still blacklisted
                if (!SpellBlacklist[parName].IsReady)
                    return;

                //Update the spells blacklist time
                SpellBlacklist[parName].UpdateSpell(parBlacklistForMs);
                //Cast
                Cast(parName, parRank);
                return;
            }

            //Add the spell to the dictionary
            SpellBlacklist.Add(parName, new SpellBlacklistItem(parBlacklistForMs));
            //Cast
            Cast(parName, parRank);
        }

        /// <summary>
        ///     Casts an aoe spell at a position
        /// </summary>
        /// <param name="parName">Name of the spell</param>
        /// <param name="parPos">Location</param>
        /// <param name="parRank">Rank of the spell</param>
        public void CastAtPos(string parName, Location parPos, int parRank = -1)
        {
            Functions.CastAtPos(parName, parPos, parRank);
        }

        /// <summary>
        ///     Check if a spell is ready to be cast
        /// </summary>
        /// <param name="parName">Name of the spell</param>
        /// <returns></returns>
        public bool IsSpellReady(string parName)
        {
            int id = GetId(parName);
            return id != 0 && Functions.IsSpellReady(id);
        }

        /// <summary>
        ///     Get the rank of the spell
        /// </summary>
        /// <param name="parSpell">The spell name</param>
        /// <returns></returns>
        public int GetSpellRank(string parSpell)
        {
            if (!PlayerSpells.ContainsKey(parSpell)) return 0;
            return PlayerSpells[parSpell].Length;
        }

        /// <summary>
        ///     Start auto attack
        /// </summary>
        public void Attack()
        {
            const string attack = "if IsCurrentAction('24') == nil then CastSpellByName('Attack') end";
            Lua.Instance.Execute(attack);
            if (Wait.For("AutoAttackTimer12", 1250))
            {
                WoWUnit target = ObjectManager.Instance.Target;
                if (target == null) return;
                ObjectManager.Instance.Player.DisableCtm();
                ObjectManager.Instance.Player.RightClick(target);
                ObjectManager.Instance.Player.EnableCtm();
            }
        }

        /// <summary>
        ///     Stop auto attack
        /// </summary>
        public void StopAttack()
        {
            const string stopAttack = "if IsCurrentAction('24') ~= nil then CastSpellByName('Attack') end";
            Lua.Instance.Execute(stopAttack);
        }

        /// <summary>
        ///     Start wanding
        /// </summary>
        public void StartWand()
        {
            const string wandStart = "if IsAutoRepeatAction(23) == nil then CastSpellByName('Shoot') end";
            if (PlayerSpells.ContainsKey(Wand))
                Lua.Instance.Execute(wandStart);
        }

        /// <summary>
        ///     Start ranged attacking
        /// </summary>
        public void StartRangedAttack()
        {
            const string rangedAttackStart =
                "if IsAutoRepeatAction(22) == nil then CastSpellByName('Auto Shot') end";
            if (PlayerSpells.ContainsKey(AutoShot))
                Lua.Instance.Execute(rangedAttackStart);
        }

        /// <summary>
        ///     Stop ranged attacking
        /// </summary>
        public void StopRangedAttack()
        {
            const string rangedAttackStop = "if IsAutoRepeatAction(22) == 1 then CastSpellByName('Auto Shot') end";
            if (PlayerSpells.ContainsKey(AutoShot))
                Lua.Instance.Execute(rangedAttackStop);
        }

        /// <summary>
        ///     Stop wanding
        /// </summary>
        public void StopWand()
        {
            const string wandStop = "if IsAutoRepeatAction(23) == 1 then CastSpellByName('Shoot') end";
            if (PlayerSpells.ContainsKey(Wand))
                Lua.Instance.Execute(wandStop);
        }

        /// <summary>
        ///     Stops the current cast
        /// </summary>
        public void StopCasting()
        {
            Lua.Instance.Execute("SpellStopCasting()");
        }

        /// <summary>
        ///     Cancels all shapeshift forms
        /// </summary>
        public void CancelShapeshift()
        {
            LocalPlayer player = ObjectManager.Instance.Player;
            if (player == null) return;
            if (!IsShapeShifted) return;
            foreach (Spell x in player.Buffs)
            {
                if (x.Name.Contains("Form"))
                    Lua.Instance.Execute("CastSpellByName('" + x.Name + "')");
            }
        }

        /// <summary>
        ///     Object used for blacklist dictionary
        /// </summary>
        internal class SpellBlacklistItem
        {
            internal int BlacklistUntil;

            internal SpellBlacklistItem(int parBlacklistFor)
            {
                BlacklistUntil = parBlacklistFor + Environment.TickCount;
            }

            internal bool IsReady => Environment.TickCount > BlacklistUntil;

            internal void UpdateSpell(int parBlacklistFor)
            {
                BlacklistUntil = parBlacklistFor + Environment.TickCount;
            }
        }

    }
    public class SpellEffect
    {
        public SpellEffect(string icon, int stackCount, EffectType type)
        {
            Icon = icon;
            StackCount = stackCount;
            Type = type;
        }

        public string Icon { get; }

        public int StackCount { get; }

        public EffectType Type { get; }
    }
}
