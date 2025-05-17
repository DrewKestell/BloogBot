using GameData.Core.Enums;
using Microsoft.Data.Sqlite;
using System.Text;

namespace StateManager.Repository
{
    public class ActorDatabase
    {
        private string connectionString;

        // Races, Classes, and Professions Definitions
        private static readonly Dictionary<Race, Class[]> ClassesByRace = new()
        {
            { Race.Human,       new[] { Class.Warrior, Class.Paladin,   Class.Rogue,    Class.Priest,   Class.Mage,     Class.Warlock } },
            { Race.Dwarf,       new[] { Class.Warrior, Class.Paladin,   Class.Hunter,   Class.Rogue,    Class.Priest } },
            { Race.NightElf,    new[] { Class.Warrior, Class.Hunter,    Class.Rogue,    Class.Priest,   Class.Druid } },
            { Race.Gnome,       new[] { Class.Warrior, Class.Rogue,     Class.Mage,     Class.Warlock } },
            { Race.Orc,         new[] { Class.Warrior, Class.Hunter,    Class.Rogue,    Class.Shaman,   Class.Warlock } },
            { Race.Undead,      new[] { Class.Warrior, Class.Rogue,     Class.Priest,   Class.Mage,     Class.Warlock } },
            { Race.Tauren,      new[] { Class.Warrior, Class.Hunter,    Class.Shaman,   Class.Druid } },
            { Race.Troll,       new[] { Class.Warrior, Class.Hunter,    Class.Rogue,    Class.Priest,   Class.Shaman,   Class.Mage } }
        };

        private static readonly (Skills, Skills)[] Professions =
        {
            (Skills.ALCHEMY, Skills.HERBALISM),
            (Skills.BLACKSMITHING, Skills.MINING),
            (Skills.ENGINERING, Skills.MINING),
            (Skills.LEATHERWORKING, Skills.SKINNING),
            (Skills.ENCHANTING, Skills.TAILORING)
        };

        // Valid professions for each class
        private static readonly Dictionary<Class, (Skills, Skills)[]> ClassProfessions = new()
        {
            { Class.Warrior,    new[] { Professions[1], Professions[2], Professions[0] } },
            { Class.Paladin,    new[] { Professions[1], Professions[2], Professions[4] } },
            { Class.Warlock,    new[] { Professions[4], Professions[0], Professions[2] } },
            { Class.Druid,      new[] { Professions[0], Professions[3], Professions[2] } },
            { Class.Hunter,     new[] { Professions[4], Professions[3], Professions[0] } },
            { Class.Mage,       new[] { Professions[4], Professions[0], Professions[2] } },
            { Class.Priest,     new[] { Professions[4], Professions[0], Professions[2] } },
            { Class.Rogue,      new[] { Professions[2], Professions[3], Professions[0] } },
            { Class.Shaman,     new[] { Professions[0], Professions[3], Professions[2] } }
        };

        private static readonly Dictionary<Class, Skills[]> Specs = new()
        {
            { Class.Warrior,    new[] { Skills.ARMS, Skills.FURY, Skills.PROTECTION } },
            { Class.Paladin,    new[] { Skills.HOLY, Skills.PROTECTION, Skills.RETRIBUTION } },
            { Class.Warlock,    new[] { Skills.AFFLICTION, Skills.DEMONOLOGY, Skills.DESTRUCTION } },
            { Class.Druid,      new[] { Skills.BALANCE, Skills.FERAL_COMBAT, Skills.RESTORATION } },
            { Class.Hunter,     new[] { Skills.BEAST_MASTERY, Skills.MARKSMANSHIP, Skills.SURVIVAL } },
            { Class.Mage,       new[] { Skills.ARCANE, Skills.FIRE, Skills.FROST } },
            { Class.Priest,     new[] { Skills.DISCIPLINE, Skills.HOLY, Skills.SHADOW } },
            { Class.Rogue,      new[] { Skills.ASSASSINATION, Skills.COMBAT, Skills.SUBTLETY } },
            { Class.Shaman,     new[] { Skills.ELEMENTAL_COMBAT, Skills.ENHANCEMENT, Skills.RESTORATION } }
        };

        private static readonly Dictionary<Skills, Role> SpecRoleFulfillment = new()
        {
            { Skills.BALANCE,           Role.DPS },
            { Skills.FERAL_COMBAT,      Role.DPS },
            { Skills.BEAST_MASTERY,     Role.DPS },
            { Skills.MARKSMANSHIP,      Role.DPS },
            { Skills.SURVIVAL,          Role.DPS },
            { Skills.ARCANE,            Role.DPS },
            { Skills.FIRE,              Role.DPS },
            { Skills.FROST,             Role.DPS },
            { Skills.SHADOW,            Role.DPS },
            { Skills.ASSASSINATION,     Role.DPS },
            { Skills.COMBAT,            Role.DPS },
            { Skills.SUBTLETY,          Role.DPS },
            { Skills.ELEMENTAL_COMBAT,  Role.DPS },
            { Skills.ENHANCEMENT,       Role.DPS },
            { Skills.AFFLICTION,        Role.DPS },
            { Skills.DEMONOLOGY,        Role.DPS },
            { Skills.DESTRUCTION,       Role.DPS },
            { Skills.ARMS,              Role.DPS },
            { Skills.FURY,              Role.DPS },
            { Skills.HOLY,              Role.Healer },
            { Skills.DISCIPLINE,        Role.Healer },
            { Skills.RESTORATION,       Role.Healer },
            { Skills.PROTECTION,        Role.Tank },
        };

