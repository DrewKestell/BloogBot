namespace PromptHandlingService.Predefined.MaNGOSConfigHandlers
{
    public class ConfigEditorFunction(IPromptRunner promptRunner) : PromptFunctionBase(promptRunner)
    {
        public class ConfigEditContext
        {
            public string Variable { get; set; } = string.Empty;
            public string NewValue { get; set; } = string.Empty;

            public override string ToString()
            {
                return $"Variable: {Variable}, New Value: {NewValue}";
            }
        }

        public static async Task<string> EditConfig(IPromptRunner promptRunner, ConfigEditContext context, CancellationToken cancellationToken)
        {
            var configEditorFunction = new ConfigEditorFunction(promptRunner)
            {
                ConfigContextVal = context
            };
            await configEditorFunction.CompleteAsync(cancellationToken);
            return configEditorFunction.EditedConfig;
        }

        public ConfigEditContext ConfigContextVal
        {
            get => GetParameter<ConfigEditContext>();
            set
            {
                SetParameter(value: value);
                ResetChat();
            }
        }

        private string? _editedConfig;

        // ReSharper disable once MemberCanBePrivate.Global
        public string EditedConfig => _editedConfig ?? throw new NullReferenceException("The config change has not been set. Call 'CompleteAsync' to set the config.");

        protected override string SystemPrompt => "You are a World of Warcraft MaNGOS config editor. Modify the correct config setting based on the input and validate the change based on allowed ranges.";

        public override async Task CompleteAsync(CancellationToken cancellationToken)
        {
            var variableInfo = _configParameters.GetValueOrDefault(ConfigContextVal.Variable);

            if (variableInfo == default)
            {
                _editedConfig = "Error: Unknown configuration variable.";
                return;
            }

            if (int.TryParse(ConfigContextVal.NewValue, out var newValue))
            {
                if (variableInfo.Range.HasValue && (newValue < variableInfo.Range.Value.Min || newValue > variableInfo.Range.Value.Max))
                {
                    _editedConfig = $"Error: The value for {ConfigContextVal.Variable} must be between {variableInfo.Range.Value.Min} and {variableInfo.Range.Value.Max}.";
                    return;
                }
                else
                {
                    _editedConfig = $"{ConfigContextVal.Variable} updated to {ConfigContextVal.NewValue}.";
                    return;
                }
            }

            _editedConfig = "Error: Invalid value provided.";
        }

        protected override void InitializeChat() { }

        private readonly Dictionary<string, (Type ValueType, object DefaultValue, (double Min, double Max)? Range, string Description)> _configParameters = new()
        {
            // Connections and Directories
            { "RealmID", (typeof(int), 1, (1, 255), "RealmID must match the realmlist inside the realmd database.") },
            { "DataDir", (typeof(string), "/opt/vanilla/data", null, "Path to the Data directory.") },
            { "LogsDir", (typeof(string), "", null, "Path to the Logs directory.") },
            { "LoginDatabase.Info", (typeof(string), "wow-vanilla-database;3306;app;app;realmd", null, "Connection info for Login Database.") },
            { "LoginDatabase.Connections", (typeof(int), 1, (1, 16), "Number of connections for Login Database.") },
            { "LoginDatabase.WorkerThreads", (typeof(int), 1, (1, 16), "Number of worker threads for Login Database.") },
            { "WorldDatabase.Info", (typeof(string), "wow-vanilla-database;3306;app;app;mangos", null, "Connection info for World Database.") },
            { "WorldDatabase.Connections", (typeof(int), 1, (1, 16), "Number of connections for World Database.") },
            { "WorldDatabase.WorkerThreads", (typeof(int), 1, (1, 16), "Number of worker threads for World Database.") },
            { "CharacterDatabase.Info", (typeof(string), "wow-vanilla-database;3306;app;app;characters", null, "Connection info for Character Database.") },
            { "CharacterDatabase.Connections", (typeof(int), 1, (1, 16), "Number of connections for Character Database.") },
            { "CharacterDatabase.WorkerThreads", (typeof(int), 1, (1, 16), "Number of worker threads for Character Database.") },
            { "LogsDatabase.Info", (typeof(string), "wow-vanilla-database;3306;app;app;logs", null, "Connection info for Logs Database.") },
            { "LogsDatabase.Connections", (typeof(int), 1, (1, 16), "Number of connections for Logs Database.") },
            { "LogsDatabase.WorkerThreads", (typeof(int), 1, (1, 16), "Number of worker threads for Logs Database.") },
            { "MaxPingTime", (typeof(int), 30, (1, 120), "Maximum ping time interval in minutes.") },
            { "WorldServerPort", (typeof(int), 8085, (1024, 65535), "Port on which the server will listen.") },
            { "BindIP", (typeof(string), "0.0.0.0", null, "IP address or hostname the World Server binds to.") },
            
            // Performance Settings
            { "UseProcessors", (typeof(int), 0, null, "Used processors mask for multi-processors system.") },
            { "ProcessPriority", (typeof(int), 1, (0, 1), "Process priority setting. 0 = Normal, 1 = High.") },
            { "Compression", (typeof(int), 1, (1, 9), "Compression level for update packages sent to client. 1 = Speed, 9 = Best compression.") },
            { "PlayerLimit", (typeof(int), 100, (-3, int.MaxValue), "Initial realm capacity. Negative values restrict based on admin/GM status.") },
            { "PlayerHardLimit", (typeof(int), 0, (0, int.MaxValue), "Maximum number of players in the world after increasing PlayerLimit.") },
            { "LoginQueue.GracePeriodSecs", (typeof(int), 0, (0, int.MaxValue), "Time to log in after last logout without queue.") },
            { "LoginPerTick", (typeof(int), 0, (0, int.MaxValue), "Max number of players allowed to log in during a world update tick.") },
            { "CharacterScreenMaxIdleTime", (typeof(int), 900, (0, int.MaxValue), "Idle time allowed on the character screen before disconnecting.") },
            { "SaveRespawnTimeImmediately", (typeof(int), 1, (0, 1), "Save respawn time immediately or wait until grid unload.") },
            { "MaxOverspeedPings", (typeof(int), 2, (0, int.MaxValue), "Maximum overspeed ping count before player kick.") },
            { "GridUnload", (typeof(int), 1, (0, 1), "Whether or not to unload grids.") },
            { "GridCleanUpDelay", (typeof(int), 300000, (0, int.MaxValue), "Grid clean-up delay in milliseconds.") },
            { "MapUpdateInterval", (typeof(int), 100, (1, int.MaxValue), "Map update interval in milliseconds.") },
            { "ChangeWeatherInterval", (typeof(int), 600000, (1, int.MaxValue), "Weather update interval in milliseconds.") },
            { "PlayerSave.Interval", (typeof(int), 900000, (1, int.MaxValue), "Player save interval in milliseconds.") },
            { "PlayerSave.Stats.MinLevel", (typeof(int), 0, (0, int.MaxValue), "Minimum level for saving character stats.") },
            { "PlayerSave.Stats.SaveOnlyOnLogout", (typeof(int), 1, (0, 1), "Save stats only on logout or during player saves.") },
            { "vmap.enableLOS", (typeof(int), 1, (0, 1), "Enable or disable VMap support for line of sight.") },
            { "vmap.enableHeight", (typeof(int), 1, (0, 1), "Enable or disable VMap support for height calculation.") },
            { "vmap.ignoreSpellIds", (typeof(string), "7720", null, "Comma-separated list of spell IDs ignored for line of sight calculation.") },
            { "vmap.enableIndoorCheck", (typeof(int), 1, (0, 1), "Enable/disable indoor checks for outdoor-only auras.") },
            { "Collision.Models.Unload", (typeof(int), 1, (0, 1), "Free model when no one uses it anymore.") },
            { "DetectPosCollision", (typeof(int), 1, (0, 1), "Check for visible collisions at target positions.") },
            { "TargetPosRecalculateRange", (typeof(double), 1.5, (0.5, 5), "Max distance from movement target point to trigger recalculation.") },
            { "UpdateUptimeInterval", (typeof(int), 10, (1, int.MaxValue), "Update realm uptime interval in minutes.") },
            { "MaxCoreStuckTime", (typeof(int), 0, (0, int.MaxValue), "Max allowed core stuck time before forced crash.") },
            { "AddonChannel", (typeof(int), 1, (0, 1), "Permit or disable the use of addon channels.") },
            { "CleanCharacterDB", (typeof(int), 1, (0, 1), "Enable or disable character DB cleanups on startup.") },
            { "WowPatch", (typeof(int), 10, (0, 10), "Server's WoW patch version.") },

            // Server Logging Settings
            { "LogSQL", (typeof(int), 1, (0, 1), "Enable logging of GM commands to a SQL log file.") },
            { "PidFile", (typeof(string), "", null, "World daemon PID file.") },
            { "LogLevel", (typeof(int), 3, (0, 3), "Server console logging level. 0 = Minimum; 3 = Full/Debug.") },
            { "LogTime", (typeof(int), 0, (0, 1), "Include time in server console output.") },
            { "LogFile", (typeof(string), "Server.log", null, "Server log file name.") },
            { "LogTimestamp", (typeof(int), 0, (0, 1), "Include timestamp in log file name.") },
            { "LogFileLevel", (typeof(int), 0, (0, 3), "Logging level for server log files.") },
            { "LogFilter_TransportMoves", (typeof(int), 1, (0, 1), "Filter transport movement logs.") },
            { "LogFilter_CreatureMoves", (typeof(int), 1, (0, 1), "Filter creature movement logs.") },
            { "LogFilter_VisibilityChanges", (typeof(int), 1, (0, 1), "Filter visibility change logs.") },
            { "LogFilter_Weather", (typeof(int), 1, (0, 1), "Filter weather logs.") },
            { "LogFilter_PlayerStats", (typeof(int), 0, (0, 1), "Filter player statistics logs.") },
            { "LogFilter_SQLText", (typeof(int), 0, (0, 1), "Filter SQL text logs.") },
            { "LogFilter_PlayerMoves", (typeof(int), 0, (0, 1), "Filter player movement logs.") },
            { "LogFilter_PeriodicAffects", (typeof(int), 0, (0, 1), "Filter periodic affect logs.") },
            { "LogFilter_AIAndMovegens", (typeof(int), 0, (0, 1), "Filter AI and move generator logs.") },
            { "LogFilter_Damage", (typeof(int), 0, (0, 1), "Filter damage logs.") },
            { "LogFilter_Combat", (typeof(int), 0, (0, 1), "Filter combat logs.") },
            { "LogFilter_SpellCast", (typeof(int), 0, (0, 1), "Filter spell casting logs.") },
            { "LogFilter_DbStrictedCheck", (typeof(int), 1, (0, 1), "Filter DB stricted check logs.") },
            { "WorldLogFile", (typeof(string), "world.log", null, "Worldserver packet logging file.") },
            { "WorldLogTimestamp", (typeof(int), 0, (0, 1), "Include timestamp in world log file name.") },
            { "DBErrorLogFile", (typeof(string), "DBErrors.log", null, "Database error log file.") },
            { "DBErrorFixFile", (typeof(string), "", null, "SQL fix request log file for database errors.") },
            { "CharLogFile", (typeof(string), "Char.log", null, "Character operations log file.") },
            { "CharLogTimestamp", (typeof(int), 0, (0, 1), "Include timestamp in character log file name.") },
            { "CharLogDump", (typeof(int), 0, (0, 1), "Write character dump before deleting to Char.log.") },
            { "GmLogFile", (typeof(string), "", null, "GM log file.") },
            { "GmLogTimestamp", (typeof(int), 0, (0, 1), "Include timestamp in GM log file name.") },
            { "GmLogPerAccount", (typeof(int), 0, (0, 1), "Log GM data to account-specific log files.") },
            { "CriticalCommandsLogFile", (typeof(string), "", null, "Log file for critical GM commands.") },
            { "RaLogFile", (typeof(string), "", null, "RA commands log file.") },
            { "LogColors", (typeof(string), "", null, "Set console log message colors in format 'normal_color details_color debug_color error_color'.") },
            { "PerformanceLog.File", (typeof(string), "perf.log", null, "File for performance logging.") },
            { "PerformanceLog.SlowWorldUpdate", (typeof(int), 100, (0, int.MaxValue), "Threshold for slow world updates in milliseconds.") },
            { "PerformanceLog.SlowMapSystemUpdate", (typeof(int), 100, (0, int.MaxValue), "Threshold for slow map system updates in milliseconds.") },
            { "PerformanceLog.SlowSessionsUpdate", (typeof(int), 100, (0, int.MaxValue), "Threshold for slow session updates in milliseconds.") },
            { "PerformanceLog.SlowUniqueSessionUpdate", (typeof(int), 20, (0, int.MaxValue), "Threshold for slow unique session updates in milliseconds.") },
            { "PerformanceLog.SlowMapUpdate", (typeof(int), 100, (0, int.MaxValue), "Threshold for slow map updates in milliseconds.") },
            { "PerformanceLog.SlowAsynQueries", (typeof(int), 100, (0, int.MaxValue), "Threshold for slow async queries in milliseconds.") },
            { "PerformanceLog.SlowPackets", (typeof(int), 20, (0, int.MaxValue), "Threshold for slow packet handling in milliseconds.") },
            { "PerformanceLog.SlowMapPackets", (typeof(int), 60, (0, int.MaxValue), "Threshold for slow map packet handling in milliseconds.") },
            { "PerformanceLog.SlowPacketBroadcast", (typeof(int), 0, (0, int.MaxValue), "Threshold for slow packet broadcasts in milliseconds.") },

            // Server Settings
            { "GameType", (typeof(int), 1, (0, 16), "Server realm style: 0 = NORMAL, 1 = PVP, 4 = NORMAL, 6 = RP, 8 = RPPVP, 16 = FFA_PVP.") },
            { "RealmZone", (typeof(int), 1, (1, 29), "Server realm zone for character name restrictions.") },
            { "TimeZoneOffset", (typeof(int), 0, (-12, 12), "Time zone offset in hours. 0 is UTC.") },
            { "DBC.Locale", (typeof(int), 255, (0, 255), "DBC language settings: 0 = English, 1 = Korean, ..., 255 = Auto Detect.") },
            { "StrictPlayerNames", (typeof(int), 0, (0, 3), "Player name restriction level based on realm zone.") },
            { "StrictCharterNames", (typeof(int), 0, (0, 3), "Guild team charter name restriction level.") },
            { "StrictPetNames", (typeof(int), 0, (0, 3), "Pet name restriction level based on realm zone.") },
            { "MinPlayerName", (typeof(int), 2, (1, 12), "Minimum player name length.") },
            { "MinCharterName", (typeof(int), 2, (1, 24), "Minimum guild charter name length.") },
            { "MinPetName", (typeof(int), 2, (1, 12), "Minimum pet name length.") },
            { "CharactersCreatingDisabled", (typeof(int), 0, (0, 3), "Disable character creation for certain factions.") },
            { "CharactersPerAccount", (typeof(int), 50, (1, int.MaxValue), "Maximum number of characters per account.") },
            { "CharactersPerRealm", (typeof(int), 10, (1, 10), "Maximum number of characters per realm.") },
            { "SkipCinematics", (typeof(int), 0, (0, 2), "Control whether or not in-game cinematics are skipped.") },
            { "MaxPlayerLevel", (typeof(int), 60, (1, 100), "Maximum player level achievable.") },
            { "StartPlayerLevel", (typeof(int), 1, (1, 100), "Starting player level upon character creation.") },
            { "StartPlayerMoney", (typeof(int), 0, (0, int.MaxValue), "Starting money amount for new players in copper.") },
            { "MaxHonorPoints", (typeof(int), 75000, (0, int.MaxValue), "Maximum honor points a player can accumulate.") },
            { "StartHonorPoints", (typeof(int), 0, (0, int.MaxValue), "Starting honor points for new players.") },
            { "MinHonorKills", (typeof(int), 15, (0, int.MaxValue), "Minimum honor kills required to enter weekly honor calculations.") },
            { "MaintenanceDay", (typeof(int), 3, (0, 6), "Day of the week for server maintenance and honor distribution.") },
            { "InstantLogout", (typeof(int), 1, (0, 1), "Enable or disable instant logout for higher security levels.") },
            { "AllFlightPaths", (typeof(int), 0, (0, 1), "Players start with all flight paths activated.") },
            { "AlwaysMaxSkillForLevel", (typeof(int), 0, (0, 1), "Automatically max out player skills for their level.") },
            { "ActivateWeather", (typeof(int), 1, (0, 1), "Enable or disable weather system.") },
            { "CastUnstuck", (typeof(int), 1, (0, 1), "Allow use of Unstuck spell.") },
            { "MaxSpellCastsInChain", (typeof(int), 10, (0, int.MaxValue), "Maximum number of spell casts in a chain to prevent overflow.") },
            { "Instance.IgnoreLevel", (typeof(int), 0, (0, 1), "Ignore level requirements to enter instances.") },
            { "Instance.IgnoreRaid", (typeof(int), 0, (0, 1), "Ignore raid requirements to enter instances.") },
            { "Instance.ResetTimeHour", (typeof(int), 4, (0, 23), "Hour of the day for global instance resets.") },
            { "Instance.UnloadDelay", (typeof(int), 1800000, (0, int.MaxValue), "Delay to unload instance maps from memory in milliseconds.") },
            { "Quests.LowLevelHideDiff", (typeof(int), 4, (-1, int.MaxValue), "Quest level difference to hide low-level quests.") },
            { "Quests.HighLevelHideDiff", (typeof(int), 7, (-1, int.MaxValue), "Quest level difference to hide high-level quests.") },
            { "Quests.IgnoreRaid", (typeof(int), 0, (0, 1), "Allow non-raid quests in raid groups.") },
            { "Group.OfflineLeaderDelay", (typeof(int), 300, (0, int.MaxValue), "Grace period for offline group leaders to reconnect before leadership is transferred.") },
            { "Guild.EventLogRecordsCount", (typeof(int), 100, (100, int.MaxValue), "Number of guild event log records stored.") },
            { "TimerBar.Fatigue.GMLevel", (typeof(int), 4, (0, 4), "Security level required to disable fatigue.") },
            { "TimerBar.Fatigue.Max", (typeof(int), 60, (0, int.MaxValue), "Maximum fatigue timer value in seconds.") },
            { "TimerBar.Breath.GMLevel", (typeof(int), 4, (0, 4), "Security level required to disable underwater breathing.") },
            { "TimerBar.Breath.Max", (typeof(int), 60, (0, int.MaxValue), "Maximum underwater breathing time in seconds.") },
            { "TimerBar.Fire.GMLevel", (typeof(int), 4, (0, 4), "Security level required to disable lava fire damage.") },
            { "TimerBar.Fire.Max", (typeof(int), 1, (0, int.MaxValue), "Lava fire damage delay timer in seconds.") },
            { "MaxPrimaryTradeSkill", (typeof(int), 2, (1, 10), "Maximum number of primary trade skills a player can learn.") },
            { "MinPetitionSigns", (typeof(int), 9, (0, 9), "Minimum number of signatures required to create a guild.") },
            { "MaxGroupXPDistance", (typeof(int), 74, (0, int.MaxValue), "Maximum distance for group members to share XP.") },
            { "MailDeliveryDelay", (typeof(int), 3600, (0, int.MaxValue), "Mail delivery delay time in seconds.") },
            { "MassMailer.SendPerTick", (typeof(int), 10, (0, int.MaxValue), "Maximum number of mails sent per tick.") },
            { "PetUnsummonAtMount", (typeof(int), 0, (0, 1), "Unsummon pet when player mounts.") },
            { "Event.Announce", (typeof(int), 0, (0, 1), "Announce server events.") },
            { "BeepAtStart", (typeof(int), 1, (0, 1), "Beep when mangosd starts (mainly on Unix/Linux).") },
            { "ShowProgressBars", (typeof(int), 1, (0, 1), "Show progress bars during server startup.") },
            { "WaitAtStartupError", (typeof(int), 0, (-1, int.MaxValue), "Wait for input after a startup error.") },
            { "Motd", (typeof(string), "Welcome to Light's Hope!", null, "Message of the day.") },

            // Player Interaction
            { "AllowTwoSide.Accounts", (typeof(int), 0, (0, 1), "Allow accounts to create characters in both factions.") },
            { "AllowTwoSide.Interaction.Chat", (typeof(int), 0, (0, 1), "Allow cross-faction chat.") },
            { "AllowTwoSide.Interaction.Channel", (typeof(int), 0, (0, 1), "Allow cross-faction channel interaction.") },
            { "AllowTwoSide.Interaction.Group", (typeof(int), 0, (0, 1), "Allow cross-faction group creation.") },
            { "AllowTwoSide.Interaction.Guild", (typeof(int), 0, (0, 1), "Allow cross-faction guild formation.") },
            { "AllowTwoSide.Interaction.Trade", (typeof(int), 0, (0, 1), "Allow cross-faction trade.") },
            { "AllowTwoSide.Interaction.Auction", (typeof(int), 0, (0, 1), "Allow cross-faction auction access.") },
            { "AllowTwoSide.Interaction.Mail", (typeof(int), 0, (0, 1), "Allow cross-faction mailing.") },
            { "AllowTwoSide.WhoList", (typeof(int), 0, (0, 1), "Show players from both factions in who list.") },
            { "AllowTwoSide.AddFriend", (typeof(int), 0, (0, 1), "Allow adding friends from the opposite faction.") },
            { "TalentsInspecting", (typeof(int), 1, (0, 1), "Allow players to inspect others' talents.") },

            // Creature and GameObject Settings
            { "ThreatRadius", (typeof(int), 100, (0, int.MaxValue), "Radius for creatures to evade after being pulled away.") },
            { "Rate.Creature.Aggro", (typeof(double), 1.0, (0.0, 2.0), "Creature aggro radius multiplier.") },
            { "MaxCreaturesAttackRadius", (typeof(int), 40, (0, int.MaxValue), "Max radius for creatures to search for targets.") },
            { "MaxPlayersStealthDetectRange", (typeof(int), 40, (0, int.MaxValue), "Max range for players to detect stealthed units.") },
            { "MaxCreaturesStealthDetectRange", (typeof(int), 15, (0, int.MaxValue), "Max range for creatures to detect stealthed units.") },
            { "MaxCreatureSummonLimit", (typeof(int), 100, (0, int.MaxValue), "Max number of creatures summoned by objects.") },
            { "CreatureFamilyFleeAssistanceRadius", (typeof(int), 30, (0, int.MaxValue), "Radius for creatures to seek assistance when fleeing.") },
            { "CreatureFamilyAssistanceRadius", (typeof(int), 10, (0, int.MaxValue), "Radius for creatures to call for assistance without moving.") },
            { "CreatureFamilyAssistanceDelay", (typeof(int), 1500, (0, int.MaxValue), "Delay for creature assistance calls in milliseconds.") },
            { "CreatureFamilyFleeDelay", (typeof(int), 7000, (0, int.MaxValue), "Time in milliseconds for creatures to flee.") },
            { "WorldBossLevelDiff", (typeof(int), 3, (0, int.MaxValue), "Level difference for boss dynamic level scaling.") },
            { "Corpse.EmptyLootShow", (typeof(int), 1, (0, 1), "Show loot window even when no loot is generated.") },
            { "Corpse.Decay.NORMAL", (typeof(int), 300, (0, int.MaxValue), "Time in seconds for normal corpse decay.") },
            { "Corpse.Decay.RARE", (typeof(int), 900, (0, int.MaxValue), "Time in seconds for rare corpse decay.") },
            { "Corpse.Decay.ELITE", (typeof(int), 600, (0, int.MaxValue), "Time in seconds for elite corpse decay.") },
            { "Corpse.Decay.RAREELITE", (typeof(int), 1200, (0, int.MaxValue), "Time in seconds for rare elite corpse decay.") },
            { "Corpse.Decay.WORLDBOSS", (typeof(int), 3600, (0, int.MaxValue), "Time in seconds for world boss corpse decay.") },
            { "Rate.Corpse.Decay.Looted", (typeof(double), 0.0, (0.0, 1.0), "Multiplier for looted corpse decay.") },
            { "SendLootRollUponReconnect", (typeof(int), 0, (0, 1), "Send loot roll windows upon player reconnect.") },
            { "Rate.Creature.Normal.Damage", (typeof(double), 1.0, (0.0, 5.0), "Damage multiplier for normal creatures.") },
            { "Rate.Creature.Elite.Elite.Damage", (typeof(double), 1.0, (0.0, 5.0), "Damage multiplier for elite creatures.") },
            { "Rate.Creature.Elite.RAREELITE.Damage", (typeof(double), 1.0, (0.0, 5.0), "Damage multiplier for rare elite creatures.") },
            { "Rate.Creature.Elite.WORLDBOSS.Damage", (typeof(double), 1.0, (0.0, 5.0), "Damage multiplier for world boss creatures.") },
            { "Rate.Creature.Normal.SpellDamage", (typeof(double), 1.0, (0.0, 5.0), "Spell damage multiplier for normal creatures.") },
            { "Rate.Creature.Elite.Elite.SpellDamage", (typeof(double), 1.0, (0.0, 5.0), "Spell damage multiplier for elite creatures.") },
            { "Rate.Creature.Elite.RAREELITE.SpellDamage", (typeof(double), 1.0, (0.0, 5.0), "Spell damage multiplier for rare elite creatures.") },
            { "Rate.Creature.Elite.WORLDBOSS.SpellDamage", (typeof(double), 1.0, (0.0, 5.0), "Spell damage multiplier for world boss creatures.") },
            { "Rate.Creature.Elite.RARE.SpellDamage", (typeof(double), 1.0, (0.0, 5.0), "Spell damage multiplier for rare creatures.") },
            { "Rate.Creature.Normal.HP", (typeof(double), 1.0, (0.0, 5.0), "Health multiplier for normal creatures.") },
            { "Rate.Creature.Elite.Elite.HP", (typeof(double), 1.0, (0.0, 5.0), "Health multiplier for elite creatures.") },
            { "Rate.Creature.Elite.RAREELITE.HP", (typeof(double), 1.0, (0.0, 5.0), "Health multiplier for rare elite creatures.") },
            { "Rate.Creature.Elite.WORLDBOSS.HP", (typeof(double), 1.0, (0.0, 5.0), "Health multiplier for world boss creatures.") },
            { "Rate.Creature.Elite.RARE.HP", (typeof(double), 1.0, (0.0, 5.0), "Health multiplier for rare creatures.") },
            { "ListenRange.Say", (typeof(int), 25, (0, int.MaxValue), "Listening range for creatures' say actions.") },
            { "ListenRange.TextEmote", (typeof(int), 25, (0, int.MaxValue), "Listening range for creatures' text emotes.") },
            { "ListenRange.Yell", (typeof(int), 300, (0, int.MaxValue), "Listening range for creatures' yell actions.") },
            { "GuidReserveSize.Creature", (typeof(int), 1000, (0, int.MaxValue), "Reserved GUID size for creatures.") },
            { "GuidReserveSize.GameObject", (typeof(int), 1000, (0, int.MaxValue), "Reserved GUID size for game objects.") },

            // Chat Settings
            { "ChatFakeMessagePreventing", (typeof(int), 0, (0, 1), "Prevents chat fake messages.") },
            { "ChatStrictLinkChecking.Severity", (typeof(int), 0, (0, 3), "Chat link validation severity.") },
            { "ChatStrictLinkChecking.Kick", (typeof(int), 0, (0, 1), "Kick players for invalid links in chat.") },
            { "ChatFlood.MessageCount", (typeof(int), 10, (0, int.MaxValue), "Maximum message count before triggering anti-flood.") },
            { "ChatFlood.MessageDelay", (typeof(int), 1, (0, int.MaxValue), "Minimum delay between messages in seconds.") },
            { "ChatFlood.MuteTime", (typeof(int), 10, (0, int.MaxValue), "Mute time after triggering anti-flood.") },
            { "Channel.SilentlyGMJoin", (typeof(int), 0, (0, 1), "Allow GMs to join channels silently.") },
            { "Channel.StrictLatinInGeneral", (typeof(int), 0, (0, 1), "Restrict general chat to Latin characters.") },

            // GM Settings
            { "GM.LoginState", (typeof(int), 2, (0, 2), "GM login mode.") },
            { "GM.Visible", (typeof(int), 2, (0, 2), "GM visibility on login.") },
            { "GM.AcceptTickets", (typeof(int), 2, (0, 2), "Whether GMs accept tickets by default.") },
            { "GM.Chat", (typeof(int), 2, (0, 2), "GM chat mode at login.") },
            { "GM.WhisperingTo", (typeof(int), 2, (0, 2), "Whether GMs accept whispers by default.") },
            { "GM.InGMList.Level", (typeof(int), 3, (0, 3), "Maximum GM level shown in the GM list.") },
            { "GM.InWhoList.Level", (typeof(int), 3, (0, 3), "Maximum GM level shown in the who list.") },
            { "GM.LogTrade", (typeof(int), 1, (0, 1), "Include GM trade in logs.") },
            { "GM.StartLevel", (typeof(int), 1, (1, 255), "GM starting level.") },
            { "GM.LowerSecurity", (typeof(int), 0, (0, 1), "Disallow lower security to interact with higher security.") },

            // Visibility and Radiuses
            { "Visibility.GroupMode", (typeof(int), 0, (0, 2), "Group visibility mode.") },
            { "Visibility.Distance.Continents", (typeof(int), 90, (0, 333), "Visibility distance on continents.") },
            { "Visibility.Distance.Continents.Min", (typeof(int), 60, (0, 333), "Minimum visibility distance on continents.") },
            { "Visibility.Distance.Instances", (typeof(int), 120, (0, 333), "Visibility distance in instances.") },
            { "Visibility.Distance.BG", (typeof(int), 180, (0, 333), "Visibility distance in battlegrounds.") },
            { "Visibility.Distance.InFlight", (typeof(int), 100, (0, 333), "Visibility distance for players in flight.") },
            { "Visibility.Distance.Grey.Unit", (typeof(int), 1, (0, int.MaxValue), "Grey visibility distance for units.") },
            { "Visibility.Distance.Grey.Object", (typeof(int), 10, (0, int.MaxValue), "Grey visibility distance for objects.") },
            { "Visibility.RelocationLowerLimit", (typeof(int), 10, (0, int.MaxValue), "Distance for visibility relocation update.") },
            { "Visibility.AIRelocationNotifyDelay", (typeof(int), 1000, (0, int.MaxValue), "Delay for AI reactions on movements.") },

            // Server Rates
            { "Rate.Health", (typeof(double), 1.0, (0.0, 5.0), "Health regeneration rate.") },
            { "Rate.Mana", (typeof(double), 1.0, (0.0, 5.0), "Mana regeneration rate.") },
            { "Rate.Rage.Income", (typeof(double), 1.0, (0.0, 5.0), "Rage income rate from damage.") },
            { "Rate.Rage.Loss", (typeof(double), 1.0, (0.0, 5.0), "Rage loss rate.") },
            { "Rate.Focus", (typeof(double), 1.0, (0.0, 5.0), "Focus regeneration rate.") },
            { "Rate.Loyalty", (typeof(double), 1.0, (0.0, 5.0), "Loyalty gain rate.") },
            { "Rate.Energy", (typeof(double), 1.0, (0.0, 5.0), "Energy regeneration rate for Rogues.") },
            { "Rate.Skill.Discovery", (typeof(double), 1.0, (0.0, 5.0), "Skill discovery rate.") },
    
            // Drop Rates
            { "Rate.Drop.Item.Poor", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for poor quality items.") },
            { "Rate.Drop.Item.Normal", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for normal quality items.") },
            { "Rate.Drop.Item.Uncommon", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for uncommon quality items.") },
            { "Rate.Drop.Item.Rare", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for rare quality items.") },
            { "Rate.Drop.Item.Epic", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for epic quality items.") },
            { "Rate.Drop.Item.Legendary", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for legendary quality items.") },
            { "Rate.Drop.Item.Artifact", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for artifact quality items.") },
            { "Rate.Drop.Item.Referenced", (typeof(double), 1.0, (0.0, 5.0), "Drop rate for referenced items.") },
            { "Rate.Drop.Money", (typeof(double), 1.0, (0.0, 5.0), "Money drop rate.") },

            // XP Rates
            { "Rate.XP.Kill", (typeof(double), 1.0, (0.0, 5.0), "Experience points gained from killing creatures.") },
            { "Rate.XP.Quest", (typeof(double), 1.0, (0.0, 5.0), "Experience points gained from quests.") },
            { "Rate.XP.Explore", (typeof(double), 1.0, (0.0, 5.0), "Experience points gained from exploration.") },

            // Rest Rates
            { "Rate.Rest.InGame", (typeof(double), 1.0, (0.0, 5.0), "In-game resting rate.") },
            { "Rate.Rest.Offline.InTavernOrCity", (typeof(double), 1.0, (0.0, 5.0), "Resting rate in tavern or city while offline.") },
            { "Rate.Rest.Offline.InWilderness", (typeof(double), 1.0, (0.0, 5.0), "Resting rate in wilderness while offline.") },

            // Damage Rates
            { "Rate.Damage.Fall", (typeof(double), 1.0, (0.0, 5.0), "Damage taken from falling.") },

            // Auction Rates
            { "Rate.Auction.Time", (typeof(double), 1.0, (0.0, 5.0), "Auction duration multiplier.") },
            { "Rate.Auction.Deposit", (typeof(double), 1.0, (0.0, 5.0), "Auction deposit multiplier.") },
            { "Rate.Auction.Cut", (typeof(double), 1.0, (0.0, 5.0), "Auction house cut rate.") },

            // Honor and Reputation
            { "Rate.Honor", (typeof(double), 1.0, (0.0, 5.0), "Honor gain rate.") },
            { "Rate.Reputation.Gain", (typeof(double), 1.0, (0.0, 5.0), "Reputation gain rate.") },
            { "Rate.Reputation.LowLevel.Kill", (typeof(double), 0.2, (0.0, 5.0), "Reputation gain from killing low-level creatures.") },
            { "Rate.Reputation.LowLevel.Quest", (typeof(double), 1.0, (0.0, 5.0), "Reputation gain from low-level quests.") },

            // Mining Rates
            { "Rate.Mining.Amount", (typeof(double), 1.0, (0.0, 5.0), "Amount of materials gained from mining.") },
            { "Rate.Mining.Next", (typeof(double), 1.0, (0.0, 5.0), "Chance to get another use of a deposit.") },

            // Talent and Respec Rates
            { "Rate.Talent", (typeof(double), 1.0, (0.0, 5.0), "Talent point gain rate.") },
            { "Rate.RespecBaseCost", (typeof(double), 1.0, (0.0, 5.0), "Base cost for unlearning talents, in gold.") },
            { "Rate.RespecMultiplicativeCost", (typeof(double), 5.0, (0.0, 5.0), "Multiplier applied to the cost for unlearning talents.") },
            { "Rate.RespecMaxMultiplier", (typeof(double), 10.0, (0.0, 5.0), "Maximum cost multiplier for unlearning talents.") },
            { "Rate.RespecMinMultiplier", (typeof(double), 2.0, (0.0, 5.0), "Minimum cost multiplier for unlearning talents.") },

            // Instance Reset Rate
            { "Rate.InstanceResetTime", (typeof(double), 1.0, (0.0, 5.0), "Rate at which instance reset times occur.") },

            // Skill Gain Rates
            { "SkillGain.Crafting", (typeof(double), 1.0, (0.0, 5.0), "Rate of crafting skill gain.") },
            { "SkillGain.Defense", (typeof(double), 1.0, (0.0, 5.0), "Rate of defense skill gain.") },
            { "SkillGain.Gathering", (typeof(double), 1.0, (0.0, 5.0), "Rate of gathering skill gain.") },
            { "SkillGain.Weapon", (typeof(double), 1.0, (0.0, 5.0), "Rate of weapon skill gain.") },

            // Skill Chance Rates
            { "SkillChance.Orange", (typeof(double), 100.0, (0.0, 100.0), "Chance of success for orange skills.") },
            { "SkillChance.Yellow", (typeof(double), 75.0, (0.0, 100.0), "Chance of success for yellow skills.") },
            { "SkillChance.Green", (typeof(double), 25.0, (0.0, 100.0), "Chance of success for green skills.") },
            { "SkillChance.Grey", (typeof(double), 0.0, (0.0, 100.0), "Chance of success for grey skills.") },

            // Skill Fail and Fishing Rates
            { "SkillFail.Loot.Fishing", (typeof(int), 0, (0, 1), "Enable or disable junk loot for fishing fails.") },
            { "SkillFail.Gain.Fishing", (typeof(int), 0, (0, 1), "Enable or disable skill gain for failed fishing attempts.") },
            { "SkillFail.Possible.FishingPool", (typeof(int), 1, (0, 1), "Enable or disable fails from low skill in fishing pools.") },

            // Durability Loss Chance
            { "DurabilityLossChance.Damage", (typeof(double), 0.5, (0.0, 1.0), "Chance to lose durability on damage.") },
            { "DurabilityLossChance.Absorb", (typeof(double), 0.5, (0.0, 1.0), "Chance to lose durability on absorbed damage.") },
            { "DurabilityLossChance.Parry", (typeof(double), 0.05, (0.0, 1.0), "Chance to lose weapon durability on a parry.") },
            { "DurabilityLossChance.Block", (typeof(double), 0.05, (0.0, 1.0), "Chance to lose shield durability on a block.") },

            // Death and Sickness
            { "Death.SicknessLevel", (typeof(int), 11, (-10, int.MaxValue), "Character sickness level after resurrection.") },
            { "Death.CorpseReclaimDelay.PvP", (typeof(int), 1, (0, 1), "Enable or disable increased corpse reclaim delay for PvP deaths.") },
            { "Death.CorpseReclaimDelay.PvE", (typeof(int), 1, (0, 1), "Enable or disable increased corpse reclaim delay for PvE deaths.") },

            // Ghost and Death Speed
            { "Death.Ghost.RunSpeed.World", (typeof(double), 1.0, (0.0, 5.0), "Ghost run speed in non-battleground zones.") },
            { "Death.Ghost.RunSpeed.Battleground", (typeof(double), 1.0, (0.0, 5.0), "Ghost run speed in battleground zones.") },

            // War Effort Resource Completion
            { "Rate.WarEffortResourceComplete", (typeof(double), 0.0, (0.0, 1.0), "Completion rate of war effort resources.") },

            // Battleground Config
            { "Battleground.CastDeserter", (typeof(int), 1, (0, 1), "Enable or disable casting Deserter spell when a player leaves a battleground in progress.") },
            { "Battleground.QueueAnnouncer.Join", (typeof(int), 0, (0, 2), "Queue announcer for join: 0 = no announcement, 1 = send to joined player only, 2 = send to all players.") },
            { "Battleground.QueueAnnouncer.Start", (typeof(int), 0, (0, 1), "Queue announcer for BG start: 0 = disable, 1 = enable.") },
            { "Battleground.InvitationType", (typeof(int), 0, (0, 1), "Set battleground invitation type: 0 = normal, 1 = experimental balancing.") },
            { "Battleground.PrematureFinishTimer", (typeof(int), 300000, (0, int.MaxValue), "Time (in milliseconds) to end the battleground if one team has too few players. 0 = disable.") },
            { "Battleground.PremadeGroupWaitForMatch", (typeof(int), 0, (0, int.MaxValue), "Time (in milliseconds) for premade groups to wait for an opponent premade group. 0 = disable.") },
            { "Battleground.RandomizeQueues", (typeof(int), 0, (0, 1), "Randomize queue positions: 0 = first in queue gets invited first.") },
            { "Battleground.GroupQueueLimit", (typeof(int), 40, (1, int.MaxValue), "Max number of players that can queue as a group.") },
            { "Battleground.QueuesCount", (typeof(int), 0, (0, int.MaxValue), "Maximum number of battleground queues a player can join simultaneously.") },

            // Network Config
            { "Network.Threads", (typeof(int), 1, (1, int.MaxValue), "Number of threads for network handling, recommended 1 thread per 1000 connections.") },
            { "Network.OutKBuff", (typeof(int), -1, (-1, int.MaxValue), "Size of the output kernel buffer.") },
            { "Network.OutUBuff", (typeof(int), 65536, (1, int.MaxValue), "Size of the userspace buffer for output per connection.") },
            { "Network.TcpNoDelay", (typeof(int), 1, (0, 1), "TCP Nagle algorithm setting: 0 = enable (more latency), 1 = disable (less latency).") },
            { "Network.KickOnBadPacket", (typeof(int), 0, (0, 1), "Kick player on receiving a bad packet.") },
            { "Network.PacketBroadcast.Threads", (typeof(int), 0, (0, int.MaxValue), "Number of threads for packet broadcasting.") },
            { "Network.PacketBroadcast.Frequency", (typeof(int), 50, (0, int.MaxValue), "How often packet broadcast threads run in milliseconds.") },
            { "Network.Interval", (typeof(int), 10, (1, int.MaxValue), "Interval in milliseconds between transmissions of the client’s outbound packet buffer.") },

            // Console, Remote Access, and SOAP
            { "Console.Enable", (typeof(int), 1, (0, 1), "Enable or disable console.") },
            { "Ra.Enable", (typeof(int), 0, (0, 1), "Enable or disable remote console.") },
            { "Ra.IP", (typeof(string), "0.0.0.0", null, "IP address for remote console binding.") },
            { "Ra.Port", (typeof(int), 3443, (1, int.MaxValue), "Port for remote console.") },
            { "Ra.MinLevel", (typeof(int), 3, (1, 5), "Minimum access level for remote console.") },
            { "Ra.Secure", (typeof(int), 1, (0, 1), "Enable secure remote access.") },
            { "Ra.Stricted", (typeof(int), 1, (0, 1), "Disallow certain console-only commands from remote access.") },
            { "SOAP.Enable", (typeof(int), 1, (0, 1), "Enable or disable SOAP service.") },
            { "SOAP.IP", (typeof(string), "0.0.0.0", null, "IP address for SOAP service binding.") },
            { "SOAP.Port", (typeof(int), 7878, (1, int.MaxValue), "Port for SOAP service.") },

            // Character Deletion
            { "CharDelete.Method", (typeof(int), 0, (0, 1), "Character deletion behavior: 0 = full delete, 1 = unlink and mark as deleted.") },
            { "CharDelete.MinLevel", (typeof(int), 0, (0, int.MaxValue), "Minimum character level for applying CharDelete.Method.") },
            { "CharDelete.KeepDays", (typeof(int), 30, (0, int.MaxValue), "Number of days before deleted characters are removed from the database.") },

            // Clustering (WIP)
            { "IsMapServer", (typeof(int), 0, (0, 1), "Whether the server is a map server. 0 = No, 1 = Yes.") },
            { "NodesListenAddress", (typeof(string), "127.0.0.1", null, "IP address the node will listen to.") },
            { "NodesListenPort", (typeof(int), 0, (0, 65535), "Port the node will listen on.") },
            { "MasterListenAddress", (typeof(string), "127.0.0.1", null, "IP address the master node will listen to.") },
            { "MasterListenPort", (typeof(int), 0, (0, 65535), "Port the master node will listen on.") },
            { "ServerName", (typeof(string), "Master", null, "Server name.") },

            // Database-based Chat
            { "OfflineChat.Enable", (typeof(int), 0, (0, 1), "Enable or disable offline chat functionality.") },
            { "OfflineChat.Port", (typeof(int), 3444, (0, 65535), "Port for offline chat service.") },
            { "OfflineChat.IP", (typeof(string), "0.0.0.0", null, "IP address for offline chat service.") },
            { "OfflineChat.Password", (typeof(string), "p8zqkt", null, "Password for offline chat service.") },

            // Player Bots
            { "PlayerBot.Enable", (typeof(int), 1, (0, 1), "Enable or disable player bots.") },
            { "PlayerBot.Debug", (typeof(int), 0, (0, 1), "Enable debug logs for player bots.") },
            { "PlayerBot.UpdateMs", (typeof(int), 1000, (0, int.MaxValue), "Update interval for player bots in milliseconds.") },
            { "PlayerBot.MinBots", (typeof(int), 0, (0, int.MaxValue), "Minimum number of player bots.") },
            { "PlayerBot.MaxBots", (typeof(int), 0, (0, int.MaxValue), "Maximum number of player bots.") },
            { "PlayerBot.Refresh", (typeof(int), 10000, (0, int.MaxValue), "Player bot refresh interval in milliseconds.") },
            { "PlayerBot.ForceLogoutDelay", (typeof(int), 1, (0, int.MaxValue), "Delay before forcing bot logout in seconds.") },

            // Other Settings
            { "DuelDist", (typeof(int), 80, (1, int.MaxValue), "Distance for duels.") },
            { "NostalriusLogFile", (typeof(string), "Info.log", null, "Log file for Nostalrius logs.") },
            { "NostalriusLogTimestamp", (typeof(int), 0, (0, 1), "Enable timestamp in Nostalrius log.") },
            { "ChatLogFile", (typeof(string), "Chat.log", null, "Log file for chat logs.") },
            { "ChatLogEnable", (typeof(int), 1, (0, 1), "Enable or disable chat log.") },
            { "ChatLogTimestamp", (typeof(int), 0, (0, 1), "Enable or disable timestamp in chat logs.") },

            // Auction House Bot
            { "AHBot.Enable", (typeof(int), 0, (0, 1), "Enable or disable Auction House Bot.") },
            { "AHBot.ah.id", (typeof(int), 7, (0, int.MaxValue), "ID for Auction House Bot.") },
            { "AHBot.ah.guid", (typeof(int), 23442, (0, int.MaxValue), "GUID for Auction House Bot.") },
            { "AHBot.ah.fid", (typeof(int), 120, (0, int.MaxValue), "Faction ID for Auction House Bot.") },
            { "AHBot.itemcount", (typeof(int), 50, (0, int.MaxValue), "Item count for Auction House Bot.") },

            // Mmaps/Pathfinding Configuration
            { "mmap.enabled", (typeof(int), 1, (0, 1), "Enable or disable Mmaps (pathfinding).") },

            // Phase Permissions
            { "Phase.Allow.Mail", (typeof(int), 1, (0, 1), "Allow mailing in phases.") },
            { "Phase.Allow.Item", (typeof(int), 1, (0, 1), "Allow items in phases.") },
            { "Phase.Allow.WhoList", (typeof(int), 1, (0, 1), "Allow WhoList in phases.") },
            { "Phase.Allow.Friend", (typeof(int), 1, (0, 1), "Allow friends in phases.") },

            // Optimization / Load Mitigation Settings
            { "Continents.InactivePlayers.SkipUpdates", (typeof(int), 0, (0, 1), "Skip updates for inactive players on continents.") },
            { "MapUpdate.ReduceGridActivationDist.Tick", (typeof(int), 0, (0, int.MaxValue), "Grid activation distance reduction in ticks.") },
            { "MapUpdate.IncreaseGridActivationDist.Tick", (typeof(int), 0, (0, int.MaxValue), "Grid activation distance increase in ticks.") },
            { "MapUpdate.MinGridActivationDistance", (typeof(int), 0, (0, int.MaxValue), "Minimum grid activation distance.") },
            { "MapUpdate.ReduceVisDist.Tick", (typeof(int), 0, (0, int.MaxValue), "Visibility distance reduction in ticks.") },
            { "MapUpdate.IncreaseVisDist.Tick", (typeof(int), 0, (0, int.MaxValue), "Visibility distance increase in ticks.") },
            { "MapUpdate.MinVisibilityDistance", (typeof(int), 0, (0, int.MaxValue), "Minimum visibility distance.") },
            { "Continents.Instanciate", (typeof(int), 0, (0, 1), "Enable or disable instantiation of continents.") },
            { "Maps.Empty.UpdateTime", (typeof(int), 0, (0, int.MaxValue), "Update time for empty maps in milliseconds.") },
    
            // Per-map Threading
            { "MapUpdate.Instanced.UpdateThreads", (typeof(int), 2, (1, int.MaxValue), "Number of update threads for instanced maps.") },
            { "MapUpdate.ObjectsUpdate.MaxThreads", (typeof(int), 4, (1, int.MaxValue), "Max number of object update threads.") },
            { "MapUpdate.ObjectsUpdate.Timeout", (typeof(int), 100, (1, int.MaxValue), "Timeout for object updates in milliseconds.") },
            { "MapUpdate.VisibilityUpdate.MaxThreads", (typeof(int), 4, (1, int.MaxValue), "Max number of visibility update threads.") },
            { "MapUpdate.VisibilityUpdate.Timeout", (typeof(int), 100, (1, int.MaxValue), "Timeout for visibility updates in milliseconds.") },

            // Multithreading Options
            { "MapUpdate.UpdatePacketsDiff", (typeof(int), 100, (0, int.MaxValue), "Max packet update diff per tick.") },
            { "MapUpdate.UpdatePlayersDiff", (typeof(int), 100, (0, int.MaxValue), "Max player update diff per tick.") },
            { "MapUpdate.UpdateCellsDiff", (typeof(int), 100, (0, int.MaxValue), "Max cell update diff per tick.") },

            // Movement and Motion Updates
            { "MapUpdate.Continents.MTCells.Threads", (typeof(int), 0, (0, int.MaxValue), "Number of threads for updating multiple cells simultaneously.") },
            { "MapUpdate.Continents.MTCells.SafeDistance", (typeof(int), 1066, (0, int.MaxValue), "Minimum safe distance between cells to avoid thread race conditions.") },
            { "Continents.MotionUpdate.Threads", (typeof(int), 0, (0, int.MaxValue), "Number of threads for motion updates on continents.") },

            // Async Tasks
            { "AsyncTasks.Threads", (typeof(int), 1, (1, int.MaxValue), "Number of threads for handling asynchronous tasks (e.g., /who, auction house).") },
            { "AsyncQueriesTickTimeout", (typeof(int), 0, (0, int.MaxValue), "Timeout for async queries per tick.") },

            // Terrain Preload
            { "Terrain.Preload.Continents", (typeof(int), 0, (0, 1), "Preload terrain data for continents.") },
            { "Terrain.Preload.Instances", (typeof(int), 0, (0, 1), "Preload terrain data for instances.") },
    
            // Async Queries
            { "AsyncQueriesTickTimeout", (typeof(int), 0, (0, int.MaxValue), "Timeout for asynchronous queries in milliseconds.") },

            // Battleground Settings
            { "Battleground.InvitationType", (typeof(int), 1, (0, 1), "Battleground invitation type. 0 = normal, 1 = balanced.") },
            { "ShowProgressBars", (typeof(int), 0, (0, 1), "Show progress bars during server startup.") },
            { "BgLogFile", (typeof(string), "bg.log", null, "File for logging battleground registrations.") },
            { "BgLogTimestamp", (typeof(int), 0, (0, 1), "Enable timestamps in battleground log.") },

            // Ban List Reload Timer
            { "BanListReloadTimer", (typeof(int), 120, (0, int.MaxValue), "Time between IP and account banned reload, in seconds.") },

            // Honor and Dishonor
            { "VD.Enable", (typeof(int), 1, (0, 1), "Enable dishonorable kills.") },

            // Pets and Spells
            { "Pet.DefaultLoyalty", (typeof(int), 1, (1, int.MaxValue), "Default loyalty level for pets.") },
            { "Spells.CCDelay", (typeof(int), 200, (0, int.MaxValue), "Crowd control spell delay in milliseconds.") },
            { "DebuffLimit", (typeof(int), 16, (1, int.MaxValue), "Maximum number of debuffs a player can have.") },
    
            // Movement Configuration
            { "Movement.MaxPointsPerPacket", (typeof(int), 80, (1, int.MaxValue), "Maximum movement points allowed per packet.") },
            { "Movement.Interpolation", (typeof(int), 0, (0, 1), "Enable or disable movement interpolation.") },
            { "Movement.RelocationVmapsCheckDelay", (typeof(int), 0, (0, int.MaxValue), "Delay for checking movement relocation and VMaps.") },

            // Anti-crash Configuration
            { "Anticrash.Options", (typeof(int), 28, (0, int.MaxValue), "Anti-crash configuration options.") },
            { "Anticrash.Rearm.Timer", (typeof(int), 60000, (0, int.MaxValue), "Timer between crash protection resets, in milliseconds.") },

            // Outdoor PvP System
            { "OutdoorPvP.EP.Enable", (typeof(int), 1, (0, 1), "Enable Outdoor PvP system in Eastern Plaguelands.") },
            { "OutdoorPvP.SI.Enable", (typeof(int), 1, (0, 1), "Enable Outdoor PvP system in Silithus.") },

            // Dynamic Respawn Rates
            { "DynamicRespawn.Range", (typeof(int), -1, (-1, int.MaxValue), "Dynamic respawn range.") },
            { "DynamicRespawn.PercentPerPlayer", (typeof(int), 0, (0, int.MaxValue), "Percent of respawn time reduced per player.") },
            { "DynamicRespawn.MaxReductionRate", (typeof(int), 0, (0, int.MaxValue), "Maximum reduction rate for respawn time.") },
            { "DynamicRespawn.MinRespawnTime", (typeof(int), 0, (0, int.MaxValue), "Minimum respawn time.") },
            { "DynamicRespawn.AffectRespawnTimeBelow", (typeof(int), 0, (0, int.MaxValue), "Affect respawn time for levels below this.") },
            { "DynamicRespawn.AffectLevelBelow", (typeof(int), 0, (0, int.MaxValue), "Affect respawn for players below this level.") },
            { "DynamicRespawn.PlayersThreshold", (typeof(int), 0, (0, int.MaxValue), "Player threshold for dynamic respawn to activate.") },
            { "DynamicRespawn.PlayersMaxLevelDiff", (typeof(int), 0, (0, int.MaxValue), "Maximum level difference between players for dynamic respawn.") },

            // Chat and Whisper Restrictions
            { "WorldChan.MinLevel", (typeof(int), 1, (0, int.MaxValue), "Minimum level to use world chat channels.") },
            { "WhisperDiffZone.MinLevel", (typeof(int), 1, (0, int.MaxValue), "Minimum level to whisper players in a different zone.") },
            { "YellRange.LinearScale.MaxLevel", (typeof(int), 0, (0, int.MaxValue), "Max level for linear scaling of yell range.") },
            { "YellRange.QuadraticScale.MaxLevel", (typeof(int), 0, (0, int.MaxValue), "Max level for quadratic scaling of yell range.") },
            { "ChannelInvite.MinLevel", (typeof(int), 10, (0, int.MaxValue), "Minimum level to invite players to channels.") },
            { "WhisperRestriction", (typeof(int), 0, (0, 1), "Restrict whispering between factions.") },
            { "SayMinLevel", (typeof(int), 0, (0, int.MaxValue), "Minimum level to use the say command.") },
            { "SayEmoteMinLevel", (typeof(int), 0, (0, int.MaxValue), "Minimum level to use say emotes.") },
            { "YellMinLevel", (typeof(int), 0, (0, int.MaxValue), "Minimum level to use the yell command.") },

            // Database Logs
            { "LogsDB.Chat", (typeof(int), 0, (0, 1), "Enable logging for chat.") },
            { "LogsDB.Characters", (typeof(int), 0, (0, 1), "Enable logging for character actions.") },
            { "LogsDB.Trades", (typeof(int), 0, (0, 1), "Enable logging for trades.") },
            { "LogsDB.Transactions", (typeof(int), 0, (0, 1), "Enable logging for transactions.") },
            { "LogsDB.Behavior", (typeof(int), 0, (0, 1), "Enable logging for behavior tracking.") },
            { "LogsDB.Battlegrounds", (typeof(int), 0, (0, 1), "Enable logging for battleground actions.") },
            { "LogsDB.TrashCharacters", (typeof(int), 0, (0, 1), "Enable logging for character deletion.") },
            { "LogsDB.ChatSpam", (typeof(int), 0, (0, 1), "Enable logging for chat spam.") },

            // Character Creation
            { "CharCreation.Enable", (typeof(int), 1, (0, 1), "Enable or disable character creation.") },

            // Item and GM Configurations
            { "Item.InstantSave.Quality", (typeof(int), 6, (0, int.MaxValue), "Instant save quality for items.") },
            { "DieCommand.Credits", (typeof(int), 1, (0, 1), "Give loots, money, and reputation on '.die' command.") },
            { "GM.AllowTrades", (typeof(int), 1, (0, 1), "Allow GMs to trade and use mail/AH.") },
            { "GM.AllowPublicChannels", (typeof(int), 0, (0, 1), "Allow GMs to speak in public channels.") },
            { "CriticalCommandsLogFile", (typeof(string), "gm_critical.log", null, "Log file for critical GM commands.") },

            // Mail Spam Mitigation
            { "MailSpam.ExpireSecs", (typeof(int), 0, (0, int.MaxValue), "Mail spam expiration time in seconds.") },
            { "MailSpam.MaxMails", (typeof(int), 2, (1, int.MaxValue), "Maximum mails allowed before triggering spam filter.") },
            { "MailSpam.Level", (typeof(int), 1, (0, int.MaxValue), "Minimum level to bypass mail spam filter.") },
            { "MailSpam.Money", (typeof(int), 0, (0, int.MaxValue), "Minimum amount of money for triggering mail spam filter.") },
            { "MailSpam.Item", (typeof(int), 0, (0, int.MaxValue), "Minimum item value for triggering mail spam filter.") },

            // Whisper Spam Mitigation
            { "WhispSpam.ExpireSecs", (typeof(int), 0, (0, int.MaxValue), "Whisper spam expiration time in seconds.") },
            { "WhispSpam.MaxScore.Base", (typeof(int), 100, (0, int.MaxValue), "Base score for whisper spam detection.") },
            { "WhispSpam.MaxScore.PerLevel", (typeof(int), 0, (0, int.MaxValue), "Score per player level for whisper spam detection.") },
            { "WhispSpam.Action", (typeof(int), 0, (0, int.MaxValue), "Action to be taken for whisper spam detection (flags).") },

            // Public Channel Mute Bypass
            { "PublicChansMute.BypassLevel", (typeof(int), 61, (0, int.MaxValue), "Minimum level to bypass public channel mute.") },

            // Exploit Logging
            { "ExploitsLogFile", (typeof(string), "", null, "Log file for potential exploit detection.") },

            // Alterac Valley Premade Prevention
            { "Alterac.MinPlayersInQueue", (typeof(int), 0, (0, int.MaxValue), "Minimum players in queue per faction to start Alterac Valley.") },
            { "Alterac.InitMaxPlayers", (typeof(int), 0, (0, int.MaxValue), "Maximum players at Alterac Valley creation. 0 = use database MaxPlayers value.") },

            // GM Tickets
            { "GMTickets.Enable", (typeof(int), 1, (0, 1), "Enable or disable GM ticketing system.") },
            { "GMTickets.MinLevel", (typeof(int), 0, (0, int.MaxValue), "Minimum level to submit GM tickets.") },
            { "GMTickets.Admin.Security", (typeof(int), 7, (0, int.MaxValue), "Minimum security level for GM ticket administrators.") },

            // Mail COD Scam Prevention
            { "Mails.COD.ForceTag.MaxLevel", (typeof(int), 0, (0, int.MaxValue), "Maximum level for tagging COD mails to prevent scamming.") },

            // Antiflood System
            { "Antiflood.Sanction", (typeof(int), 4, (0, int.MaxValue), "Sanction applied to players sending too many packets.") }
        };
    }
}
