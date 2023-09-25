using BloogBot.AI;
using BloogBot.Game;
using Bootstrapper;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Input;
using BloogBot.Models.Dto;
using static BloogBot.UI.WinImports;
using System.Windows.Threading;
using System.Threading.Tasks;
using BloogBot.AI.SharedStates;
using BloogBot.Game.Enums;
using BloogBot.Models.Enums;

namespace BloogBot.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string COMMAND_ERROR = "An error occured. See Console for details.";

        static readonly string[] CityNames = { "Orgrimmar", "Thunder Bluff", "Undercity", "Stormwind", "Darnassus", "Ironforge" };

        readonly BotLoader botLoader = new BotLoader();
        readonly CharacterState instanceUpdate;
        readonly BotSettings botSettings;
        bool readyForCommands;

        IDependencyContainer dependencyContainer;

        public IntPtr processPointer;

        private Socket _socket;
        private Task _heartbeatTask;
        private Task _commandListenerTask;

        private string _activity;
        private Race _race;
        private Class _class;
        private Role _role;

        public MainViewModel()
        {
            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var botSettingsFilePath = Path.Combine(currentFolder, "botSettings.json");
            botSettings = JsonConvert.DeserializeObject<BotSettings>(File.ReadAllText(botSettingsFilePath));
            UpdatePropertiesWithAttribute(typeof(BotSettingAttribute));

            Logger.Initialize(botSettings);
            SqliteRepository.Initialize();
            DiscordClientWrapper.Initialize(botSettings);
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                _socket.Connect(IPAddress.Parse(botSettings.ListenAddress), botSettings.Port);
            }
            catch (Exception e)
            {
                Logger.Log(e.Message);
            }
            void killswitch()
            {
                Stop();
            }
            instanceUpdate = new CharacterState(killswitch)
            {
                ProcessId = Process.GetCurrentProcess().Id
            };

            InitializeObjectManager();

            _commandListenerTask = Task.Run(AsyncCommandListenerStart);
            _heartbeatTask = Task.Run(AsyncHeartbeatStart);
        }
        public ObservableCollection<IBot> Bots { get; private set; }

        #region Commands

        // Start command
        ICommand startCommand;

        void UiStart()
        {
            Start();
        }
        void StopCallback()
        {
            Stop();
        }

        void Start()
        {
            try
            {
                ReloadBots();

                currentBot.Start(dependencyContainer, StopCallback);

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void ReloadBots()
        {
            Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

            CurrentBot = GetPreferredBot();

            dependencyContainer = CurrentBot.GetDependencyContainer(botSettings, instanceUpdate);
            Console.WriteLine($"Bots: {CurrentBot.Name} {dependencyContainer.AccountName}");
        }
        private IBot GetPreferredBot()
        {
            switch (_class)
            {
                case Class.Druid:
                    if ((_role & Role.Tank) == Role.Tank)
                    {
                        if ((_role & Role.Damage) == Role.Damage)
                        {
                            return Bots.First(b => b.Name == "Balance Druid");
                        }
                        else
                        {
                            return Bots.First(b => b.Name == "Restoration Druid");
                        }
                    }
                    return Bots.First(b => b.Name == "Feral Druid");
                case Class.Hunter:
                    return Bots.First(b => b.Name == "Beastmaster Hunter");
                case Class.Mage:
                    return Bots.First(b => b.Name == "Frost Mage");
                case Class.Paladin:
                    if ((_role & Role.Tank) == Role.Tank)
                    {
                        return Bots.First(b => b.Name == "Protection Paladin");
                    }
                    else if ((_role & Role.Damage) == Role.Damage)
                    {
                        return Bots.First(b => b.Name == "Retribution Paladin");
                    }
                    return Bots.First(b => b.Name == "Holy Paladin");
                case Class.Priest:
                    if ((_role & Role.Healer) == Role.Healer)
                    {
                        if (_activity == "PvP")
                        {
                            return Bots.First(b => b.Name == "Discipline Priest");
                        }
                        else
                        {
                            return Bots.First(b => b.Name == "Holy Priest");
                        }
                    }
                    return Bots.First(b => b.Name == "Shadow Priest");
                case Class.Rogue:
                    if (_activity == "PvP")
                    {
                        return Bots.First(b => b.Name == "Backstab Rogue");
                    }
                    else
                    {
                        return Bots.First(b => b.Name == "Combat Rogue");
                    }
                case Class.Shaman:
                    if ((_role & Role.Tank) == Role.Tank)
                    {
                        return Bots.First(b => b.Name == "Enhancement Shaman");
                    }
                    else if ((_role & Role.Damage) == Role.Damage)
                    {
                        return Bots.First(b => b.Name == "Elemental Shaman");
                    }
                    return Bots.First(b => b.Name == "Elemental Shaman");
                case Class.Warlock:
                    return Bots.First(b => b.Name == "Frost Mage");
                case Class.Warrior:
                    if ((_role & Role.Tank) == Role.Tank)
                    {
                        return Bots.First(b => b.Name == "Protection Warrior");
                    }
                    else if (_activity == "PvP")
                    {
                        return Bots.First(b => b.Name == "Arms Warrior");
                    }
                    break;
            }
            return Bots.First(b => b.Name == "Fury Warrior");
        }

        private async Task AsyncCommandListenerStart()
        {
            byte[] buffer = new byte[1024];
            string json = "";

            while (true)
            {
                try
                {
                    OnPropertyChanged(nameof(StartCommandEnabled));
                    OnPropertyChanged(nameof(StopCommandEnabled));

                    if (instanceUpdate != null)
                    {
                        try
                        {
                            if (_socket == null)
                            {
                                return;
                            }
                            if (!_socket.Connected)
                            {
                                try
                                {
                                    _socket.Connect(IPAddress.Parse(botSettings.ListenAddress), botSettings.Port);
                                }
                                catch (Exception e)
                                {
                                    Logger.Log(e.Message);
                                }
                            }
                            if (_socket.Connected)
                            {
                                int receivedDataLength = _socket.Receive(buffer);
                                if (receivedDataLength > 0)
                                {
                                    json = Encoding.UTF8.GetString(buffer, 0, receivedDataLength);
                                    Console.WriteLine(string.Format("Receivied command: {0}", json));

                                    if (json.Length > 0 && json.StartsWith("{"))
                                    {
                                        try
                                        {
                                            InstanceCommand instanceCommand = JsonConvert.DeserializeObject<InstanceCommand>(json);

                                            if (instanceCommand != null)
                                            {
                                                if (instanceCommand.StateName == InstanceCommand.LOGIN)
                                                {
                                                    instanceUpdate.LoginRequested = true;

                                                    Enum.TryParse(instanceCommand.CommandParam1, out _race);
                                                    Enum.TryParse(instanceCommand.CommandParam2, out _class);
                                                    Enum.TryParse(instanceCommand.CommandParam3, out _role);

                                                    instanceUpdate.Role = _role;
                                                    _activity = instanceCommand.CommandParam4;

                                                    ReloadBots();

                                                    currentBot.Login(AccountHelper.GetAccountByRaceAndClass(_race, _class));
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(string.Format("{0} caused by {1}", e.Message, json));
                                        }
                                        Array.Clear(buffer, 0, buffer.Length);
                                    }
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            try
                            {
                                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            }
                            catch
                            {
                                _socket.Close();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format($"{0} caused by {1}", e.Message, json));
                }
                await Task.Delay(500);
            }
        }
        private async Task AsyncHeartbeatStart()
        {
            byte[] buffer = new byte[1024];
            string json = "";

            while (true)
            {
                try
                {
                    OnPropertyChanged(nameof(StartCommandEnabled));
                    OnPropertyChanged(nameof(StopCommandEnabled));

                    if (instanceUpdate != null)
                    {
                        try
                        {
                            if (_socket == null)
                            {
                                return;
                            }
                            if (!_socket.Connected)
                            {
                                try
                                {
                                    _socket.Connect(IPAddress.Parse(botSettings.ListenAddress), botSettings.Port);
                                }
                                catch (Exception e)
                                {
                                    Logger.Log(e.Message);
                                }
                            }
                            if (_socket.Connected)
                            {
                                string instanceUpdateJson = JsonConvert.SerializeObject(instanceUpdate);
                                //Console.WriteLine(string.Format("Sending message: {0}", instanceUpdateJson));
                                _socket.Send(Encoding.ASCII.GetBytes(instanceUpdateJson));
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            try
                            {
                                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                            }
                            catch
                            {
                                _socket.Close();
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format($"{0} caused by {1}", e.Message, json));
                }
                await Task.Delay(500);
            }
        }

        public ICommand StartCommand =>
            startCommand ?? (startCommand = new CommandHandler(UiStart, true));

        // Stop command
        ICommand stopCommand;

        void UiStop()
        {
            Stop();
        }

        void Stop()
        {
            try
            {
                CurrentBot.Stop();
            }
            catch (Exception e)
            {
                Logger.Log(e);
            }
        }

        public ICommand StopCommand =>
            stopCommand ?? (stopCommand = new CommandHandler(UiStop, true));

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        #endregion

        #region Observables
        public bool StartCommandEnabled => !currentBot.Running();

        public bool StopCommandEnabled => currentBot.Running();

        // General
        IBot currentBot;
        public IBot CurrentBot
        {
            get => currentBot;
            set
            {
                currentBot = value;
            }
        }

        #endregion

        public void InitializeObjectManager()
        {
            ObjectManager.Initialize(instanceUpdate);
            ObjectManager.StartEnumeration();
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
    }

    public class BotSettingAttribute : Attribute
    {
    }
}