        public ActorDatabase()
        {
            connectionString = "Data Source=actors.db";
            InitializeDatabase();
        }

        // Initialize the SQLite database
        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(connectionString);
            connection.Open();

            var tableCommand = connection.CreateCommand();
            tableCommand.CommandText =
                @"CREATE TABLE IF NOT EXISTS Actor (
                        accountName TEXT NOT NULL,
                        name TEXT NOT NULL,
                        race TEXT NOT NULL,
                        class TEXT NOT NULL,
                        talent_spec TEXT,
                        profession1 TEXT,
                        profession2 TEXT,
                        openness REAL,
                        conscientiousness REAL,
                        extroversion REAL,
                        agreeableness REAL,
                        neuroticism REAL
                    );";

            tableCommand.ExecuteNonQuery();
            Console.WriteLine("Actor table created successfully in Actors DB.");
        }

        // Generate random personality values based on archetypes
        private static (float, float, float, float, float) GeneratePersonality(int archetype, Random rand)
        {
            return archetype switch
            {
                1 => (RandomRange(rand, 0.7f, 1.0f), RandomRange(rand, 0.9f, 1.0f), RandomRange(rand, 0.3f, 0.6f), RandomRange(rand, 0.4f, 0.7f), RandomRange(rand, 0.3f, 0.6f)),
                2 => (RandomRange(rand, 0.6f, 0.9f), RandomRange(rand, 0.7f, 0.9f), RandomRange(rand, 0.8f, 1.0f), RandomRange(rand, 0.3f, 0.5f), RandomRange(rand, 0.5f, 0.8f)),
                3 => (RandomRange(rand, 0.8f, 1.0f), RandomRange(rand, 0.4f, 0.6f), RandomRange(rand, 0.8f, 1.0f), RandomRange(rand, 0.8f, 1.0f), RandomRange(rand, 0.2f, 0.4f)),
                _ => (RandomRange(rand, 0.4f, 0.6f), RandomRange(rand, 0.7f, 0.9f), RandomRange(rand, 0.1f, 0.3f), RandomRange(rand, 0.6f, 0.9f), RandomRange(rand, 0.3f, 0.5f)),
            };
        }

        private static float RandomRange(Random rand, float min, float max)
        {
            return (float)(rand.NextDouble() * (max - min) + min);
        }

        // Generate the INSERT script with a 1:1:3 ratio of Tank, Healer, and DPS
        public static string GenerateInsertScript(int actorCount)
        {
            var sb = new StringBuilder();
            sb.AppendLine("INSERT INTO Actor (accountName, name, race, class, talent_spec, profession1, profession2, openness, conscientiousness, extroversion, agreeableness, neuroticism) VALUES");

            Random rand = new();
            int currentTankCount = 0, currentHealerCount = 0, currentDpsCount = 0;

            // Maintain the 1:1:3 ratio of tank, healer, DPS
            int tankLimit = actorCount / 5;
            int healerLimit = actorCount / 5;
            int dpsLimit = actorCount - tankLimit - healerLimit;

            foreach (var race in ClassesByRace.Keys)
            {
                var classes = ClassesByRace[race];
                foreach (var cls in classes)
                {
                    var specs = Specs[cls];
                    var professions = ClassProfessions[cls];

                    foreach (var spec in specs)
                    {
                        if ((SpecRoleFulfillment[spec] == Role.Tank && currentTankCount >= tankLimit) ||
                            (SpecRoleFulfillment[spec] == Role.Healer && currentHealerCount >= healerLimit) ||
                            (SpecRoleFulfillment[spec] == Role.DPS && currentDpsCount >= dpsLimit))
                            continue;

                        // Assign professions
                        var professionPair = professions[rand.Next(professions.Length)];

                        // Assign personality traits
                        var (openness, conscientiousness, extroversion, agreeableness, neuroticism) = GeneratePersonality(rand.Next(1, 4), rand);

                        // Generate player data
                        string accountName = $"Player_{cls}_{spec}";
                        string name = $"Bot_{cls}_{spec}";

                        sb.AppendLine($"('{accountName}', '{name}', '{race}', '{cls}', '{spec}', '{professionPair.Item1}', '{professionPair.Item2}', {openness:F2}, {conscientiousness:F2}, {extroversion:F2}, {agreeableness:F2}, {neuroticism:F2})");

                        // Track count for role distribution
                        if (SpecRoleFulfillment[spec] == Role.Tank) currentTankCount++;
                        else if (SpecRoleFulfillment[spec] == Role.Healer) currentHealerCount++;
                        else if (SpecRoleFulfillment[spec] == Role.DPS) currentDpsCount++;

                        if (currentTankCount >= tankLimit && currentHealerCount >= healerLimit && currentDpsCount >= dpsLimit)
                            break;
                    }
                    if (currentTankCount >= tankLimit && currentHealerCount >= healerLimit && currentDpsCount >= dpsLimit)
                        break;
                }
                if (currentTankCount >= tankLimit && currentHealerCount >= healerLimit && currentDpsCount >= dpsLimit)
                    break;
            }

            sb.Append(";");
            return sb.ToString();
        }
    }
}
