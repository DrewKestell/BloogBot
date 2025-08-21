using System.Text;
using GameData.Core.Enums;

namespace GameData.Core.Constants
{
    /// <summary>
    /// Druid shapeshift forms
    /// </summary>
    public enum DruidForm
    {
        Human = 0,
        Bear = 1,
        Cat = 2,
        Travel = 3,
        Aquatic = 4,
        Moonkin = 5
    }

    /// <summary>
    /// Race dimensions system with integrated DBC reader
    /// Automatically loads CreatureModelData.dbc from local directory
    /// 
    /// Usage examples:
    /// - Get human male dimensions: GetCapsuleForRace(Race.Human, Gender.Male)
    /// - Get female orc dimensions: GetCapsuleForRace(Race.Orc, Gender.Female)
    /// - Get night elf bear form: GetCapsuleForDruidForm(Race.NightElf, DruidForm.Bear)
    /// - Check all loaded models: GetAllRaceGenderDimensions()
    /// 
    /// Note: All methods require Gender to be specified explicitly.
    /// </summary>
    public static class RaceDimensions
    {
        #region Private Fields

        private static Dictionary<uint, CreatureModelDataRecord> modelDataCache;

        // Enhanced caching system
        private static Dictionary<(Race race, Gender gender), (float radius, float height)> raceGenderCache;
        private static Dictionary<(Race race, DruidForm form), (float radius, float height)> druidFormCache;

        // Legacy support
        private static Dictionary<Race, (float radius, float height)> dimensionCache;

        // Raw data storage
        private static Dictionary<string, (uint id, float radius, float height)> playerModelDimensions;
        private static Dictionary<string, (uint id, float radius, float height)> druidFormDimensions;
        private static bool isInitialized = false;

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns collision capsule dimensions for a given character race (defaults to male).
        /// Data is loaded from CreatureModelData.dbc if available.
        /// </summary>
        public static (float radius, float height) GetCapsuleForRace(Race race)
        {
            return GetCapsuleForRace(race, Gender.Male);
        }

        /// <summary>
        /// Returns collision capsule dimensions for a specific race and gender.
        /// </summary>
        public static (float radius, float height) GetCapsuleForRace(Race race, Gender gender)
        {
            if (!isInitialized)
                Initialize();

            if (raceGenderCache != null && raceGenderCache.TryGetValue((race, gender), out var dimensions))
                return dimensions;

            // Try opposite gender as fallback
            var oppositeGender = gender == Gender.Male ? Gender.Female : Gender.Male;
            if (raceGenderCache != null && raceGenderCache.TryGetValue((race, oppositeGender), out dimensions))
                return dimensions;

            // Default fallback
            return (0.306f, 2.0313f);
        }

        /// <summary>
        /// Returns collision capsule dimensions for druid forms.
        /// </summary>
        public static (float radius, float height) GetCapsuleForDruidForm(Race race, DruidForm form)
        {
            if (!isInitialized)
                Initialize();

            // Only Night Elf and Tauren can be druids
            if (race != Race.NightElf && race != Race.Tauren)
                return GetCapsuleForRace(race); // Return base race dimensions

            // Humanoid form returns race dimensions
            if (form == DruidForm.Human)
                return GetCapsuleForRace(race);

            if (druidFormCache != null && druidFormCache.TryGetValue((race, form), out var dimensions))
                return dimensions;

            // Try to get generic form (not race-specific)
            var genericRace = race == Race.NightElf ? Race.Tauren : Race.NightElf;
            if (druidFormCache != null && druidFormCache.TryGetValue((genericRace, form), out dimensions))
                return dimensions;

            // Fallback to humanoid form
            return GetCapsuleForRace(race);
        }

        /// <summary>
        /// Gets the minimum swim depth for a race.
        /// In vmangos: collision_height * 0.75f
        /// </summary>
        public static float GetMinSwimDepth(Race race)
        {
            var (_, height) = GetCapsuleForRace(race);
            return height * 0.75f;
        }

