using RaidLeaderBot.Server;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RaidLeaderBot
{
    public sealed class RaidActivityViewModel : INotifyPropertyChanged
    {
        private NavigationSocketServer _navigationSocketServer;
        private DatabaseSocketServer _databaseSocketServer;
        public ObservableCollection<int> ActivityPresetIndexes { get; set; } = new ObservableCollection<int>();
        public ObservableCollection<RaidLeaderViewModel> RaidPresetViewModels { get; set; } = new ObservableCollection<RaidLeaderViewModel>();
        public ObservableCollection<RaidMemberViewModel> RaidMemberViewModels { get; set; } = new ObservableCollection<RaidMemberViewModel>();
        public CharacterState CharacterState => RaidPresetViewModels.Count > 0 ? SelectedRaid.GetCharacterStateByRaidMemberViewModel(SelectedRaidMember) : new CharacterState();
        public float PositionX => CharacterState.Position.X;
        public float PositionY => CharacterState.Position.Y;
        public float PositionZ => CharacterState.Position.Z;
        public RaidPreset SelectedRaidPreset => SelectedPresetRaidList[SelectedRaidIndex];
        public List<RaidPreset> SelectedPresetRaidList => RaidLeaderBotSettings.Instance.ActivityPresets[_activityPresetIndex];
        public RaidLeaderViewModel SelectedRaid => RaidPresetViewModels[SelectedRaidIndex];
        public RaidMemberViewModel SelectedRaidMember => SelectedRaid.RaidMemberViewModels[SelectedRaidMemberIndex];

        Task _asyncCharacterStateRefresherTask;
        public void Initialize()
        {
            _databaseSocketServer = new DatabaseSocketServer(RaidLeaderBotSettings.Instance.DatabasePort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
            _navigationSocketServer = new NavigationSocketServer(RaidLeaderBotSettings.Instance.NavigationPort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));

            ConfigSockerServer.Instance.Start();

            _databaseSocketServer.Start();
            _navigationSocketServer.Start();

            ActivityPresetIndexes.Clear();

            for (int i = 0; i < RaidLeaderBotSettings.Instance.ActivityPresets.Count; i++)
            {
                ActivityPresetIndexes.Add(i + 1);
            }

            ActivityPresetIndex = 1;
            SelectedRaidMemberIndex = 0;

            _asyncCharacterStateRefresherTask = Task.Run(StartCharacterStateRefresherAsync);
            OnPropertyChanged(nameof(ActivityPresetIndexes));
        }
        private async void StartCharacterStateRefresherAsync()
        {
            while (true)
            {
                OnPropertyChanged(nameof(CharacterState));
                OnPropertyChanged(nameof(PositionX));
                OnPropertyChanged(nameof(PositionY));
                OnPropertyChanged(nameof(PositionZ));
                OnPropertyChanged(nameof(CanSendCommand));

                await Task.Delay(100);
            }
        }
        public void AddPreset()
        {
            RaidLeaderBotSettings.Instance.ActivityPresets.Add(new List<RaidPreset>() { new RaidPreset() });

            ActivityPresetIndexes.Add(ActivityPresetIndexes.Count + 1);

            ActivityPresetIndex = ActivityPresetIndexes.Count;

            SetRaidFocusState(0, true);

            OnPropertyChanged(nameof(ActivityPresetIndexes));
            OnPropertyChanged(nameof(CanAddPreset));
            OnPropertyChanged(nameof(CanRemovePreset));
        }
        public void RemovePreset()
        {
            ActivityPresetIndexes.RemoveAt(ActivityPresetIndexes.Count - 1);

            RaidLeaderBotSettings.Instance.ActivityPresets.RemoveAt(_activityPresetIndex);

            int newIndexValue = _activityPresetIndex - 1;
            newIndexValue = Math.Max(newIndexValue, 0);
            newIndexValue = Math.Min(newIndexValue, ActivityPresetIndexes.Count - 1);

            ActivityPresetIndex = newIndexValue + 1;

            OnPropertyChanged(nameof(ActivityPresetIndexes));
            OnPropertyChanged(nameof(CanAddPreset));
            OnPropertyChanged(nameof(CanRemovePreset));
        }
        public void AddRaid()
        {
            for (int i = 0; i < RaidPresetViewModels.Count; i++)
            {
                SetRaidFocusState(i, false);
            }

            SelectedPresetRaidList.Add(new RaidPreset()
            {
                RaidMemberPresets = new List<RaidMemberPreset>() {
                        new RaidMemberPreset()
                    },
                Activity = SelectedRaidPreset.Activity,
                RaidLeaderPort = SelectedRaidPreset.RaidLeaderPort + 1
            });

            RaidPresetViewModels.Add(new RaidLeaderViewModel(SelectedPresetRaidList.Last()) { Index = RaidPresetViewModels.Count });

            SetRaidFocusState(RaidPresetViewModels.Count - 1, true);

            OnPropertyChanged(nameof(CanAddRaid));
            OnPropertyChanged(nameof(CanRemoveRaid));
        }
        public void RemoveRaid()
        {
            SelectedPresetRaidList
                .RemoveAt(SelectedRaidIndex);

            RaidPresetViewModels.RemoveAt(SelectedRaidIndex);

            int newIndex = SelectedRaidIndex - 1;
            newIndex = Math.Max(newIndex, 0);
            newIndex = Math.Min(newIndex, SelectedPresetRaidList.Count - 1);

            SetRaidFocusState(newIndex, true);

            OnPropertyChanged(nameof(CanAddRaid));
            OnPropertyChanged(nameof(CanRemoveRaid));
        }
        public void MoveTo()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.GoTo,
                CommandParam1 = DestinationMapId.ToString(),
                CommandParam2 = DestinationX.ToString(),
                CommandParam3 = DestinationY.ToString(),
                CommandParam4 = DestinationZ.ToString()
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void TeleportTo()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.ExecuteLuaCommand,
                CommandParam1 = $"SendChatMessage('.go xyz {DestinationX} {DestinationY} {DestinationZ} {DestinationMapId}')",
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void JumpAt()
        {

        }
        public void SetFacing()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.SetFacing,
                CommandParam1 = DestinationFacing.ToString(),
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void MoveForward()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.MoveForward,
                CommandParam1 = DistanceToMove.ToString(),
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void InteractWith()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.InteractWith,
                CommandParam1 = DistanceToMove.ToString(),
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void CastSpell()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.CastSpellOn,
                CommandParam1 = DistanceToMove.ToString(),
                CommandParam2 = SpellIdToCast.ToString(),
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void UseItem()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.MoveForward,
                CommandParam1 = DistanceToMove.ToString(),
                CommandParam2 = ItemIdToUse.ToString(),
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void ExecuteChatCommand()
        {
            InstanceCommand command = new InstanceCommand()
            {
                CommandAction = CommandAction.ExecuteLuaCommand,
                CommandParam1 = $"SendChatMessage('{ChatCommandToExecute}')",
            };

            SelectedRaid.QueueCommandToProcess(CharacterState.ProcessId, command);
        }
        public void CurrentPositionToDestination()
        {
            DestinationMapId = CharacterState.MapId;
            DestinationX = CharacterState.Position.X;
            DestinationY = CharacterState.Position.Y;
            DestinationZ = CharacterState.Position.Z;

            OnPropertyChanged(nameof(DestinationMapId));
            OnPropertyChanged(nameof(DestinationX));
            OnPropertyChanged(nameof(DestinationY));
            OnPropertyChanged(nameof(DestinationZ));
        }
        public void SaveConfig()
        {
            RaidLeaderBotSettings.Instance.SaveConfig();
        }
        public void SetRaidFocusState(int index, bool isFocused)
        {
            if (isFocused)
            {
                SelectedRaidIndex = index;

                SetMemberFocusState(0, true);
            }
            RaidPresetViewModels[index].IsFocused = isFocused;
        }
        public void SetMemberFocusState(int index, bool isFocused)
        {
            if (isFocused)
            {
                SelectedRaidMemberIndex = index;
            }
            RaidMemberViewModels[index].IsFocused = isFocused;
        }
        private int _activityPresetIndex;
        public int ActivityPresetIndex
        {
            get { return _activityPresetIndex + 1; }
            set
            {
                _activityPresetIndex = value - 1;

                RegenerateRaidPresetCollection();
                SetRaidFocusState(0, true);

                OnPropertyChanged(nameof(ActivityPresetIndex));
            }
        }
        private int _selectedRaidIndex;
        public int SelectedRaidIndex
        {
            get { return _selectedRaidIndex; }
            set
            {
                _selectedRaidIndex = value;

                RenegerateRaidMembersCollection();

                OnPropertyChanged(nameof(SelectedRaidIndex));
            }
        }
        private int _selectedRaidMemberIndex;
        public int SelectedRaidMemberIndex
        {
            get { return _selectedRaidMemberIndex; }
            set
            {
                _selectedRaidMemberIndex = value;

                OnPropertyChanged(nameof(SelectedRaidMemberIndex));
            }
        }
        private void RegenerateRaidPresetCollection()
        {
            for (int i = 0; i < RaidPresetViewModels.Count; i++)
            {
                SetRaidFocusState(i, false);
            }

            RaidPresetViewModels.Clear();

            for (int i = 0; i < SelectedPresetRaidList.Count; i++)
            {
                RaidPresetViewModels.Add(new RaidLeaderViewModel(SelectedPresetRaidList[i]) { Index = i });
            }

            OnPropertyChanged(nameof(RaidPresetViewModels));
            OnPropertyChanged(nameof(CanAddRaid));
            OnPropertyChanged(nameof(CanRemoveRaid));
        }
        private void RenegerateRaidMembersCollection()
        {
            for (int i = 0; i < RaidMemberViewModels.Count; i++)
            {
                SetMemberFocusState(i, false);
            }

            RaidMemberViewModels = SelectedRaid.RaidMemberViewModels;

            OnPropertyChanged(nameof(RaidMemberViewModels));
        }
        public bool CanSendCommand => RaidPresetViewModels.Count > 0 && !SelectedRaidMember.ShouldRun && CharacterState.Guid > 0;
        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;

                OnPropertyChanged(nameof(IsRunning));
            }
        }
        private int _destinationMapId;
        public int DestinationMapId
        {
            set => _destinationMapId = value;
            get => _destinationMapId;
        }
        private float _destinationX;
        public float DestinationX
        {
            set => _destinationX = value;
            get => _destinationX;
        }
        private float _destinationY;
        public float DestinationY
        {
            set => _destinationY = value;
            get => _destinationY;
        }
        private float _destinationZ;
        public float DestinationZ
        {
            set => _destinationZ = value;
            get => _destinationZ;
        }
        private float _destinationFacing;
        public float DestinationFacing
        {
            set => _destinationFacing = value;
            get => _destinationFacing;
        }
        private float _distanceToMove;
        public float DistanceToMove
        {
            set => _distanceToMove = value;
            get => _distanceToMove;
        }
        private int _spellIdToCast;
        public int SpellIdToCast
        {
            set => _spellIdToCast = value;
            get => _spellIdToCast;
        }
        private int _itemIdToUse;
        public int ItemIdToUse
        {
            set => _itemIdToUse = value;
            get => _itemIdToUse;
        }
        private string _chatCommandToExecute;
        public string ChatCommandToExecute
        {
            set => _chatCommandToExecute = value;
            get => _chatCommandToExecute;
        }

        public bool CanAddPreset => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets.Count < 10;
        public bool CanRemovePreset => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets.Count > 1;
        public bool CanAddRaid => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1].Count < 4;
        public bool CanRemoveRaid => !IsRunning && RaidLeaderBotSettings.Instance.ActivityPresets[ActivityPresetIndex - 1].Count > 1;

        public ICommand SaveConfigCommand => _saveConfigCommand ??= new CommandHandler(SaveConfig, true);
        public ICommand AddPresetCommand => _addPresetCommand ??= new CommandHandler(AddPreset, true);
        public ICommand RemovePresetCommand => _removePresetCommand ??= new CommandHandler(RemovePreset, true);
        public ICommand AddRaidCommand => _addRaidCommand ??= new CommandHandler(AddRaid, true);
        public ICommand RemoveRaidCommand => _removeRaidCommand ??= new CommandHandler(RemoveRaid, true);
        public ICommand MoveToCommand => _moveToCommand ??= new CommandHandler(MoveTo, true);
        public ICommand TeleportToCommand => _teleportToCommand ??= new CommandHandler(TeleportTo, true);
        public ICommand JumpAtCommand => _jumpAtCommand ??= new CommandHandler(JumpAt, true);
        public ICommand SetFacingCommand => _setFacingCommand ??= new CommandHandler(SetFacing, true);
        public ICommand MoveForwardCommand => _moveForwardCommand ??= new CommandHandler(MoveForward, true);
        public ICommand InteractWithCommand => _interactWithCommand ??= new CommandHandler(InteractWith, true);
        public ICommand CastSpellCommand => _castSpellCommand ??= new CommandHandler(CastSpell, true);
        public ICommand UseItemCommand => _useItemCommand ??= new CommandHandler(UseItem, true);
        public ICommand ExecuteChatCommandCommand => _executeChatCommandCommand ??= new CommandHandler(ExecuteChatCommand, true);
        public ICommand CurrentPositionToDestinationCommand => _currentPositionToDestinationCommand ??= new CommandHandler(CurrentPositionToDestination, true);

        private ICommand _saveConfigCommand;
        private ICommand _addPresetCommand;
        private ICommand _removePresetCommand;

        private ICommand _addRaidCommand;
        private ICommand _removeRaidCommand;

        private ICommand _moveToCommand;
        private ICommand _teleportToCommand;
        private ICommand _jumpAtCommand;
        private ICommand _setFacingCommand;
        private ICommand _moveForwardCommand;
        private ICommand _interactWithCommand;
        private ICommand _castSpellCommand;
        private ICommand _useItemCommand;
        private ICommand _executeChatCommandCommand;
        private ICommand _currentPositionToDestinationCommand;

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
