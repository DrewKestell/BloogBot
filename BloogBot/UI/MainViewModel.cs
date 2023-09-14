using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;

namespace BloogBot.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string COMMAND_ERROR = "An error occured. See Console for details.";

        static readonly string[] CityNames = { "Orgrimmar", "Thunder Bluff", "Undercity", "Stormwind", "Darnassus", "Ironforge" };

        readonly BotLoader botLoader = new BotLoader();
        readonly Probe probe;
        readonly BotSettings botSettings;
        bool readyForCommands;

        public MainViewModel()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var botSettingsFilePath = Path.Combine(currentFolder, "botSettings.json");
            botSettings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(botSettingsFilePath));
            UpdatePropertiesWithAttribute(typeof(BotSettingAttribute));

            Logger.Initialize(botSettings);
            Repository.Initialize(botSettings.DatabaseType,botSettings.DatabasePath);
            DiscordClientWrapper.Initialize(botSettings);
            TravelPathGenerator.Initialize(() =>
            {
                OnPropertyChanged(nameof(SaveTravelPathCommandEnabled));
            });

            void callback()
            {
                UpdatePropertiesWithAttribute(typeof(ProbeFieldAttribute));
            }
            void killswitch()
            {
                Stop();
            }
            probe = new Probe(callback, killswitch)
            {
                BlacklistedMobIds = Repository.ListBlacklistedMobIds()
            };

            InitializeTravelPaths();
            InitializeHotspots();
            InitializeNpcs();
            ReloadBots();
        }

        public ObservableCollection<string> ConsoleOutput { get; } = new ObservableCollection<string>();
        public ObservableCollection<IBot> Bots { get; private set; }
        public ObservableCollection<TravelPath> TravelPaths { get; private set; }
        public ObservableCollection<Hotspot> Hotspots { get; private set; }
        public ObservableCollection<Npc> RepairNpcs { get; private set; }
        public ObservableCollection<Npc> InkeeperNpcs { get; private set; }
        public ObservableCollection<Npc> AmmoNpcs { get; private set; }

        #region Commands

        // Start command
        ICommand startCommand;

        void UiStart()
        {
            Start();
            Log("Bot started!");
        }

        void Start()
        {
            try
            {
                ObjectManager.KillswitchTriggered = false;

                var container = CurrentBot.GetDependencyContainer(botSettings, probe, Hotspots);

                void stopCallback()
                {
                    OnPropertyChanged(nameof(StartCommandEnabled));
                    OnPropertyChanged(nameof(StopCommandEnabled));
                    OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                    OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                    OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                    OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                    OnPropertyChanged(nameof(CurrentBotEnabled));
                    OnPropertyChanged(nameof(GrindingHotspotEnabled));
                    OnPropertyChanged(nameof(CurrentTravelPathEnabled));
                }

                currentBot.Start(container, stopCallback);

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
                OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                OnPropertyChanged(nameof(CurrentBotEnabled));
                OnPropertyChanged(nameof(GrindingHotspotEnabled));
                OnPropertyChanged(nameof(CurrentTravelPathEnabled));
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand StartCommand =>
            startCommand ?? (startCommand = new CommandHandler(UiStart, true));

        // Stop command
        ICommand stopCommand;

        void UiStop()
        {
            Stop();
            Log("Bot stopped!");
        }

        void Stop()
        {
            try
            {
                var container = CurrentBot.GetDependencyContainer(botSettings, probe, Hotspots);

                currentBot.Stop();

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
                OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                OnPropertyChanged(nameof(CurrentBotEnabled));
                OnPropertyChanged(nameof(GrindingHotspotEnabled));
                OnPropertyChanged(nameof(CurrentTravelPathEnabled));
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public ICommand StopCommand =>
            stopCommand ?? (stopCommand = new CommandHandler(UiStop, true));

        // ReloadBot command
        ICommand reloadBotsCommand;

        void ReloadBots()
        {
            try
            {
                Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());
                CurrentBot = Bots.FirstOrDefault(b => b.Name == botSettings.CurrentBotName) ?? Bots.First();

                OnPropertyChanged(nameof(Bots));
                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
                OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));

                Log("Bot successfully loaded!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand ReloadBotsCommand =>
            reloadBotsCommand ?? (reloadBotsCommand = new CommandHandler(ReloadBots, true));

        ICommand blacklistCurrentTargetCommand;

        void BlacklistCurrentTarget()
        {
            try
            {
                var target = ObjectManager.CurrentTarget;
                if (target != null)
                {
                    if (Repository.BlacklistedMobExists(target.Guid))
                    {
                        Log("Target already blacklisted. Removing from blacklist.");
                        Repository.RemoveBlacklistedMob(target.Guid);
                        probe.BlacklistedMobIds.Remove(target.Guid);
                    }
                    else
                    {
                        Repository.AddBlacklistedMob(target.Guid);
                        probe.BlacklistedMobIds.Add(target.Guid);
                        Log($"Successfully blacklisted mob: {target.Guid}");
                    }
                }
                else
                    Log("Blacklist failed. No target selected.");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand BlacklistCurrentTargetCommand =>
            blacklistCurrentTargetCommand ?? (blacklistCurrentTargetCommand = new CommandHandler(BlacklistCurrentTarget, true));

        // Test command
        ICommand testCommand;

        void Test()
        {
            var container = CurrentBot.GetDependencyContainer(botSettings, probe, Hotspots);

            currentBot.Test(container);
        }

        public ICommand TestCommand =>
            testCommand ?? (testCommand = new CommandHandler(Test, true));

        // StartPowerlevel command
        ICommand startPowerlevelCommand;

        void StartPowerlevel()
        {
            var container = CurrentBot.GetDependencyContainer(botSettings, probe, Hotspots);

            void stopCallback()
            {
                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
                OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                OnPropertyChanged(nameof(CurrentBotEnabled));
                OnPropertyChanged(nameof(GrindingHotspotEnabled));
                OnPropertyChanged(nameof(CurrentTravelPathEnabled));
            }

            currentBot.StartPowerlevel(container, stopCallback);

            OnPropertyChanged(nameof(StartCommandEnabled));
            OnPropertyChanged(nameof(StopCommandEnabled));
            OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
            OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
            OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
            OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
            OnPropertyChanged(nameof(CurrentBotEnabled));
            OnPropertyChanged(nameof(GrindingHotspotEnabled));
            OnPropertyChanged(nameof(CurrentTravelPathEnabled));
        }

        public ICommand StartPowerlevelCommand =>
            startPowerlevelCommand ?? (startPowerlevelCommand = new CommandHandler(StartPowerlevel, true));

        // SaveSettings command
        ICommand saveSettingsCommand;

        void SaveSettings()
        {
            try
            {
                botSettings.CurrentTravelPathId = CurrentTravelPath?.Id;
                botSettings.GrindingHotspotId = GrindingHotspot?.Id;
                botSettings.CurrentBotName = CurrentBot.Name;

                var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                var botSettingsFilePath = Path.Combine(currentFolder, "botSettings.json");
                var json = JsonConvert.SerializeObject(botSettings, Formatting.Indented);
                File.WriteAllText(botSettingsFilePath, json);

                Log("Settings successfully saved!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand SaveSettingsCommand =>
            saveSettingsCommand ?? (saveSettingsCommand = new CommandHandler(SaveSettings, true));
        
        // StartRecordingTravelPath command
        ICommand startRecordingTravelPathCommand;

        void StartRecordingTravelPath()
        {
            try
            {
                var isLoggedIn = ThreadSynchronizer.RunOnMainThread(() => Functions.GetPlayerGuid() > 0);
                if (isLoggedIn)
                {
                    TravelPathGenerator.Record(ObjectManager.Player, Log);

                    OnPropertyChanged(nameof(StartRecordingTravelPathCommandEnabled));
                    OnPropertyChanged(nameof(SaveTravelPathCommandEnabled));
                    OnPropertyChanged(nameof(CancelTravelPathCommandEnabled));

                    Log("Recording new travel path...");

                }
                else
                    Log("Recording failed. Not logged in.");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand StartRecordingTravelPathCommand =>
            startRecordingTravelPathCommand ?? (startRecordingTravelPathCommand = new CommandHandler(StartRecordingTravelPath, true));

        // CancelTravelPath command
        ICommand cancelTravelPathCommand;

        void CancelTravelPath()
        {
            try
            {
                TravelPathGenerator.Cancel();

                OnPropertyChanged(nameof(StartRecordingTravelPathCommandEnabled));
                OnPropertyChanged(nameof(SaveTravelPathCommandEnabled));
                OnPropertyChanged(nameof(CancelTravelPathCommandEnabled));

                Log("Canceling new travel path...");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand CancelTravelPathCommand =>
            cancelTravelPathCommand ?? (cancelTravelPathCommand = new CommandHandler(CancelTravelPath, true));

        // SaveTravelPath command
        ICommand saveTravelPathCommand;

        void SaveTravelPath()
        {
            try
            {
                var waypoints = TravelPathGenerator.Save();
                var travelPath = Repository.AddTravelPath(newTravelPathName, waypoints);

                TravelPaths.Add(travelPath);
                TravelPaths = new ObservableCollection<TravelPath>(TravelPaths.OrderBy(p => p?.Name));

                NewTravelPathName = string.Empty;

                OnPropertyChanged(nameof(StartRecordingTravelPathCommandEnabled));
                OnPropertyChanged(nameof(SaveTravelPathCommandEnabled));
                OnPropertyChanged(nameof(CancelTravelPathCommandEnabled));
                OnPropertyChanged(nameof(TravelPaths));

                Log("New travel path successfully saved!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand SaveTravelPathCommand =>
            saveTravelPathCommand ?? (saveTravelPathCommand = new CommandHandler(SaveTravelPath, true));

        // StartTravelPath
        ICommand startTravelPathCommand;

        void StartTravelPath()
        {
            try
            {
                var container = CurrentBot.GetDependencyContainer(botSettings, probe, Hotspots);

                void callback()
                {
                    OnPropertyChanged(nameof(StartCommandEnabled));
                    OnPropertyChanged(nameof(StopCommandEnabled));
                    OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                    OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                    OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                    OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                    OnPropertyChanged(nameof(CurrentBotEnabled));
                    OnPropertyChanged(nameof(GrindingHotspotEnabled));
                    OnPropertyChanged(nameof(CurrentTravelPathEnabled));
                }

                currentBot.Travel(container, reverseTravelPath, callback);

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
                OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                OnPropertyChanged(nameof(CurrentBotEnabled));
                OnPropertyChanged(nameof(GrindingHotspotEnabled));
                OnPropertyChanged(nameof(CurrentTravelPathEnabled));

                Log("Travel started!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand StartTravelPathCommand =>
            startTravelPathCommand ?? (startTravelPathCommand = new CommandHandler(StartTravelPath, true));

        // StopTravelPath
        ICommand stopTravelPathCommand;

        void StopTravelPath()
        {
            try
            {
                var container = CurrentBot.GetDependencyContainer(botSettings, probe, Hotspots);

                currentBot.Stop();

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
                OnPropertyChanged(nameof(StartPowerlevelCommandEnabled));
                OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
                OnPropertyChanged(nameof(StopTravelPathCommandEnabled));
                OnPropertyChanged(nameof(ReloadBotsCommandEnabled));
                OnPropertyChanged(nameof(CurrentBotEnabled));
                OnPropertyChanged(nameof(GrindingHotspotEnabled));
                OnPropertyChanged(nameof(CurrentTravelPathEnabled));

                Log("TravelPath stopped!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand StopTravelPathCommand =>
            stopTravelPathCommand ?? (stopTravelPathCommand = new CommandHandler(StopTravelPath, true));
        
        // ClearLog
        ICommand clearLogCommand;

        void ClearLog()
        {
            try
            {
                ConsoleOutput.Clear();
                OnPropertyChanged(nameof(ConsoleOutput));
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand ClearLogCommand =>
            clearLogCommand ?? (clearLogCommand = new CommandHandler(ClearLog, true));

        // AddNpc
        ICommand addNpcCommand;

        void AddNpc()
        {
            try
            {
                var target = ObjectManager.CurrentTarget;
                if (target != null)
                {
                    if (Repository.NpcExists(target.Name))
                    {
                        Log("NPC already exists!");
                        return;
                    }

                    var npc = Repository.AddNpc(
                        target.Name,
                        npcIsInnkeeper,
                        npcSellsAmmo,
                        npcRepairs,
                        false, // npcQuest - deprecated
                        npcHorde,
                        npcAlliance,
                        target.Position.X,
                        target.Position.Y,
                        target.Position.Z,
                        ObjectManager.ZoneText);

                    InitializeNpcs();
                    Log("NPC saved successfully!");
                }
                else
                    Log("NPC not saved. No target selected.");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand AddNpcCommand =>
            addNpcCommand ?? (addNpcCommand = new CommandHandler(AddNpc, true));

        // RecordHotspot
        ICommand startRecordingHotspotCommand;

        void StartRecordingHotspot()
        {
            try
            {
                var isLoggedIn = ThreadSynchronizer.RunOnMainThread(() => Functions.GetPlayerGuid() > 0);
                if (isLoggedIn)
                {
                    HotspotGenerator.Record();

                    OnPropertyChanged(nameof(StartRecordingHotspotCommandEnabled));
                    OnPropertyChanged(nameof(AddHotspotWaypointCommandEnabled));
                    OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
                    OnPropertyChanged(nameof(CancelHotspotCommandEnabled));

                    Log("Recording new hotspot...");

                }
                else
                    Log("Recording failed. Not logged in.");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand StartRecordingHotspotCommand =>
            startRecordingHotspotCommand ?? (startRecordingHotspotCommand = new CommandHandler(StartRecordingHotspot, true));

        // AddHotspotWaypoint
        ICommand addHotspotWaypointCommand;

        void AddHotspotWaypoint()
        {
            try
            {
                var isLoggedIn = ThreadSynchronizer.RunOnMainThread(() => Functions.GetPlayerGuid() > 0);
                if (isLoggedIn)
                {
                    HotspotGenerator.AddWaypoint(ObjectManager.Player.Position);

                    OnPropertyChanged(nameof(SaveHotspotCommandEnabled));

                    Log("Waypoint successfully added!");

                }
                else
                    Log("Failed to add waypoint. Not logged in.");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand AddHotspotWaypointCommand =>
            addHotspotWaypointCommand ?? (addHotspotWaypointCommand = new CommandHandler(AddHotspotWaypoint, true));

        // SaveHotspot
        ICommand saveHotspotCommand;

        void SaveHotspot()
        {
            try
            {
                string faction;
                if (newHotspotHorde && newHotspotAlliance)
                    faction = "Alliance / Horde";
                else if (newHotspotHorde)
                    faction = "Horde";
                else
                    faction = "Alliance";

                var waypoints = HotspotGenerator.Save();
                var hotspot = Repository.AddHotspot(
                    ObjectManager.ZoneText,
                    newHotspotDescription,
                    faction,
                    waypoints,
                    newHotspotInnkeeper,
                    newHotspotRepairVendor,
                    newHotspotAmmoVendor,
                    newHotspotMinLevel,
                    newHotspotTravelPath,
                    newHotspotSafeForGrinding);

                Hotspots.Add(hotspot);
                Hotspots = new ObservableCollection<Hotspot>(Hotspots.OrderBy(h => h?.MinLevel).ThenBy(h => h?.Zone).ThenBy(h => h?.Description));

                NewHotspotDescription = string.Empty;
                NewHotspotMinLevel = 0;
                NewHotspotInnkeeper = null;
                NewHotspotRepairVendor = null;
                NewHotspotAmmoVendor = null;
                NewHotspotTravelPath = null;
                NewHotspotHorde = false;
                NewHotspotAlliance = false;
                
                OnPropertyChanged(nameof(StartRecordingHotspotCommandEnabled));
                OnPropertyChanged(nameof(AddHotspotWaypointCommandEnabled));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
                OnPropertyChanged(nameof(CancelHotspotCommandEnabled));
                OnPropertyChanged(nameof(Hotspots));

                Log("New hotspot successfully saved!");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand SaveHotspotCommand =>
            saveHotspotCommand ?? (saveHotspotCommand = new CommandHandler(SaveHotspot, true));

        // CancelHotspot
        ICommand cancelHotspotCommand;

        void CancelHotspot()
        {
            try
            {
                HotspotGenerator.Cancel();

                OnPropertyChanged(nameof(StartRecordingHotspotCommandEnabled));
                OnPropertyChanged(nameof(AddHotspotWaypointCommandEnabled));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
                OnPropertyChanged(nameof(CancelHotspotCommandEnabled));

                Log("Canceling new travel path...");
            }
            catch (Exception e)
            {
                Logger.Log(e);
                Log(COMMAND_ERROR);
            }
        }

        public ICommand CancelHotspotCommand =>
            cancelHotspotCommand ?? (cancelHotspotCommand = new CommandHandler(CancelHotspot, true));

        #endregion

        #region Observables
        // IsEnabled
        public bool AddNpcCommandEnabled =>
            (npcIsInnkeeper || npcSellsAmmo || npcRepairs) &&
            (npcHorde || npcAlliance);

        public bool StartRecordingTravelPathCommandEnabled => !TravelPathGenerator.Recording;

        public bool SaveTravelPathCommandEnabled =>
            TravelPathGenerator.Recording &&
            TravelPathGenerator.PositionCount > 0 &&
            !string.IsNullOrWhiteSpace(newTravelPathName);

        public bool CancelTravelPathCommandEnabled => TravelPathGenerator.Recording;

        public bool StartTravelPathCommandEnabled =>
            !currentBot.Running() &&
            CurrentTravelPath != null;

        public bool StopTravelPathCommandEnabled => currentBot.Running();

        public bool CurrentTravelPathEnabled => !currentBot.Running();

        public bool StartCommandEnabled => !currentBot.Running();

        public bool StopCommandEnabled => currentBot.Running();

        public bool StartPowerlevelCommandEnabled => !currentBot.Running();

        public bool ReloadBotsCommandEnabled => !currentBot.Running();
        
        public bool StartRecordingHotspotCommandEnabled =>
            !HotspotGenerator.Recording;

        public bool AddHotspotWaypointCommandEnabled =>
            HotspotGenerator.Recording;

        public bool SaveHotspotCommandEnabled =>
            HotspotGenerator.Recording &&
            HotspotGenerator.PositionCount > 0 &&
            !string.IsNullOrWhiteSpace(newHotspotDescription) &&
            newHotspotMinLevel > 0 &&
            (newHotspotHorde || newHotspotAlliance);

        public bool CancelHotspotCommandEnabled =>
            HotspotGenerator.Recording;

        public bool CurrentBotEnabled => !currentBot.Running();

        public bool GrindingHotspotEnabled => !currentBot.Running();

        // General
        IBot currentBot;
        public IBot CurrentBot
        {
            get => currentBot;
            set
            {
                currentBot = value;
                OnPropertyChanged(nameof(CurrentBot));
            }
        }

        bool npcIsInnkeeper;
        public bool NpcIsInnkeeper
        {
            get => npcIsInnkeeper;
            set
            {
                npcIsInnkeeper = value;
                OnPropertyChanged(nameof(NpcIsInnkeeper));
                OnPropertyChanged(nameof(AddNpcCommandEnabled));
            }
        }

        bool npcSellsAmmo;
        public bool NpcSellsAmmo
        {
            get => npcSellsAmmo;
            set
            {
                npcSellsAmmo = value;
                OnPropertyChanged(nameof(NpcSellsAmmo));
                OnPropertyChanged(nameof(AddNpcCommandEnabled));
            }
        }

        bool npcRepairs;
        public bool NpcRepairs
        {
            get => npcRepairs;
            set
            {
                npcRepairs = value;
                OnPropertyChanged(nameof(NpcRepairs));
                OnPropertyChanged(nameof(AddNpcCommandEnabled));
            }
        }

        bool npcHorde;
        public bool NpcHorde
        {
            get => npcHorde;
            set
            {
                npcHorde = value;
                OnPropertyChanged(nameof(NpcHorde));
                OnPropertyChanged(nameof(AddNpcCommandEnabled));
            }
        }

        bool npcAlliance;
        public bool NpcAlliance
        {
            get => npcAlliance;
            set
            {
                npcAlliance = value;
                OnPropertyChanged(nameof(NpcAlliance));
                OnPropertyChanged(nameof(AddNpcCommandEnabled));
            }
        }

        string newTravelPathName;
        public string NewTravelPathName
        {
            get => newTravelPathName;
            set
            {
                newTravelPathName = value;
                OnPropertyChanged(nameof(NewTravelPathName));
            }
        }
        
        string newHotspotDescription;
        public string NewHotspotDescription
        {
            get => newHotspotDescription;
            set
            {
                newHotspotDescription = value;
                OnPropertyChanged(nameof(NewHotspotDescription));
            }
        }

        int newHotspotMinLevel;
        public int NewHotspotMinLevel
        {
            get => newHotspotMinLevel;
            set
            {
                newHotspotMinLevel = value;
                OnPropertyChanged(nameof(NewHotspotMinLevel));
            }
        }

        Npc newHotspotInnkeeper;
        public Npc NewHotspotInnkeeper
        {
            get => newHotspotInnkeeper;
            set
            {
                newHotspotInnkeeper = value;
                OnPropertyChanged(nameof(NewHotspotInnkeeper));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
            }
        }

        Npc newHotspotRepairVendor;
        public Npc NewHotspotRepairVendor
        {
            get => newHotspotRepairVendor;
            set
            {
                newHotspotRepairVendor = value;
                OnPropertyChanged(nameof(NewHotspotRepairVendor));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
            }
        }

        Npc newHotspotAmmoVendor;
        public Npc NewHotspotAmmoVendor
        {
            get => newHotspotAmmoVendor;
            set
            {
                newHotspotAmmoVendor = value;
                OnPropertyChanged(nameof(NewHotspotAmmoVendor));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
            }
        }

        TravelPath newHotspotTravelPath;
        public TravelPath NewHotspotTravelPath
        {
            get => newHotspotTravelPath;
            set
            {
                newHotspotTravelPath = value;
                OnPropertyChanged(nameof(NewHotspotTravelPath));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
            }
        }

        bool newHotspotHorde;
        public bool NewHotspotHorde
        {
            get => newHotspotHorde;
            set
            {
                newHotspotHorde = value;
                OnPropertyChanged(nameof(NewHotspotHorde));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
            }
        }

        bool newHotspotAlliance;
        public bool NewHotspotAlliance
        {
            get => newHotspotAlliance;
            set
            {
                newHotspotAlliance = value;
                OnPropertyChanged(nameof(NewHotspotAlliance));
                OnPropertyChanged(nameof(SaveHotspotCommandEnabled));
            }
        }

        bool newHotspotSafeForGrinding;
        public bool NewHotspotSafeForGrinding
        {
            get => newHotspotSafeForGrinding;
            set
            {
                newHotspotSafeForGrinding = value;
                OnPropertyChanged(nameof(NewHotspotSafeForGrinding));
            }
        }

        // ProbeFields
        [ProbeField]
        public string CurrentState
        {
            get => probe.CurrentState;
        }

        [ProbeField]
        public string CurrentPosition
        {
            get => probe.CurrentPosition;
        }

        [ProbeField]
        public string CurrentZone
        {
            get => probe.CurrentZone;
        }

        [ProbeField]
        public string TargetName
        {
            get => probe.TargetName;
        }

        [ProbeField]
        public string TargetClass
        {
            get => probe.TargetClass;
        }

        [ProbeField]
        public string TargetCreatureType
        {
            get => probe.TargetCreatureType;
        }

        [ProbeField]
        public string TargetPosition
        {
            get => probe.TargetPosition;
        }

        [ProbeField]
        public string TargetRange
        {
            get => probe.TargetRange;
        }

        [ProbeField]
        public string TargetFactionId
        {
            get => probe.TargetFactionId;
        }

        [ProbeField]
        public string TargetIsCasting
        {
            get => probe.TargetIsCasting;
        }

        [ProbeField]
        public string TargetIsChanneling
        {
            get => probe.TargetIsChanneling;
        }

        [ProbeField]
        public string UpdateLatency
        {
            get => probe.UpdateLatency;
        }

        // BotSettings
        [BotSetting]
        public string Food
        {
            get => botSettings.Food;
            set => botSettings.Food = value;
        }

        [BotSetting]
        public string Drink
        {
            get => botSettings.Drink;
            set => botSettings.Drink = value;
        }

        [BotSetting]
        public string TargetingIncludedNames
        {
            get => botSettings.TargetingIncludedNames;
            set => botSettings.TargetingIncludedNames = value;
        }

        [BotSetting]
        public string TargetingExcludedNames
        {
            get => botSettings.TargetingExcludedNames;
            set
            {
                botSettings.TargetingExcludedNames = value;
                OnPropertyChanged(nameof(TargetingExcludedNames));
            }
        }

        [BotSetting]
        public int LevelRangeMin
        {
            get => botSettings.LevelRangeMin;
            set
            {
                botSettings.LevelRangeMin = value;
                OnPropertyChanged(nameof(LevelRangeMin));
            }
        }

        [BotSetting]
        public int LevelRangeMax
        {
            get => botSettings.LevelRangeMax;
            set
            {
                botSettings.LevelRangeMax = value;
                OnPropertyChanged(nameof(LevelRangeMax));
            }
        }

        [BotSetting]
        public bool CreatureTypeBeast
        {
            get => botSettings.CreatureTypeBeast;
            set
            {
                botSettings.CreatureTypeBeast = value;
                OnPropertyChanged(nameof(CreatureTypeBeast));
            }
        }

        [BotSetting]
        public bool CreatureTypeDragonkin
        {
            get => botSettings.CreatureTypeDragonkin;
            set
            {
                botSettings.CreatureTypeDragonkin = value;
                OnPropertyChanged(nameof(CreatureTypeDragonkin));
            }
        }

        [BotSetting]
        public bool CreatureTypeDemon
        {
            get => botSettings.CreatureTypeDemon;
            set
            {
                botSettings.CreatureTypeDemon = value;
                OnPropertyChanged(nameof(CreatureTypeDemon));
            }
        }

        [BotSetting]
        public bool CreatureTypeElemental
        {
            get => botSettings.CreatureTypeElemental;
            set
            {
                botSettings.CreatureTypeElemental = value;
                OnPropertyChanged(nameof(CreatureTypeElemental));
            }
        }

        [BotSetting]
        public bool CreatureTypeHumanoid
        {
            get => botSettings.CreatureTypeHumanoid;
            set
            {
                botSettings.CreatureTypeHumanoid = value;
                OnPropertyChanged(nameof(CreatureTypeHumanoid));
            }
        }

        [BotSetting]
        public bool CreatureTypeUndead
        {
            get => botSettings.CreatureTypeUndead;
            set
            {
                botSettings.CreatureTypeUndead = value;
                OnPropertyChanged(nameof(CreatureTypeUndead));
            }
        }

        [BotSetting]
        public bool CreatureTypeGiant
        {
            get => botSettings.CreatureTypeGiant;
            set
            {
                botSettings.CreatureTypeGiant = value;
                OnPropertyChanged(nameof(CreatureTypeGiant));
            }
        }

        [BotSetting]
        public bool UnitReactionHostile
        {
            get => botSettings.UnitReactionHostile;
            set
            {
                botSettings.UnitReactionHostile = value;
                OnPropertyChanged(nameof(UnitReactionHostile));
            }
        }

        [BotSetting]
        public bool UnitReactionUnfriendly
        {
            get => botSettings.UnitReactionUnfriendly;
            set
            {
                botSettings.UnitReactionUnfriendly = value;
                OnPropertyChanged(nameof(UnitReactionUnfriendly));
            }
        }

        [BotSetting]
        public bool UnitReactionNeutral
        {
            get => botSettings.UnitReactionNeutral;
            set
            {
                botSettings.UnitReactionNeutral = value;
                OnPropertyChanged(nameof(UnitReactionNeutral));
            }
        }

        [BotSetting]
        public bool LootPoor
        {
            get => botSettings.LootPoor;
            set
            {
                botSettings.LootPoor = value;
                OnPropertyChanged(nameof(LootPoor));
            }
        }

        [BotSetting]
        public bool LootCommon
        {
            get => botSettings.LootCommon;
            set
            {
                botSettings.LootCommon = value;
                OnPropertyChanged(nameof(LootCommon));
            }
        }

        [BotSetting]
        public bool LootUncommon
        {
            get => botSettings.LootUncommon;
            set
            {
                botSettings.LootUncommon = value;
                OnPropertyChanged(nameof(LootUncommon));
            }
        }

        [BotSetting]
        public string LootExcludedNames
        {
            get => botSettings.LootExcludedNames;
            set
            {
                botSettings.LootExcludedNames = value;
                OnPropertyChanged(nameof(LootExcludedNames));
            }
        }

        [BotSetting]
        public bool SellPoor
        {
            get => botSettings.SellPoor;
            set
            {
                botSettings.SellPoor = value;
                OnPropertyChanged(nameof(SellPoor));
            }
        }

        [BotSetting]
        public bool SellCommon
        {
            get => botSettings.SellCommon;
            set
            {
                botSettings.SellCommon = value;
                OnPropertyChanged(nameof(SellCommon));
            }
        }

        [BotSetting]
        public bool SellUncommon
        {
            get => botSettings.SellUncommon;
            set
            {
                botSettings.SellUncommon = value;
                OnPropertyChanged(nameof(SellUncommon));
            }
        }

        [BotSetting]
        public string SellExcludedNames
        {
            get => botSettings.SellExcludedNames;
            set
            {
                botSettings.SellExcludedNames = value;
                OnPropertyChanged(nameof(SellExcludedNames));
            }
        }

        [BotSetting]
        public Hotspot GrindingHotspot
        {
            get => botSettings.GrindingHotspot;
            set
            {
                botSettings.GrindingHotspot = value;
                OnPropertyChanged(nameof(GrindingHotspot));
            }
        }

        [BotSetting]
        public TravelPath CurrentTravelPath
        {
            get => botSettings.CurrentTravelPath;
            set
            {
                botSettings.CurrentTravelPath = value;
                OnPropertyChanged(nameof(CurrentTravelPath));
                OnPropertyChanged(nameof(StartTravelPathCommandEnabled));
            }
        }

        [BotSetting]
        public bool UseTeleportKillswitch
        {
            get => botSettings.UseTeleportKillswitch;
            set
            {
                botSettings.UseTeleportKillswitch = value;
                OnPropertyChanged(nameof(UseTeleportKillswitch));
            }
        }

        [BotSetting]
        public bool UseStuckInPositionKillswitch
        {
            get => botSettings.UseStuckInPositionKillswitch;
            set
            {
                botSettings.UseStuckInPositionKillswitch = value;
                OnPropertyChanged(nameof(UseStuckInPositionKillswitch));
            }
        }

        [BotSetting]
        public bool UseStuckInStateKillswitch
        {
            get => botSettings.UseStuckInStateKillswitch;
            set
            {
                botSettings.UseStuckInStateKillswitch = value;
                OnPropertyChanged(nameof(UseStuckInStateKillswitch));
            }
        }

        [BotSetting]
        public bool UsePlayerTargetingKillswitch
        {
            get => botSettings.UsePlayerTargetingKillswitch;
            set
            {
                botSettings.UsePlayerTargetingKillswitch = value;
                OnPropertyChanged(nameof(UsePlayerTargetingKillswitch));
            }
        }

        [BotSetting]
        public bool UsePlayerProximityKillswitch
        {
            get => botSettings.UsePlayerProximityKillswitch;
            set
            {
                botSettings.UsePlayerProximityKillswitch = value;
                OnPropertyChanged(nameof(UsePlayerProximityKillswitch));
            }
        }

        [BotSetting]
        public int TargetingWarningTimer
        {
            get => botSettings.TargetingWarningTimer;
            set
            {
                botSettings.TargetingWarningTimer = value;
                OnPropertyChanged(nameof(TargetingWarningTimer));
            }
        }

        [BotSetting]
        public int TargetingStopTimer
        {
            get => botSettings.TargetingStopTimer;
            set
            {
                botSettings.TargetingStopTimer = value;
                OnPropertyChanged(nameof(TargetingStopTimer));
            }
        }

        [BotSetting]
        public int ProximityWarningTimer
        {
            get => botSettings.ProximityWarningTimer;
            set
            {
                botSettings.ProximityWarningTimer = value;
                OnPropertyChanged(nameof(ProximityWarningTimer));
            }
        }

        [BotSetting]
        public int ProximityStopTimer
        {
            get => botSettings.ProximityStopTimer;
            set
            {
                botSettings.ProximityStopTimer = value;
                OnPropertyChanged(nameof(ProximityStopTimer));
            }
        }

        [BotSetting]
        public string PowerlevelPlayerName
        {
            get => botSettings.PowerlevelPlayerName;
            set
            {
                botSettings.PowerlevelPlayerName = value;
                OnPropertyChanged(nameof(PowerlevelPlayerName));
            }
        }

        bool reverseTravelPath;
        public bool ReverseTravelPath
        {
            get => reverseTravelPath;
            set
            {
                reverseTravelPath = value;
                OnPropertyChanged(nameof(ReverseTravelPath));
            }
        }

        [BotSetting]
        public bool UseVerboseLogging
        {
            get => botSettings.UseVerboseLogging;
            set
            {
                botSettings.UseVerboseLogging = value;
                OnPropertyChanged(nameof(UseVerboseLogging));
            }
        }

        #endregion

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        void Log(string message) =>
            ConsoleOutput.Add($"({DateTime.Now.ToShortTimeString()}) {message}");

        void InitializeTravelPaths()
        {
            TravelPaths = new ObservableCollection<TravelPath>(Repository.ListTravelPaths());
            TravelPaths.Insert(0, null);
            OnPropertyChanged(nameof(CurrentTravelPath));
            OnPropertyChanged(nameof(TravelPaths));
        }

        void InitializeHotspots()
        {
            var hotspots = Repository.ListHotspots()
                .OrderBy(h => h.MinLevel)
                .ThenBy(h => h.Zone)
                .ThenBy(h => h.Description);

            Hotspots = new ObservableCollection<Hotspot>(hotspots);
            Hotspots.Insert(0, null);
            GrindingHotspot = Hotspots.FirstOrDefault(h => h?.Id == botSettings.GrindingHotspotId);
            OnPropertyChanged(nameof(Hotspots));
        }

        void InitializeNpcs()
        {
            var npcs = Repository.ListNpcs()
                .OrderBy(n => n.Horde)
                .ThenBy(n => n.Name);

            var repairNpcs = npcs.Where(n => n.Repairs);
            var inkeeperNpcs = npcs.Where(n => n.IsInnkeeper);
            var ammoNpcs = npcs.Where(n => n.SellsAmmo);

            RepairNpcs = new ObservableCollection<Npc>(repairNpcs);
            InkeeperNpcs = new ObservableCollection<Npc>(inkeeperNpcs);
            AmmoNpcs = new ObservableCollection<Npc>(ammoNpcs);

            RepairNpcs.Insert(0, null);
            InkeeperNpcs.Insert(0, null);
            AmmoNpcs.Insert(0, null);

            OnPropertyChanged(nameof(RepairNpcs));
            OnPropertyChanged(nameof(InkeeperNpcs));
            OnPropertyChanged(nameof(AmmoNpcs));
        }

        public void InitializeObjectManager()
        {
            ObjectManager.Initialize(probe);
            ObjectManager.StartEnumeration();
            Task.Run(async () => await InitializeCommandHandler());
        }
        
        void UpdatePropertiesWithAttribute(Type type)
        {
            foreach (var propertyInfo in GetType().GetProperties())
            {
                if (Attribute.IsDefined(propertyInfo, type))
                    OnPropertyChanged(propertyInfo.Name);
            }
        }

        void OnChatMessageCallback(object sender, OnChatMessageArgs e)
        {
            var player = ObjectManager.Player;
            if (player != null && !CityNames.Contains(ObjectManager.ZoneText))
            {
                if (e.ChatChannel == "Say")
                    DiscordClientWrapper.SendMessage($"{player.Name} saw a chat message from {e.UnitName}: {e.Message}");
                else if (e.ChatChannel == "Whisper")
                    DiscordClientWrapper.SendMessage($"{player.Name} received a whisper from {e.UnitName}: {e.Message}");
            }
        }

        async Task InitializeCommandHandler()
        {
            WoWEventHandler.OnChatMessage += OnChatMessageCallback;

            while (true)
            {
                try
                {
                    var player = ObjectManager.Player;

                    if (player != null)
                    {
                        if (!readyForCommands)
                        {
                            if (string.IsNullOrWhiteSpace(player.Name) || player.Level == 0)
                                continue;

                            Repository.DeleteCommandsForPlayer(player.Name);
                            SignLatestReport(player, false);
                            readyForCommands = true;

                            await Task.Delay(2000); // wait for 1 second to sign the latest report, otherwise you'll randomly report on login
                        }
                        else
                        {
                            var commands = Repository.GetCommandsForPlayer(ObjectManager.Player.Name);

                            foreach (var command in commands)
                            {
                                switch (command.Command)
                                {
                                    case "!start":
                                        Start();
                                        break;
                                    case "!stop":
                                        Stop();
                                        break;
                                    case "!chat":
                                        ThreadSynchronizer.RunOnMainThread(() =>
                                        {
                                            player.LuaCall($"SendChatMessage('{command.Args}')");
                                        });
                                        break;
                                    case "!whisper":
                                        ThreadSynchronizer.RunOnMainThread(() =>
                                        {
                                            var splitArgs = command.Args.Split(' ');
                                            var recipient = splitArgs[0];
                                            var message = string.Join(" ", splitArgs.Skip(1));

                                            player.LuaCall($"SendChatMessage('{message}', 'WHISPER', nil, '{recipient}')");
                                        });
                                        break;
                                    case "!logout":
                                        ThreadSynchronizer.RunOnMainThread(() =>
                                        {
                                            player.LuaCall("Logout()");
                                        });
                                        break;
                                    case "!hearthstone":
                                        ThreadSynchronizer.RunOnMainThread(() =>
                                        {
                                            var hearthstone = Inventory.GetAllItems().FirstOrDefault(i => i.Info.Name == "Hearthstone");
                                            if (hearthstone != null)
                                                hearthstone.Use();
                                        });
                                        break;
                                    case "!status":
                                        ThreadSynchronizer.RunOnMainThread(() =>
                                        {
                                            string[] hordeRaces = { "Orc", "Troll", "Tauren", "Undead" };
                                            string[] aRaces = { "Human", "Dwarf", "Night elf", "Gnome", "Tauren", "Troll" };
                                            var status = new StringBuilder();
                                            var race = player.LuaCallWithResults("{0} = UnitRace('player')")[0];
                                            var raceString = aRaces.Contains(race) ? $"a {race}" : $"an {race}";
                                            var alive = player.Health <= 0 || player.InGhostForm ? "dead" : "alive";
                                            var greeting = hordeRaces.Contains(race) ? "Zug zug!" : "Hail, and well met!";
                                            status.Append($"{greeting} {player.Name} reporting in. I'm {raceString} {player.Class} on the server {ObjectManager.ServerName}.\n");
                                            status.Append($"I'm currently level {player.Level}, and I'm {alive}!\n");
                                            if (CurrentBot.Running())
                                            {
                                                status.Append($"I'm currently in the {probe.CurrentState}.\n");
                                                status.Append($"I'm grinding in {GrindingHotspot.DisplayName}.\n");
                                            }
                                            else
                                            {
                                                status.Append("I'm currently idle.");
                                            }
                                            DiscordClientWrapper.SendMessage(status.ToString());
                                        });
                                        break;
                                    case "!info":
                                        var dir = Path.GetDirectoryName(Assembly.GetAssembly(typeof(MainViewModel)).CodeBase);
                                        var path = new UriBuilder(dir).Path;
                                        var bloogBotExe = $"{path}\\BloogBot.exe";
                                        var bloogBotExeAssemblyVersion = AssemblyName.GetAssemblyName(bloogBotExe).Version;
                                        var botDll = $"{path}\\{CurrentBot.FileName}";
                                        var botAssemblyVersion = AssemblyName.GetAssemblyName(botDll).Version;
                                        var sb = new StringBuilder();
                                        sb.Append($"{player.Name}\n");
                                        sb.Append($"BloogBot.exe version {bloogBotExeAssemblyVersion}\n");
                                        sb.Append($"{CurrentBot.FileName} version {botAssemblyVersion}\n");
                                        DiscordClientWrapper.SendMessage(sb.ToString());
                                        break;
                                }

                                Repository.DeleteCommand(command.Id);
                            }

                            SignLatestReport(player, true);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Log(e);
                }

                await Task.Delay(250);
            }
        }

        void SignLatestReport(LocalPlayer player, bool reportIn)
        {
            var summary = Repository.GetLatestReportSignatures();

            if (summary.CommandId != -1 && !summary.Signatures.Any(s => s.Player == player.Name))
            {
                if (reportIn)
                {
                    var active = CurrentBot.Running() ? $"Grinding at: {GrindingHotspot.DisplayName}" : "Idle";
                    DiscordClientWrapper.SendMessage($"{player.Name} ({player.Class} - Level {player.Level}). Server: {ObjectManager.ServerName}. {active}.");
                }

                Repository.AddReportSignature(player.Name, summary.CommandId);
            }
        }
    }

    public class BotSettingAttribute : Attribute
    {
    }

    public class ProbeFieldAttribute : Attribute
    {
    }
}