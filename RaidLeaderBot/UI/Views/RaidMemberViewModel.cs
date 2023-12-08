using RaidLeaderBot.UI.Views.Talents;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using static RaidMemberBot.Constants.Enums;

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
        public IEnumerable<Class> ClassIds
        {
            get
            {
                return Race switch
                {
                    Race.Human => new List<Class>() { Class.Mage, Class.Paladin, Class.Priest, Class.Rogue, Class.Warlock, Class.Warrior },
                    Race.Dwarf => new List<Class>() { Class.Hunter, Class.Paladin, Class.Priest, Class.Rogue, Class.Warrior },
                    Race.NightElf => new List<Class>() { Class.Druid, Class.Hunter, Class.Priest, Class.Rogue, Class.Warrior },
                    Race.Gnome => new List<Class>() { Class.Mage, Class.Rogue, Class.Warlock },
                    Race.Orc => new List<Class>() { Class.Hunter, Class.Rogue, Class.Shaman, Class.Warlock, Class.Warrior },
                    Race.Undead => new List<Class>() { Class.Mage, Class.Priest, Class.Rogue, Class.Warlock, Class.Warrior },
                    Race.Tauren => new List<Class>() { Class.Druid, Class.Hunter, Class.Shaman, Class.Warrior },
                    Race.Troll => new List<Class>() { Class.Hunter, Class.Mage, Class.Priest, Class.Rogue, Class.Shaman, Class.Warrior },
                    _ => Enum.GetValues(typeof(Class)).Cast<Class>(),
                };
            }
        }
        public IEnumerable<Race> Races
        {
            get
            {
                return IsAlliance ? new List<Race>() { Race.Human, Race.Dwarf, Race.NightElf, Race.Gnome } : new List<Race>() { Race.Orc, Race.Undead, Race.Tauren, Race.Troll };
            }
        }
        private RaidMemberPreset _raidMemberPreset;
        public RaidMemberPreset RaidMemberPreset
        {
            get => _raidMemberPreset;
            set => _raidMemberPreset = value;
        }
        private RaidMemberTalentsViewModel _raidMemberTalentsViewModel;
        public RaidMemberTalentsViewModel RaidMemberTalentsViewModel
        {
            get => _raidMemberTalentsViewModel;
            set => _raidMemberTalentsViewModel = value;
        }

        public RaidMemberViewModel()
        {
            _raidMemberPreset = new RaidMemberPreset();
        }
        public RaidMemberViewModel(RaidMemberPreset raidMemberPreset)
        {
            _raidMemberPreset = raidMemberPreset;
            _accountName = AccountUsageRegistry.Instance.CheckoutNextAvaiableAccountName(Race, Class);

            Header = $"No client connected";

            OnPropertyChanged(nameof(Race));
            OnPropertyChanged(nameof(Class));
            OnPropertyChanged(nameof(RaidMemberPreset));

            SetTalentViewModel();
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
                    Class = Class.Warrior;
                }
                OnPropertyChanged(nameof(Race));
                OnPropertyChanged(nameof(ClassIds));
            }
        }
        public Class Class
        {
            get => _raidMemberPreset.Class;
            set
            {
                _raidMemberPreset.Class = value;
                SetTalentViewModel();
                OnPropertyChanged(nameof(Class));
            }
        }

        private void SetTalentViewModel()
        {
            switch (_raidMemberPreset.Class)
            {
                case Class.Druid:
                    RaidMemberTalentsViewModel = new DruidTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Hunter:
                    RaidMemberTalentsViewModel = new HunterTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Mage:
                    RaidMemberTalentsViewModel = new MageTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Paladin:
                    RaidMemberTalentsViewModel = new PaladinTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Priest:
                    RaidMemberTalentsViewModel = new PriestTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Rogue:
                    RaidMemberTalentsViewModel = new RogueTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Shaman:
                    RaidMemberTalentsViewModel = new ShamanTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Warlock:
                    RaidMemberTalentsViewModel = new WarlockTalentsViewModel(_raidMemberPreset);
                    break;
                case Class.Warrior:
                    RaidMemberTalentsViewModel = new WarriorTalentsViewModel(_raidMemberPreset);
                    break;
            }

            OnPropertyChanged(nameof(RaidMemberTalentsViewModel));
        }

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
                if (Class == Class.Paladin)
                {
                    _raidMemberPreset.Class = Class.Shaman;
                }

                switch (Class)
                {
                    case Class.Warrior:
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
                    case Class.Hunter:
                        if (Race == Race.Dwarf)
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Tauren;
                        }
                        break;
                    case Class.Rogue:
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
                    case Class.Priest:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case Class.Shaman:
                        if (Race == Race.Dwarf)
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case Class.Mage:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Troll;
                        }
                        break;
                    case Class.Warlock:
                        if (Race == Race.Human)
                        {
                            _raidMemberPreset.Race = Race.Undead;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Orc;
                        }
                        break;
                    case Class.Druid:
                        _raidMemberPreset.Race = Race.Tauren;
                        break;
                }
            }
            else
            {
                if (Class == Class.Shaman)
                {
                    _raidMemberPreset.Class = Class.Paladin;
                }
                switch (Class)
                {
                    case Class.Warrior:
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
                    case Class.Hunter:
                        if (Race == Race.Orc)
                        {
                            _raidMemberPreset.Race = Race.Dwarf;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.NightElf;
                        }
                        break;
                    case Class.Rogue:
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
                    case Class.Priest:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.NightElf;
                        }
                        break;
                    case Class.Paladin:
                        if (Race == Race.Orc)
                        {
                            _raidMemberPreset.Race = Race.Dwarf;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        break;
                    case Class.Mage:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Gnome;
                        }
                        break;
                    case Class.Warlock:
                        if (Race == Race.Undead)
                        {
                            _raidMemberPreset.Race = Race.Human;
                        }
                        else
                        {
                            _raidMemberPreset.Race = Race.Gnome;
                        }
                        break;
                    case Class.Druid:
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
        private string _accountName;
        public string AccountName => _accountName;
        public string BotProfileName
        {
            get
            {
                switch (Class)
                {
                    case Class.Warrior:
                        if (_raidMemberPreset.IsMainTank)
                        {
                            return "Protection Warrior";
                        }
                        else if (_raidMemberPreset.IsRole1)
                        {
                            return "Arms Warrior";
                        }
                        return "Fury Warrior";
                    case Class.Paladin:
                        if (_raidMemberPreset.IsMainTank)
                        {
                            return "Protection Paladin";
                        }
                        else if (_raidMemberPreset.IsMainHealer)
                        {
                            return "Holy Paladin";
                        }
                        return "Retribution Paladin";
                    case Class.Hunter:
                        if (_raidMemberPreset.IsRole1)
                        {
                            return "Beast Master Hunter";
                        }
                        else if (_raidMemberPreset.IsRole2)
                        {
                            return "Survival Hunter";
                        }
                        return "Marksmanship Hunter";
                    case Class.Rogue:
                        if (_raidMemberPreset.IsRole1)
                        {
                            return "Assassination Rogue";
                        }
                        else if (_raidMemberPreset.IsRole2)
                        {
                            return "Subtlety Rogue";
                        }
                        return "Combat Rogue";
                    case Class.Priest:
                        if (_raidMemberPreset.IsMainHealer)
                        {
                            return "Holy Priest";
                        }
                        else if (_raidMemberPreset.IsRole1)
                        {
                            return "Discipline Priest";
                        }
                        return "Shadow Priest";
                    case Class.Shaman:
                        if (_raidMemberPreset.IsMainHealer)
                        {
                            return "Restoration Shaman";
                        }
                        else if (_raidMemberPreset.IsRole1)
                        {
                            return "Elemental Shaman";
                        }
                        return "Enhancement Shaman";
                    case Class.Mage:
                        if (_raidMemberPreset.IsRole1)
                        {
                            return "Fire Mage";
                        }
                        else if (_raidMemberPreset.IsRole2)
                        {
                            return "Frost Mage";
                        }
                        return "Frost Mage";
                    case Class.Warlock:
                        if (_raidMemberPreset.IsRole1)
                        {
                            return "Destruction Warlock";
                        }
                        else if (_raidMemberPreset.IsRole2)
                        {
                            return "Demonology Warlock";
                        }
                        return "Affliction Warlock";
                    default:
                        if (_raidMemberPreset.IsMainTank)
                        {
                            return "Feral Combat Druid";
                        }
                        else if (_raidMemberPreset.IsMainHealer)
                        {
                            return "Restoration Druid";
                        }
                        return "Balance Druid";
                }
            }
        }
        public List<int> Spells => _raidMemberPreset.Spells;
        public List<int> Talents => _raidMemberPreset.Talents;
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

        public ICommand StartCommand => _startCommand ??= new CommandHandler(StartBot, true);
        public ICommand StopCommand => _stopCommand ??= new CommandHandler(StopBot, true);

        ICommand _startCommand;
        ICommand _stopCommand;

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
