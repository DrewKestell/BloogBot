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
                Race.Human => (0.3889f, 1.95f),
                Race.Orc => (0.5f, 2.25f),
                Race.Dwarf => (0.35f, 1.5f),
                Race.NightElf => (0.5f, 2.2f),
                Race.Undead => (0.4f, 2.0f),
                Race.Tauren => (0.8f, 2.7f),
                Race.Gnome => (0.3f, 1.25f),
                Race.Troll => (0.65f, 2.5f),
                _ => (0.4f, 2.0f), // fallback
            };
        }
    }
}
