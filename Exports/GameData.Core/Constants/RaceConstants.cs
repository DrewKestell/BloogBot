using GameData.Core.Enums;

namespace GameData.Core.Constants
{
    public static class RaceDimensions
    {
        /// <summary>
        /// Returns collision capsule dimensions for a given character race.
        /// Based on WoW Classic 1.12.1 values and vmangos data.
        /// </summary>
        /// <param name="race">The character race</param>
        /// <returns>Tuple of (radius, height)</returns>
        public static (float radius, float height) GetCapsuleForRace(Race race)
        {
            // These values are approximations based on:
            // 1. vmangos bounding_radius and combat_reach values
            // 2. WoW Classic testing data
            // 3. The fact that standard collision height needs to be around 2.0-2.5 yards
            //    for proper swimming detection with 75% submersion requirement

            return race switch
            {
                // Standard races (bounding_radius ~0.3-0.4, height ~2.0)
                Race.Human => (0.3889f, 2.0f),     // Standard humanoid
                Race.Dwarf => (0.347f, 1.35f),     // Shorter but wider
                Race.Gnome => (0.3f, 1.0f),        // Smallest race
                Race.NightElf => (0.3889f, 2.5f),  // Tallest Alliance race

                // Horde races
                Race.Orc => (0.5f, 2.4f),          // Bulkier than human
                Race.Undead => (0.3836f, 2.1f),    // Similar to human but slightly taller
                Race.Troll => (0.5f, 2.7f),        // Very tall when standing upright
                Race.Tauren => (1.5f, 3.0f),       // Largest race, special hitbox

                // Default fallback
                _ => (0.3889f, 2.0f),
            };
        }

        /// <summary>
        /// Gets the minimum swim depth for a race (how deep they need to be to swim).
        /// In vmangos: GetMinSwimDepth() = GetCollisionHeight() * 0.75f
        /// </summary>
        public static float GetMinSwimDepth(Race race)
        {
            var (_, height) = GetCapsuleForRace(race);
            return height * 0.75f;
        }

        /// <summary>
        /// Gets the combat reach for a race (melee range extension).
        /// Most races have standard 1.5 yard reach, tauren have more.
        /// </summary>
        public static float GetCombatReach(Race race)
        {
            return race switch
            {
                Race.Tauren => 2.0f,    // Tauren have extended reach
                Race.Gnome => 1.0f,     // Gnomes have shorter reach
                _ => 1.5f,              // Standard reach for most races
            };
        }
    }

    // Usage in your test:
    // For an Orc, height would be 2.4 yards
    // Swimming starts when: z < (liquidZ - 2.4 * 0.75) = liquidZ - 1.8
    // So at liquidZ=32.934, swimming starts at z < 31.134
    // Your unit at 31.1499 is just above this threshold, so NOT swimming (correct!)
}
