using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Different lists of units
    /// </summary>
    public sealed class UnitInfo
    {
        private static readonly Lazy<UnitInfo> _instance = new Lazy<UnitInfo>(() => new UnitInfo());

        private readonly Dictionary<ulong, int> dottedUnits = new Dictionary<ulong, int>();
        private readonly object listLock = new object();

        private UnitInfo()
        {
            WoWEventHandler.Instance.AuraChanged += (sender, args) =>
            {
                if (args.AffectedUnit.ToLower() != "target") return;
                var guid = ObjectManager.Instance.Player.TargetGuid;
                if (guid == 0) return;
                if (ObjectManager.Instance.Npcs.FirstOrDefault(i => i.Guid == guid) == null) return;
                AddDottedUnit(guid);
            };
        }

        /// <summary>
        ///     Access to the Instance
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static UnitInfo Instance => _instance.Value;

        /// <summary>
        ///     NPCs that attack the character
        /// </summary>
        /// <value>
        ///     The NPC attackers.
        /// </value>
        public List<WoWUnit> NpcAttackers
        {
            get
            {
                return ThreadSynchronizer.Instance.RunOnMainThread(() =>
                {
                    lock (listLock)
                    {
                        var dottedUnitsToRemove =
                            dottedUnits.Where(kvp => Environment.TickCount - kvp.Value >= 40000).ToList();
                        foreach (
                            var item in dottedUnitsToRemove)
                            dottedUnits.Remove(item.Key);
                        var player = ObjectManager.Instance.Player;
                        var tmp = new List<WoWUnit>();
                        var maybe = new List<WoWUnit>();
                        var units = ObjectManager.Instance.Units;
                        foreach (var i in units)
                        {
                            var basicCheck = i.IsMob &&
                                             i.Health > 0 &&
                                             i.Reaction != UnitReaction.Friendly
                                             && !i.IsPlayerPet;
                            if (!basicCheck) continue;

                            var OnMe = i.TargetGuid == player.Guid;
                            if (OnMe)
                            {
                                tmp.Add(i);
                                continue;
                            }
                            if (player.HasPet)
                            {
                                var pet = ObjectManager.Instance.Pet;
                                var OnPet = pet != null && i.TargetGuid == pet.Guid;
                                if (OnPet)
                                {
                                    tmp.Add(i);
                                    continue;
                                }
                            }
                            var TappedButNoTarget = i.TappedByMe && i.TargetGuid == 0;
                            var Under100 = i.HealthPercent != 100;
                            if (TappedButNoTarget && Under100)
                            {
                                tmp.Add(i);
                                continue;
                            }
                            var DebuffedByPlayer = i.HealthPercent == 100 &&
                                                   (i.Debuffs.Count > 0 || i.IsCrowdControlled) &&
                                                   dottedUnits.ContainsKey(i.Guid);
                            if (TappedButNoTarget && DebuffedByPlayer)
                                maybe.Add(i);
                        }
                        if (tmp.Count == 0 && maybe.Count != 0)
                            tmp.AddRange(maybe);
                        return tmp;
                    }
                });
            }
        }

        /// <summary>
        ///     Units we can loot
        /// </summary>
        /// <value>
        ///     The lootable.
        /// </value>
        public List<WoWUnit> Lootable => ObjectManager.Instance.Units.Where(i => i.IsMob && i.CanBeLooted).ToList();

        /// <summary>
        ///     Units we can skin
        /// </summary>
        /// <value>
        ///     The skinable.
        /// </value>
        public List<WoWUnit> Skinable => ObjectManager.Instance.Units.Where(i => i.IsMob && i.IsSkinable).ToList();

        internal void AddDottedUnit(ulong parGuid)
        {
            lock (listLock)
            {
                if (dottedUnits.ContainsKey(parGuid)) return;
                dottedUnits.Add(parGuid, Environment.TickCount);
            }
        }

        internal void RemoveDottedUnit(ulong parGuid)
        {
            lock (listLock)
            {
                if (!dottedUnits.ContainsKey(parGuid)) return;
                dottedUnits.Remove(parGuid);
            }
        }
    }
}
