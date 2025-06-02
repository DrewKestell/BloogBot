using Communication;
using StateManagerUI.Handlers;
using System.ComponentModel;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace StateManagerUI.Views
{
    public sealed class StateManagerViewModel : INotifyPropertyChanged
    {
        private Timer? _statusPollTimer;
        private readonly TimeSpan _pollInterval = TimeSpan.FromSeconds(10);

        private readonly Dictionary<CharacterDefinition, CharacterDefinition> _characterStates = [];

        public ICommand LocalStateManagerLoadCommand { get; } = new CommandHandler(
            () =>
            {

            }, true
        );

        public ICommand StateManagerConnectCommand { get; } = new CommandHandler(
            () =>
            {

            }, true
        );

        public ICommand StateManagerDisconnectCommand { get; } = new CommandHandler(
            () =>
            {

            }, true
        );

        public StateManagerViewModel()
        {

            OnPropertyChanged(nameof(SelectCharacterIndex));
        }

        private void StartStatusTimer()
        {
            _statusPollTimer ??= new Timer(async _ => await PollServerStatusAsync(), null, TimeSpan.Zero, _pollInterval);
        }

        private async Task PollServerStatusAsync()
        {
            try
            {
                RealmState = await CheckPortStatus(3724) ? "UP" : "DOWN";
                WorldState = await CheckPortStatus(8085) ? "UP" : "DOWN";

                // You could replace this with a real query to the server
                TotalPopulation = RealmState == "UP" && WorldState == "UP" ? "3000" : "0";

                OnPropertyChanged(nameof(RealmState));
                OnPropertyChanged(nameof(WorldState));
                OnPropertyChanged(nameof(TotalPopulation));
            }
            catch
            {
                RealmState = "UNKNOWN";
                WorldState = "UNKNOWN";
                TotalPopulation = "--";

                OnPropertyChanged(nameof(RealmState));
                OnPropertyChanged(nameof(WorldState));
                OnPropertyChanged(nameof(TotalPopulation));
            }
        }

        private static async Task<bool> CheckPortStatus(int port, int timeoutMs = 1000)
        {
            try
            {
                using var client = new TcpClient();
                var task = client.ConnectAsync("127.0.0.1", port);
                var completed = await Task.WhenAny(task, Task.Delay(timeoutMs));
                return task.IsCompleted && client.Connected;
            }
            catch { return false; }
        }

        public async Task CheckServerStatusAsync()
        {
            var is3724 = await CheckPortStatus(3724);
            var is7878 = await CheckPortStatus(7878);
            var is8085 = await CheckPortStatus(8085);

            IsConnected = is3724 && is7878 && is8085;
        }

        public string RealmState { get; set; } = "UNKNOWN";
        public string WorldState { get; set; } = "UNKNOWN";
        public string TotalPopulation { get; set; } = "0";

        public string StateManagerUrl { get; set; } = "http://localhost:8085";
        public string MangosUrl { get; set; } = "http://localhost:7878";
        public string AdminUsername { get; set; } = "ADMINISTRATOR";
        public string AdminPassword { get; set; } = "PASSWORD";

        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public int SelectCharacterIndex => _selectedCharacterIndex;
        public int CurrentPageIndex => _currentPageIndex;

        private int _currentPageIndex { get; set; } = -1;
        private int _selectedCharacterIndex { get; set; } = -1;

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                if (_isConnected != value)
                {
                    _isConnected = value;
                    OnPropertyChanged();
                }
            }
        }

        public float OpennessValue
        {
            get => _characterStates.Count == 0 ? 0.0f : _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Openness;
            set
            {
                _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Openness = value;
                OnPropertyChanged();
            }
        }
        public float ConscientiousnessValue
        {
            get => _characterStates.Count == 0 ? 0.0f : _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Conscientiousness;
            set
            {
                _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Conscientiousness = value;
                OnPropertyChanged();
            }
        }
        public float ExtraversionValue
        {
            get => _characterStates.Count == 0 ? 0.0f : _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Extraversion;
            set
            {
                _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Extraversion = value;
                OnPropertyChanged();
            }
        }
        public float AgreeablenessValue
        {
            get => _characterStates.Count == 0 ? 0.0f : _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Agreeableness;
            set
            {
                _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Agreeableness = value;
                OnPropertyChanged();
            }
        }
        public float NeuroticismValue
        {
            get => _characterStates.Count == 0 ? 0.0f : _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Neuroticism;
            set
            {
                _characterStates.Keys.ToArray()[20 * _currentPageIndex + _selectedCharacterIndex].Neuroticism = value;
                OnPropertyChanged();
            }
        }
    }
}
