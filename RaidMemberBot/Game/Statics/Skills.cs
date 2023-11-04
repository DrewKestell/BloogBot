using RaidMemberBot.Constants;
using RaidMemberBot.ExtensionMethods;
using System;
using System.Collections.Generic;

namespace RaidMemberBot.Game.Statics
{
    /// <summary>
    ///     Class related to Skills (Engineering etc.)
    /// </summary>
    public class Skills
    {
        /// <summary>
        /// Represents a skill
        /// </summary>
        public class Skill
        {
            internal Skill() { }
            public Enums.Skills Id { get; set; }
            public int CurrentLevel { get; set; }
            public int MaxLevel { get; set; }
        }

        private static readonly Lazy<Skills> _instance = new Lazy<Skills>(() => new Skills());
        private Skills()
        {
        }

        /// <summary>
        /// Returns all skills the player has learned
        /// </summary>
        /// <returns></returns>
        public List<Skill> GetAllPlayerSkills()
        {
            if (!ObjectManager.Instance.IsIngame) return new List<Skill>();
            IntPtr start = ObjectManager.Instance.Player.SkillField;
            List<Skill> list = new List<Skill>();
            int maxSkills = 0x00B700B4.ReadAs<int>();
            for (int i = 0; i < maxSkills + 5; i++)
            {
                IntPtr curPointer = start.Add(i * 12);
                Enums.Skills id = curPointer.ReadAs<Enums.Skills>();
                if (!Enum.IsDefined(typeof(Enums.Skills), id))
                {
                    continue;
                }
                int minMax = curPointer.Add(4).ReadAs<int>();

                list.Add(new Skill
                {
                    Id = id,
                    CurrentLevel = minMax & 0xFFFF,
                    MaxLevel = minMax >> 16
                });
            }
            return list;
        }

        /// <summary>
        ///     Access to the current instance
        /// </summary>
        /// <value>
        ///     The instance.
        /// </value>
        public static Skills Instance => _instance.Value;
    }
}