        /// <summary>
        /// Gets the combat reach for a race.
        /// </summary>
        public static float GetCombatReach(Race race)
        {
            return GetCombatReach(race, Gender.Male);
        }

        /// <summary>
        /// Gets the combat reach for a specific race and gender.
        /// Combat reach is typically proportional to the bounding radius.
        /// </summary>
        public static float GetCombatReach(Race race, Gender gender)
        {
            // Special case for Tauren - they have extended reach
            if (race == Race.Tauren)
                return 1.95f;

            // For other races, standard reach
            return 1.5f;
        }

        /// <summary>
        /// Gets the bounding radius for a race.
        /// </summary>
        public static float GetBoundingRadius(Race race)
        {
            var (radius, _) = GetCapsuleForRace(race);
            return radius;
        }

        /// <summary>
        /// Gets the bounding radius for a specific race and gender
        /// </summary>
        public static float GetBoundingRadius(Race race, Gender gender)
        {
            var (radius, _) = GetCapsuleForRace(race, gender);
            return radius;
        }

        /// <summary>
        /// Gets the collision height for a race.
        /// </summary>
        public static float GetCollisionHeight(Race race)
        {
            var (_, height) = GetCapsuleForRace(race);
            return height;
        }

        /// <summary>
        /// Gets the collision height for a specific race and gender
        /// </summary>
        public static float GetCollisionHeight(Race race, Gender gender)
        {
            var (_, height) = GetCapsuleForRace(race, gender);
            return height;
        }

        /// <summary>
        /// Gets all loaded player model dimensions by race and gender
        /// </summary>
        public static Dictionary<(Race race, Gender gender), (float radius, float height)> GetAllRaceGenderDimensions()
        {
            if (!isInitialized)
                Initialize();
            return new Dictionary<(Race, Gender), (float, float)>(raceGenderCache ?? new Dictionary<(Race, Gender), (float, float)>());
        }

        /// <summary>
        /// Gets all loaded druid form dimensions by race and form
        /// </summary>
        public static Dictionary<(Race race, DruidForm form), (float radius, float height)> GetAllDruidFormDimensions()
        {
            if (!isInitialized)
                Initialize();
            return new Dictionary<(Race, DruidForm), (float, float)>(druidFormCache ?? new Dictionary<(Race, DruidForm), (float, float)>());
        }

        /// <summary>
        /// Gets all loaded player model dimensions (raw data)
        /// </summary>
        public static Dictionary<string, (uint id, float radius, float height)> GetAllPlayerModels()
        {
            if (!isInitialized)
                Initialize();
            return playerModelDimensions ?? new Dictionary<string, (uint, float, float)>();
        }

        /// <summary>
        /// Gets all loaded druid form dimensions (raw data)
        /// </summary>
        public static Dictionary<string, (uint id, float radius, float height)> GetAllDruidForms()
        {
            if (!isInitialized)
                Initialize();
            return druidFormDimensions ?? new Dictionary<string, (uint, float, float)>();
        }

        /// <summary>
        /// Debug method to check ModelScale values for all player models
        /// </summary>
        public static void DebugModelScales()
        {
            if (!isInitialized)
                Initialize();

            var playerModels = new List<(string name, uint id, float scale, float width, float height)>();

            foreach (var kvp in modelDataCache)
            {
                var record = kvp.Value;
                if (record.ModelPath != null && record.ModelPath.StartsWith("Character\\"))
                {
                    string modelName = ExtractModelName(record.ModelPath);
                    playerModels.Add((modelName, record.Id, record.ModelScale, record.CollisionWidth, record.CollisionHeight));
                }
            }

            // Sort by model name
            playerModels.Sort((a, b) => a.name.CompareTo(b.name));

            foreach (var model in playerModels)
            {
                float scaledWidth = model.width * model.scale;
                float scaledHeight = model.height * model.scale;

                ConsoleColor originalColor = Console.ForegroundColor;
                if (Math.Abs(model.scale - 1.0f) > 0.001f)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                }

                Console.ForegroundColor = originalColor;
            }

