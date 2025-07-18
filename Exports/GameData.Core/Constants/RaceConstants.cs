using GameData.Core.Enums;

namespace GameData.Core.Constants
{
    public static class RaceDimensions
    {
        /// <summary>
        /// Returns canonical collision capsule dimensions for a given character race.
        /// </summary>
        /// <param name="race">The character race</param>
        /// <returns>Tuple of (radius, height)</returns>
        public static (float radius, float height) GetCapsuleForRace(Race race)
        {
            return race switch
            {
                Race.Human => (0.5f, 2.0f),   // raised slightly from 0.3889
                Race.Orc => (0.55f, 2.3f),
                Race.Dwarf => (0.45f, 1.6f),
                Race.NightElf => (0.55f, 2.3f),
                Race.Undead => (0.5f, 2.1f),
                Race.Tauren => (0.9f, 2.8f),   // very wide and tall
                Race.Gnome => (0.4f, 1.4f),
                Race.Troll => (0.7f, 2.6f),
                _ => (0.5f, 2.0f),
            };
        }
    }
}
