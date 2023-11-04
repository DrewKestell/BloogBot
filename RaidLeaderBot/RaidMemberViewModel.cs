using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using static RaidMemberBot.Constants.Enums;
using System.Windows.Input;
using System.Security.Policy;

namespace RaidLeaderBot
{
    public sealed class RaidMemberViewModel : INotifyPropertyChanged
    {
        public int Index { get; set; }
        public IEnumerable<TargetMarkers> EnumTargetMarkers
        {
            get
            {
                return Enum.GetValues(typeof(TargetMarkers)).Cast<TargetMarkers>();
            }
        }
        public IEnumerable<ClassId> ClassIds
        {
            get
            {
                switch (Race)
                {
                    case Race.Human:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Paladin, ClassId.Rogue, ClassId.Priest, ClassId.Mage, ClassId.Warlock };
                    case Race.Dwarf:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Paladin, ClassId.Hunter, ClassId.Rogue, ClassId.Priest };
                    case Race.NightElf:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Hunter, ClassId.Rogue, ClassId.Priest, ClassId.Druid };
                    case Race.Gnome:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Rogue, ClassId.Mage, ClassId.Warlock };
                    case Race.Orc:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Hunter, ClassId.Rogue, ClassId.Shaman, ClassId.Warlock };
                    case Race.Undead:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Rogue, ClassId.Priest, ClassId.Mage, ClassId.Warlock };
                    case Race.Tauren:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Hunter, ClassId.Shaman, ClassId.Druid };
                    case Race.Troll:
                        return new List<ClassId>() { ClassId.Warrior, ClassId.Hunter, ClassId.Rogue, ClassId.Priest, ClassId.Shaman, ClassId.Mage };
                }

                return Enum.GetValues(typeof(ClassId)).Cast<ClassId>();
            }
        }
        public IEnumerable<Race> Races
        {
            get
            {
                return IsAlliance ? new List<Race>() { Race.Human, Race.Dwarf, Race.NightElf, Race.Gnome } : new List<Race>() { Race.Orc, Race.Undead, Race.Tauren, Race.Troll };
            }
        }
        public RaidMemberPreset _raidMemberPreset;
        public RaidMemberViewModel()
        {
            _raidMemberPreset = new RaidMemberPreset();
        }
        public RaidMemberViewModel(RaidMemberPreset raidMemberPreset)
        {
            _raidMemberPreset = raidMemberPreset;

            Header = $"No client connected";

            OnPropertyChanged(nameof(Race));
            OnPropertyChanged(nameof(Class));
            OnPropertyChanged(nameof(Level));
        }

        ICommand _startCommand;
        ICommand _stopCommand;

        public ICommand StartCommand => _startCommand ??= new CommandHandler(StartBot, true);
        public ICommand StopCommand => _stopCommand ??= new CommandHandler(StopBot, true);

        public void StartBot()
        {
            ShouldRun = true;
        }

        public void StopBot()
        {
            ShouldRun = false;
        }
        public void SwapFaction()
        {
            bool wasAlliance = IsAlliance;
            IsAlliance = !IsAlliance;

            if (wasAlliance)
            {
                if (Class == ClassId.Paladin)
                {
                    _raidMemberPreset.Class = ClassId.Shaman;
                }

                switch (Class)
                {
                    case ClassId.Warrior:
                        switch (Race)
                        { 
                            case Race.Human:
                                _raidMemberPreset.Race = Race.Undead;
                                break;
                            case Race.Dwarf:
                                _raidMemberPreset.Race = Race.Orc;
                                break;
                            case Race.NightElf:
                                _raidMemberPreset.Race = Race.Tauren;
                                break;
                            case Race.Gnome:
                                _raidMemberPreset.Race = Race.Troll;
                                break;
                        }
                        break;
                    case ClassId.Hunter:
                        if (Race == Race.Dwarf)
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Tauren;
                        }
                        break;
                    case ClassId.Rogue:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        }
                        else if (Race == Race.Dwarf)
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case ClassId.Priest:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case ClassId.Shaman:
                        if (Race == Race.Dwarf)
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        } else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case ClassId.Mage:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case ClassId.Warlock:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        } else
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        }
                        break;
                    case ClassId.Druid:
                        _raidMemberPreset.Race = Race.Tauren;
                        break;
                }
            }
            else
            {
                if (Class == ClassId.Shaman)
                {
                    _raidMemberPreset.Class = ClassId.Paladin;
                }
                switch (Class)
                {
                    case ClassId.Warrior:
                        switch (Race)
                        {
                            case Race.Undead:
                                _raidMemberPreset.Race = Race.Human;
                                break;
                            case Race.Orc:
                                _raidMemberPreset.Race = Race.Dwarf;
                                break;
                            case Race.Tauren:
                                _raidMemberPreset.Race = Race.NightElf;
                                break;
                            case Race.Troll:
                                _raidMemberPreset.Race = Race.Gnome;
                                break;
                        }
                        break;
                    case ClassId.Hunter:
                        if (Race == Race.Orc)
                        {
                            _raidMemberPreset.Race = Race.Dwarf;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.NightElf;
                        }
                        break;
                    case ClassId.Rogue:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else if (Race == Race.Orc)
                        {
                            _raidMemberPreset.Race = Race.Dwarf;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.NightElf;
                        }
                        break;
                    case ClassId.Priest:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.NightElf;
                        }
                        break;
                    case ClassId.Paladin:
                        if (Race == Race.Orc)
                        {
                            _raidMemberPreset.Race = Race.Dwarf;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        break;
                    case ClassId.Mage:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Gnome;
                        }
                        break;
                    case ClassId.Warlock:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Gnome;
                        }
                        break;
                    case ClassId.Druid:
                        _raidMemberPreset.Race = Race.NightElf;
                        break;
                }
            }
            OnPropertyChanged(nameof(Races));
            OnPropertyChanged(nameof(ClassIds));

            OnPropertyChanged(nameof(Race));
            OnPropertyChanged(nameof(Class));
        }

        private bool _shouldRun;
        public bool ShouldRun
        {
            get => _shouldRun;
            set
            {
                _shouldRun = value;
                OnPropertyChanged(nameof(ShouldRun));
            }
        }
        private bool _isFocused;
        public bool IsFocused
        {
            get
            {
                return _isFocused;
            }
            set
            {
                _isFocused = value;

                OnPropertyChanged(nameof(IsFocused));
            }
        }

        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                _header = value;
                OnPropertyChanged(nameof(Header));
            }
        }
        public bool IsAlliance { get; set; }

        public Race Race
        {
            get => _raidMemberPreset.Race;
            set
            {
                _raidMemberPreset.Race = value;

                if (!ClassIds.Contains(Class))
                {
                    Class = ClassId.Warrior;
                }
                OnPropertyChanged(nameof(Race));
                OnPropertyChanged(nameof(ClassIds));
            }
        }

        public ClassId Class
        {
            get => _raidMemberPreset.Class;
            set
            {
                _raidMemberPreset.Class = value;
                OnPropertyChanged(nameof(Class));
            }
        }

        public int Level
        {
            get => _raidMemberPreset.Level;
            set
            {
                _raidMemberPreset.Level = value;
                OnPropertyChanged(nameof(Level));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
        private List<int> GetSpellIds(ClassId clazz, int talentTree, int talentIndex)
        {
            switch (clazz)
            {
                case ClassId.Druid:
                    return new List<int>();
                case ClassId.Hunter:
                    return new List<int>();
                case ClassId.Mage:
                    return new List<int>();
            }

            return new List<int>();
        }
    }
}