            // Check druid forms too
            var druidModels = new List<(string name, uint id, float scale, float width, float height)>();

            foreach (var kvp in modelDataCache)
            {
                var record = kvp.Value;
                if (record.ModelPath != null &&
                    (record.ModelPath.Contains("Druid") ||
                     record.ModelPath.Contains("Moonkin") ||
                     record.ModelPath.Contains("SeaLion")))
                {
                    string modelName = ExtractModelName(record.ModelPath);
                    druidModels.Add((modelName, record.Id, record.ModelScale, record.CollisionWidth, record.CollisionHeight));
                }
            }

            if (druidModels.Count > 0)
            {

                foreach (var model in druidModels)
                {
                    float scaledWidth = model.width * model.scale;
                    float scaledHeight = model.height * model.scale;

                    ConsoleColor originalColor = Console.ForegroundColor;
                    if (Math.Abs(model.scale - 1.0f) > 0.001f)
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    Console.ForegroundColor = originalColor;
                }
            }
        }

        /// <summary>
        /// Gets all dimensions for a specific race (both genders and druid forms if applicable)
        /// </summary>
        public static Dictionary<string, (float radius, float height)> GetAllDimensionsForRace(Race race)
        {
            if (!isInitialized)
                Initialize();

            var result = new Dictionary<string, (float radius, float height)>();

            // Add gender dimensions
            if (raceGenderCache.TryGetValue((race, Gender.Male), out var maleDims))
                result["Male"] = maleDims;

            if (raceGenderCache.TryGetValue((race, Gender.Female), out var femaleDims))
                result["Female"] = femaleDims;

            // Add druid forms if applicable
            if (CanBeDruid(race))
            {
                foreach (DruidForm form in Enum.GetValues(typeof(DruidForm)))
                {
                    if (form == DruidForm.Human) continue;

                    if (druidFormCache.TryGetValue((race, form), out var formDims))
                        result[$"{form} Form"] = formDims;
                }
            }

            return result;
        }

        /// <summary>
        /// Prints dimensions for a specific query (for debugging)
        /// </summary>
        public static void PrintDimensions(Race race, Gender gender, DruidForm? form = null)
        {
            if (!isInitialized)
                Initialize();

            if (form.HasValue && CanBeDruid(race))
            {
                var (radius, height) = GetCapsuleForDruidForm(race, form.Value);
            }
            else
            {
                var (radius, height) = GetCapsuleForRace(race, gender);
            }
        }

        /// <summary>
        /// Checks if a race can be a druid
        /// </summary>
        public static bool CanBeDruid(Race race)
        {
            return race == Race.NightElf || race == Race.Tauren;
        }

        /// <summary>
        /// Gets the swim depth for a specific race and gender
        /// </summary>
        public static float GetMinSwimDepth(Race race, Gender gender)
        {
            var (_, height) = GetCapsuleForRace(race, gender);
            return height * 0.75f;
        }

        #endregion

        #region Private Initialization Methods

        private static void Initialize()
        {
            if (isInitialized) return;

            // Initialize all caches
            dimensionCache = new Dictionary<Race, (float radius, float height)>();
            raceGenderCache = new Dictionary<(Race, Gender), (float radius, float height)>();
            druidFormCache = new Dictionary<(Race, DruidForm), (float radius, float height)>();
            playerModelDimensions = new Dictionary<string, (uint, float, float)>();
            druidFormDimensions = new Dictionary<string, (uint, float, float)>();

            try
            {
                string dbcPath = FindDbcFile();

                if (dbcPath == null)
                {
                    UseHardcodedValues();
                    isInitialized = true;
                    return;
                }

                LoadModelData(dbcPath);
                CacheRaceDimensions();
                CacheDruidFormDimensions();
                isInitialized = true;
            }
            catch (Exception ex)
            {
                UseHardcodedValues();
                isInitialized = true;
            }
        }

