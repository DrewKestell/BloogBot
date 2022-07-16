using BloogBot.Game.Enums;

using System;
using System.Collections.Generic;

namespace BloogBot.Game.Objects
{
    class WoWPet : WoWUnit
    {
        internal WoWPet(
             IntPtr pointer,
             ulong guid,
             ObjectType objectType) : base(pointer, guid, objectType)
        {
            // TODO
            //RefreshSpells();
        }

        readonly IDictionary<string, int[]> petSpells = new Dictionary<string, int[]>();

        public bool KnowsSpell(string name) => petSpells.ContainsKey(name);

        public void Attack() => LuaCall("PetAttack()");

        public void RefreshSpells()
        {
            petSpells.Clear();
            for (var i = 0; i < 1024; i++)
            {
                var currentSpellId = MemoryManager.ReadInt((IntPtr)(MemoryAddresses.WoWPet_SpellsBase + 4 * i));
                if (currentSpellId == 0) break;
                var spell = Functions.GetSpellDBEntry(currentSpellId);

                if (petSpells.ContainsKey(spell.Name))
                    petSpells[spell.Name] = new List<int>(petSpells[spell.Name])
                    {
                        currentSpellId
                    }.ToArray();
                else
                    petSpells.Add(spell.Name, new[] { currentSpellId });
            }
        }
    }
}
