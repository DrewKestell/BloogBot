using BloogBot.AI;
using BloogBot.Game;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Windows.Input;
using BloogBot.Models.Dto;
using System.Threading.Tasks;
using System.Timers;

namespace BloogBot.UI
{
    public class MainViewModel : INotifyPropertyChanged
    {
        const string COMMAND_ERROR = "An error occured. See Console for details.";

        static readonly string[] CityNames = { "Orgrimmar", "Thunder Bluff", "Undercity", "Stormwind", "Darnassus", "Ironforge" };

        readonly BotRunner botRunner;
        readonly CharacterState characterState;
        readonly BotSettings botSettings;
        bool readyForCommands;
        Timer timer;

        public IntPtr processPointer;

        private Socket _socket;
        private Task _heartbeatTask;
        private Task _commandListenerTask;

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
            characterState = new CharacterState()
            {
                ProcessId = Process.GetCurrentProcess().Id
            };

            botRunner = new BotRunner(characterState);

            InitializeObjectManager();

            _commandListenerTask = Task.Run(AsyncCommandListenerStart);
            _heartbeatTask = Task.Run(AsyncHeartbeatStart);
        }

        #region Commands

        // Start command
        ICommand startCommand;

        public ICommand StartCommand =>
            startCommand ?? (startCommand = new CommandHandler(UiStart, true));

        void UiStart()
        {
            Start();
        }

        void Start()
        {
            try
            {
                botRunner.Start();

                OnPropertyChanged(nameof(StartCommandEnabled));
                OnPropertyChanged(nameof(StopCommandEnabled));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
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

                    if (characterState != null)
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
                                                if (instanceCommand.CommandName == InstanceCommand.START)
                                                {
                                                    Start();
                                                    characterState.StartRequested = true;
                                                }
                                                else if (instanceCommand.CommandName == InstanceCommand.STOP)
                                                {
                                                    Stop();
                                                    characterState.StopRequested = true;
                                                }
                                                else if (instanceCommand.CommandName == InstanceCommand.SET_ACCOUNT_INFO)
                                                {
                                                    botRunner.SetAccountInfo(instanceCommand.CommandParam1, int.Parse(instanceCommand.CommandParam2), instanceCommand.CommandParam3);
                                                    characterState.SetAccountInfoRequested = true;
                                                }
                                            }
                                        }
                                        catch (Exception e)
                                        {
                                            Console.WriteLine(string.Format("{0}", e.Message));
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
            string json = "";

            while (true)
            {
                try
                {
                    OnPropertyChanged(nameof(StartCommandEnabled));
                    OnPropertyChanged(nameof(StopCommandEnabled));

                    if (characterState != null)
                    {
                        characterState.IsRunning = botRunner.Running();
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
                                string instanceUpdateJson = JsonConvert.SerializeObject(characterState);

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
                    Console.WriteLine(string.Format("{0} caused by {1}", e.Message, json));
                }
                await Task.Delay(500);
            }
        }

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
                botRunner.Stop();
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
        public bool StartCommandEnabled => !botRunner.Running();

        public bool StopCommandEnabled => botRunner.Running();

        #endregion

        public void InitializeObjectManager()
        {
            ObjectManager.Initialize(characterState);
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