        private static string FindDbcFile()
        {
            string[] possiblePaths = new[]
            {
                "CreatureModelData.dbc",
                Path.Combine("dbc", "CreatureModelData.dbc"),
                Path.Combine("Data", "CreatureModelData.dbc"),
                Path.Combine("Data", "dbc", "CreatureModelData.dbc"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "CreatureModelData.dbc"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dbc", "CreatureModelData.dbc")
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                    return path;
            }

            return null;
        }

        private static void LoadModelData(string filePath)
        {
            modelDataCache = new Dictionary<uint, CreatureModelDataRecord>();

            using (var reader = new DBCReader(filePath))
            {
                foreach (var record in reader.ReadRecords<CreatureModelDataRecord>())
                {
                    modelDataCache[record.Id] = record;

                    if (record.ModelPath == null) continue;

                    string modelPath = record.ModelPath;
                    string modelName = ExtractModelName(modelPath);

                    // Apply ModelScale to collision dimensions if it's not 1.0
                    float scaledWidth = record.CollisionWidth * record.ModelScale;
                    float scaledHeight = record.CollisionHeight * record.ModelScale;


                    // Check for player models
                    if (modelPath.StartsWith("Character\\"))
                    {
                        playerModelDimensions[modelName] = (record.Id, scaledWidth, scaledHeight);
                    }
                    // Check for druid forms (various patterns)
                    else if (modelPath.Contains("Druid") ||
                             modelPath.Contains("Moonkin") ||
                             modelPath.Contains("SeaLion") ||
                             modelPath.Contains("CatForm") ||
                             modelPath.Contains("BearForm") ||
                             modelPath.Contains("TravelForm"))
                    {
                        druidFormDimensions[modelName] = (record.Id, scaledWidth, scaledHeight);
                    }
                }
            }
        }

        private static string ExtractModelName(string modelPath)
        {
            // Extract just the model name from the path
            // e.g., "Character\Human\Male\HumanMale.mdx" -> "HumanMale"
            int lastSlash = modelPath.LastIndexOf('\\');
            if (lastSlash >= 0)
            {
                string filename = modelPath.Substring(lastSlash + 1);
                int dotPos = filename.LastIndexOf('.');
                if (dotPos > 0)
                    return filename.Substring(0, dotPos);
                return filename;
            }
            return modelPath;
        }

        private static void CacheRaceDimensions()
        {
            // Map race and gender combinations to model names
            var raceGenderMap = new Dictionary<(Race, Gender), string[]>
            {
                // Human
                { (Race.Human, Gender.Male), new[] { "HumanMale" } },
                { (Race.Human, Gender.Female), new[] { "HumanFemale" } },
                
                // Orc
                { (Race.Orc, Gender.Male), new[] { "OrcMale" } },
                { (Race.Orc, Gender.Female), new[] { "OrcFemale" } },
                
                // Dwarf
                { (Race.Dwarf, Gender.Male), new[] { "DwarfMale" } },
                { (Race.Dwarf, Gender.Female), new[] { "DwarfFemale" } },
                
                // Night Elf
                { (Race.NightElf, Gender.Male), new[] { "NightElfMale" } },
                { (Race.NightElf, Gender.Female), new[] { "NightElfFemale" } },
                
                // Undead (may be called Scourge in some versions)
                { (Race.Undead, Gender.Male), new[] { "UndeadMale", "ScourgeMale" } },
                { (Race.Undead, Gender.Female), new[] { "UndeadFemale", "ScourgeFemale" } },
                
                // Tauren
                { (Race.Tauren, Gender.Male), new[] { "TaurenMale" } },
                { (Race.Tauren, Gender.Female), new[] { "TaurenFemale" } },
                
                // Gnome
                { (Race.Gnome, Gender.Male), new[] { "GnomeMale" } },
                { (Race.Gnome, Gender.Female), new[] { "GnomeFemale" } },
                
                // Troll
                { (Race.Troll, Gender.Male), new[] { "TrollMale" } },
                { (Race.Troll, Gender.Female), new[] { "TrollFemale" } }
            };

            // Cache race+gender combinations
            foreach (var kvp in raceGenderMap)
            {
                var (race, gender) = kvp.Key;

                foreach (var modelName in kvp.Value)
                {
                    if (playerModelDimensions.TryGetValue(modelName, out var dims))
                    {
                        // Note: dimensions are already scaled in LoadModelData
                        raceGenderCache[(race, gender)] = (dims.radius, dims.height);

                        // Also update legacy cache for male models
                        if (gender == Gender.Male)
                        {
                            dimensionCache[race] = (dims.radius, dims.height);
                        }
                        break;
                    }
                }
            }

            // If no models found, use hardcoded values
            if (raceGenderCache.Count == 0)
            {
                UseHardcodedValues();
            }
        }

        private static void CacheDruidFormDimensions()
        {
            // Map druid forms to their model patterns
            var formPatterns = new Dictionary<DruidForm, string[]>
            {
                { DruidForm.Bear, new[] { "druidbear", "bearform", "bear" } },
                { DruidForm.Cat, new[] { "druidcat", "catform", "cat" } },
                { DruidForm.Travel, new[] { "druidtravel", "travelform", "travel", "cheetah" } },
                { DruidForm.Aquatic, new[] { "druidaquatic", "sealion", "aquaticform", "aquatic" } },
                { DruidForm.Moonkin, new[] { "moonkin", "druidmoonkin", "owlkin" } }
            };

            foreach (var druidModel in druidFormDimensions)
            {
                string modelName = druidModel.Key.ToLower();
                var dims = druidModel.Value;

                // Determine which race this form is for
                Race? race = null;
                if (modelName.Contains("tauren"))
                    race = Race.Tauren;
                else if (modelName.Contains("nightelf") || modelName.Contains("nelf") || modelName.Contains("ne_"))
                    race = Race.NightElf;

                // Determine which form this is
                DruidForm? form = null;
                foreach (var formPattern in formPatterns)
                {
                    foreach (var pattern in formPattern.Value)
                    {
                        if (modelName.Contains(pattern))
                        {
                            form = formPattern.Key;
                            break;
                        }
                    }
                    if (form.HasValue) break;
                }

                // Cache the form if we identified it
                if (form.HasValue)
                {
                    if (race.HasValue)
                    {
                        // Race-specific form
                        druidFormCache[(race.Value, form.Value)] = (dims.radius, dims.height);
                    }
                    else
                    {
                        // Generic form - apply to both races
                        druidFormCache[(Race.NightElf, form.Value)] = (dims.radius, dims.height);
                        druidFormCache[(Race.Tauren, form.Value)] = (dims.radius, dims.height);
                    }
                }
            }
        }
        
        private static void UseHardcodedValues()
        {
            // Fallback values for male models
            var maleDefaults = new Dictionary<Race, (float, float)>
            {
                { Race.Human, (0.306f, 2.0313f) },
                { Race.Dwarf, (0.347f, 1.4688f) },
                { Race.Gnome, (0.2999f, 1.0625f) },
                { Race.NightElf, (0.306f, 2.4375f) },
                { Race.Orc, (0.3718f, 2.2813f) },
                { Race.Undead, (0.3836f, 1.9063f) },
                { Race.Troll, (0.3718f, 2.5313f) },
                { Race.Tauren, (0.9747f, 2.625f) }
            };

            // Female models are typically slightly smaller
            var femaleDefaults = new Dictionary<Race, (float, float)>
            {
                { Race.Human, (0.208f, 1.7906f) },
                { Race.Dwarf, (0.347f, 1.35f) },
                { Race.Gnome, (0.2999f, 0.9844f) },
                { Race.NightElf, (0.306f, 2.3125f) },
                { Race.Orc, (0.2556f, 2.0313f) },
                { Race.Undead, (0.3565f, 1.6563f) },
                { Race.Troll, (0.4279f, 2.1406f) },
                { Race.Tauren, (0.9747f, 2.375f) }
            };

            // Populate all caches
            foreach (var kvp in maleDefaults)
            {
                dimensionCache[kvp.Key] = kvp.Value;
                raceGenderCache[(kvp.Key, Gender.Male)] = kvp.Value;
            }

            foreach (var kvp in femaleDefaults)
            {
                raceGenderCache[(kvp.Key, Gender.Female)] = kvp.Value;
            }

            // Hardcoded druid form estimates
            druidFormCache[(Race.NightElf, DruidForm.Bear)] = (1.2f, 2.5f);
            druidFormCache[(Race.NightElf, DruidForm.Cat)] = (0.8f, 1.5f);
            druidFormCache[(Race.Tauren, DruidForm.Bear)] = (1.5f, 3.0f);
            druidFormCache[(Race.Tauren, DruidForm.Cat)] = (1.0f, 1.8f);
        }

        #endregion

        #region Integrated DBC Reader

        /// <summary>
        /// DBC file reader
        /// </summary>
        private class DBCReader : IDisposable
        {
            private const uint DBC_HEADER = 0x43424457; // "WDBC"
            private BinaryReader reader;
            private DBCHeader header;
            private byte[] stringTable;

            public struct DBCHeader
            {
                public uint Magic;
                public uint RecordCount;
                public uint FieldCount;
                public uint RecordSize;
                public uint StringTableSize;
            }

            public uint RecordCount => header.RecordCount;

            public DBCReader(string filePath)
            {
                reader = new BinaryReader(File.OpenRead(filePath));
                ReadHeader();
                LoadStringTable();
            }

            private void ReadHeader()
            {
                header = new DBCHeader
                {
                    Magic = reader.ReadUInt32(),
                    RecordCount = reader.ReadUInt32(),
                    FieldCount = reader.ReadUInt32(),
                    RecordSize = reader.ReadUInt32(),
                    StringTableSize = reader.ReadUInt32()
                };

                if (header.Magic != DBC_HEADER)
                    throw new InvalidDataException("Invalid DBC header");

                // Set the record size for CreatureModelDataRecord
                CreatureModelDataRecord.SetRecordSize((int)header.RecordSize);
            }

            private void LoadStringTable()
            {
                long stringTablePos = 20 + (header.RecordCount * header.RecordSize);
                reader.BaseStream.Seek(stringTablePos, SeekOrigin.Begin);

                if (header.StringTableSize > 0)
                {
                    if (stringTablePos + header.StringTableSize > reader.BaseStream.Length)
                    {
                        int availableBytes = (int)(reader.BaseStream.Length - stringTablePos);
                        stringTable = reader.ReadBytes(availableBytes);
                    }
                    else
                    {
                        stringTable = reader.ReadBytes((int)header.StringTableSize);
                    }
                }
                else
                {
                    stringTable = new byte[0];
                }
            }

            public string GetString(int offset)
            {
                if (offset == 0 || stringTable == null || offset < 0 || offset >= stringTable.Length)
                    return string.Empty;

                int end = offset;
                while (end < stringTable.Length && stringTable[end] != 0)
                    end++;

                return Encoding.UTF8.GetString(stringTable, offset, end - offset);
            }

            public IEnumerable<T> ReadRecords<T>() where T : IDBCRecord, new()
            {
                reader.BaseStream.Seek(20, SeekOrigin.Begin);

                for (uint i = 0; i < header.RecordCount; i++)
                {
                    T record = new T();
                    long positionBefore = reader.BaseStream.Position;
                    record.Read(reader, this);

                    // Ensure we're at the right position for next record
                    long correctPosition = positionBefore + header.RecordSize;
                    if (reader.BaseStream.Position != correctPosition)
                    {
                        reader.BaseStream.Seek(correctPosition, SeekOrigin.Begin);
                    }

                    yield return record;
                }
            }

            public void Dispose()
            {
                reader?.Dispose();
            }
        }

        private interface IDBCRecord
        {
            void Read(BinaryReader reader, DBCReader dbc);
        }

        /// <summary>
        /// CreatureModelData.dbc record structure - Auto-detects version
        /// </summary>
        private class CreatureModelDataRecord : IDBCRecord
        {
            private static int recordSize = 0;

            public uint Id { get; private set; }
            public uint Flags { get; private set; }
            public string ModelPath { get; private set; }
            public uint SizeClass { get; private set; }
            public float ModelScale { get; private set; }
            public uint BloodLevel { get; private set; }
            public uint FootprintTextureId { get; private set; }
            public float FootprintTextureLength { get; private set; }
            public float FootprintTextureWidth { get; private set; }
            public float FootprintParticleScale { get; private set; }
            public uint FoleyMaterialId { get; private set; }
            public uint FootstepShakeSize { get; private set; }
            public uint DeathThudShakeSize { get; private set; }
            public uint SoundData { get; private set; }
            public float CollisionWidth { get; private set; }
            public float CollisionHeight { get; private set; }
            public float MountHeight { get; private set; }
            public float BoundingRadius { get; private set; }
            public float BoundingHeight { get; private set; }
            public float CollisionOffset { get; private set; }

            public static void SetRecordSize(int size)
            {
                recordSize = size;
            }

            public void Read(BinaryReader reader, DBCReader dbc)
            {
                // Read common fields (1-14)
                Id = reader.ReadUInt32();
                Flags = reader.ReadUInt32();
                int modelPathOffset = reader.ReadInt32();
                ModelPath = dbc.GetString(modelPathOffset);

                SizeClass = reader.ReadUInt32();
                ModelScale = reader.ReadSingle();
                BloodLevel = reader.ReadUInt32();
                FootprintTextureId = reader.ReadUInt32();
                FootprintTextureLength = reader.ReadSingle();
                FootprintTextureWidth = reader.ReadSingle();
                FootprintParticleScale = reader.ReadSingle();
                FoleyMaterialId = reader.ReadUInt32();
                FootstepShakeSize = reader.ReadUInt32();
                DeathThudShakeSize = reader.ReadUInt32();
                SoundData = reader.ReadUInt32();

                // Fields 15-16 (collision data - present in all versions)
                CollisionWidth = reader.ReadSingle();
                CollisionHeight = reader.ReadSingle();

                // Additional fields based on version
                if (recordSize == 80)
                {
                    // WoW 1.12.1 format - fields 17-20
                    MountHeight = reader.ReadSingle();
                    BoundingRadius = reader.ReadSingle();
                    BoundingHeight = reader.ReadSingle();
                    CollisionOffset = reader.ReadSingle();
                }
                else if (recordSize == 96)
                {
                    // TBC/WotLK format - fields 17-24
                    MountHeight = reader.ReadSingle();
                    BoundingRadius = reader.ReadSingle();
                    BoundingHeight = reader.ReadSingle();
                    CollisionOffset = reader.ReadSingle();

                    // Skip extra fields
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                    reader.ReadSingle();
                }
                else
                {
                    // 64-byte format - use collision as bounding
                    MountHeight = 0.0f;
                    BoundingRadius = CollisionWidth;
                    BoundingHeight = CollisionHeight;
                    CollisionOffset = 0.0f;
                }
            }
        }

        #endregion
    }
}