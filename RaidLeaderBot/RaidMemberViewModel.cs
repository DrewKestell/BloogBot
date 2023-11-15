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
                    Race.Human => new List<Class>() { Class.Warrior, Class.Paladin, Class.Rogue, Class.Priest, Class.Mage, Class.Warlock },
                    Race.Dwarf => new List<Class>() { Class.Warrior, Class.Paladin, Class.Hunter, Class.Rogue, Class.Priest },
                    Race.NightElf => new List<Class>() { Class.Warrior, Class.Hunter, Class.Rogue, Class.Priest, Class.Druid },
                    Race.Gnome => new List<Class>() { Class.Warrior, Class.Rogue, Class.Mage, Class.Warlock },
                    Race.Orc => new List<Class>() { Class.Warrior, Class.Hunter, Class.Rogue, Class.Shaman, Class.Warlock },
                    Race.Undead => new List<Class>() { Class.Warrior, Class.Rogue, Class.Priest, Class.Mage, Class.Warlock },
                    Race.Tauren => new List<Class>() { Class.Warrior, Class.Hunter, Class.Shaman, Class.Druid },
                    Race.Troll => new List<Class>() { Class.Warrior, Class.Hunter, Class.Rogue, Class.Priest, Class.Shaman, Class.Mage },
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
            set
            {
                _raidMemberPreset = value;
            }
            get => _raidMemberPreset;
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

            UpdateTalentTrees();
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
                UpdateTalentTrees();

                OnPropertyChanged(nameof(Class));
            }
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
        private void UpdateTalentTrees()
        {
            switch (Class)
            {
                case Class.Warrior:
                    TalentTree1Header = "Arms";
                    TalentTree2Header = "Fury";
                    TalentTree3Header = "Protection";

                    // Improved Heroic Strike
                    Talent1Index1Row = 0;
                    Talent1Index1Column = 0;
                    // Deflection
                    Talent1Index2Row = 0;
                    Talent1Index2Column = 1;
                    // Improved Rend
                    Talent1Index3Row = 0;
                    Talent1Index3Column = 2;

                    // Improved Charge
                    Talent1Index4Row = 2;
                    Talent1Index4Column = 0;
                    // Tactical Mastery
                    Talent1Index5Row = 2;
                    Talent1Index5Column = 1;
                    // Improved Thunder Clap
                    Talent1Index6Row = 2;
                    Talent1Index6Column = 3;

                    // Improved Overpower
                    Talent1Index7Row = 4;
                    Talent1Index7Column = 0;
                    // Anger Management
                    Talent1Index8Row = 4;
                    Talent1Index8Column = 1;
                    // Deep Woods
                    Talent1Index9Row = 4;
                    Talent1Index9Column = 2;

                    // Two-Handed Weapon Specialization
                    Talent1Index10Row = 6;
                    Talent1Index10Column = 1;
                    // Impale
                    Talent1Index11Row = 6;
                    Talent1Index11Column = 2;

                    // Axe Specialization
                    Talent1Index12Row = 8;
                    Talent1Index12Column = 0;
                    // Sweeping Strikes
                    Talent1Index13Row = 8;
                    Talent1Index13Column = 1;
                    // Mace Specilization
                    Talent1Index14Row = 8;
                    Talent1Index14Column = 2;
                    // Sword Specialization
                    Talent1Index15Row = 8;
                    Talent1Index15Column = 3;

                    // Polearm Specialization
                    Talent1Index16Visibility = Visibility.Visible;
                    Talent1Index16Row = 10;
                    Talent1Index16Column = 0;
                    // Improved Hamstring
                    Talent1Index17Visibility = Visibility.Visible;
                    Talent1Index17Row = 10;
                    Talent1Index17Column = 2;

                    // Mortal Strike
                    Talent1Index18Visibility = Visibility.Visible;
                    Talent1Index18Row = 12;
                    Talent1Index18Column = 1;


                    // Booming Voice
                    Talent2Index1Row = 0;
                    Talent2Index1Column = 1;
                    // Cruelty
                    Talent2Index2Row = 0;
                    Talent2Index2Column = 2;

                    // Improved Demoralizing Shout
                    Talent2Index3Row = 2;
                    Talent2Index3Column = 1;
                    // Unbridled Wrath
                    Talent2Index4Row = 2;
                    Talent2Index4Column = 2;

                    // Improved Cleave
                    Talent2Index5Row = 4;
                    Talent2Index5Column = 0;
                    // Piercing Howl
                    Talent2Index6Row = 4;
                    Talent2Index6Column = 1;
                    // Blood Craze
                    Talent2Index7Row = 4;
                    Talent2Index7Column = 2;
                    // Improved Battle Shout
                    Talent2Index8Row = 4;
                    Talent2Index8Column = 3;

                    // Duel Wield Specialization
                    Talent2Index9Row = 6;
                    Talent2Index9Column = 0;
                    // Improved Execute
                    Talent2Index10Row = 6;
                    Talent2Index10Column = 1;
                    // Enrage
                    Talent2Index11Row = 6;
                    Talent2Index11Column = 2;

                    // Improved Slam
                    Talent2Index12Row = 8;
                    Talent2Index12Column = 0;
                    // Death Wish
                    Talent2Index13Row = 8;
                    Talent2Index13Column = 1;
                    // Improved Intercept
                    Talent2Index14Row = 8;
                    Talent2Index14Column = 3;

                    // Improved Berserker Rage
                    Talent2Index15Row = 10;
                    Talent2Index15Column = 0;
                    // Flurry
                    Talent2Index16Visibility = Visibility.Visible;
                    Talent2Index16Row = 10;
                    Talent2Index16Column = 2;

                    // Bloodthirst
                    Talent2Index17Visibility = Visibility.Visible;
                    Talent2Index17Row = 12;
                    Talent2Index17Column = 1;

                    // 
                    Talent2Index18Visibility = Visibility.Hidden;
                    Talent2Index19Visibility = Visibility.Hidden;


                    // Shield Specialization
                    Talent3Index1Row = 0;
                    Talent3Index1Column = 1;
                    // Anticipation
                    Talent3Index2Row = 0;
                    Talent3Index2Column = 2;

                    // Improved Bloodrage
                    Talent3Index3Row = 2;
                    Talent3Index3Column = 0;
                    // Toughness
                    Talent3Index4Row = 2;
                    Talent3Index4Column = 2;
                    // Iron Will
                    Talent3Index5Row = 2;
                    Talent3Index5Column = 3;

                    // Last Stand
                    Talent3Index6Row = 4;
                    Talent3Index6Column = 0;
                    // Improved Shield Block
                    Talent3Index7Row = 4;
                    Talent3Index7Column = 1;
                    // Improved Revenge
                    Talent3Index8Row = 4;
                    Talent3Index8Column = 2;
                    // Defiance
                    Talent3Index9Row = 4;
                    Talent3Index9Column = 3;

                    // Improved Sunder Armor
                    Talent3Index10Row = 6;
                    Talent3Index10Column = 0;
                    // Improved Disarm
                    Talent3Index11Row = 6;
                    Talent3Index11Column = 1;
                    // Improved Taunt
                    Talent3Index12Row = 6;
                    Talent3Index12Column = 2;

                    // Improved Shield Wall
                    Talent3Index13Row = 8;
                    Talent3Index13Column = 0;
                    // Concussion Blow
                    Talent3Index14Row = 8;
                    Talent3Index14Column = 1;
                    // Improved Shield Bash
                    Talent3Index15Row = 8;
                    Talent3Index15Column = 2;

                    // One-Handed Specialization
                    Talent3Index16Visibility = Visibility.Visible;
                    Talent3Index16Row = 10;
                    Talent3Index16Column = 2;

                    // Shield Slam
                    Talent3Index17Visibility = Visibility.Visible;
                    Talent3Index17Row = 12;
                    Talent3Index17Column = 1;

                    // 
                    Talent3Index18Visibility = Visibility.Hidden;
                    break;
                case Class.Hunter:
                    TalentTree1Header = "Beast Mastery";
                    TalentTree2Header = "Marksmanship";
                    TalentTree3Header = "Survival";
                    break;
                case Class.Rogue:
                    TalentTree1Header = "Assassination";
                    TalentTree2Header = "Combat";
                    TalentTree3Header = "Subtlety";
                    break;
                case Class.Priest:
                    TalentTree1Header = "Discipline";
                    TalentTree2Header = "Holy";
                    TalentTree3Header = "Shadow";
                    break;
                case Class.Paladin:
                    TalentTree1Header = "Holy";
                    TalentTree2Header = "Protection";
                    TalentTree3Header = "Retribution";
                    break;
                case Class.Shaman:
                    TalentTree1Header = "Elemental";
                    TalentTree2Header = "Enhancement";
                    TalentTree3Header = "Restoration";
                    break;
                case Class.Mage:
                    TalentTree1Header = "Arcane";
                    TalentTree2Header = "Fire";
                    TalentTree3Header = "Frost";
                    break;
                case Class.Warlock:
                    TalentTree1Header = "Affliction";
                    TalentTree2Header = "Demonology";
                    TalentTree3Header = "Destruction";
                    break;
                case Class.Druid:
                    TalentTree1Header = "Balance";
                    TalentTree2Header = "Feral Combat";
                    TalentTree3Header = "Restoration";
                    break;
            }
        }
        public int Talent1SpentPoints { get; set; }
        public int Talent2SpentPoints { get; set; }
        public int Talent3SpentPoints { get; set; }
        private List<int> _talent1Index1Spells;
        public List<int> Talent1Index1Spells
        {
            get
            {
                return _talent1Index1Spells;
            }
            set
            {
                _talent1Index1Spells = value;
                OnPropertyChanged(nameof(Talent1Index1Spells));
            }
        }
        public List<int> AllSetTalentSpells { get { return _raidMemberPreset.Talents; } }
        public int TotalTalentPoints => Math.Max(0, RaidMemberPreset.Level - 9);

        private string _talentTree1Header;
        private string _talentTree2Header;
        private string _talentTree3Header;
        private string _talentTree1TooltipHeader;
        private string _talentTree2TooltipHeader;
        private string _talentTree3TooltipHeader;
        private string _talentTree1TooltipSubtext;
        private string _talentTree2TooltipSubtext;
        private string _talentTree3TooltipSubtext;
        public Visibility Talent1Index16Visibility { get; set; }
        public Visibility Talent1Index17Visibility { get; set; }
        public Visibility Talent1Index18Visibility { get; set; }
        public Visibility Talent2Index15Visibility { get; set; }
        public Visibility Talent2Index16Visibility { get; set; }
        public Visibility Talent2Index17Visibility { get; set; }
        public Visibility Talent2Index18Visibility { get; set; }
        public Visibility Talent2Index19Visibility { get; set; }
        public Visibility Talent3Index15Visibility { get; set; }
        public Visibility Talent3Index16Visibility { get; set; }
        public Visibility Talent3Index17Visibility { get; set; }
        public Visibility Talent3Index18Visibility { get; set; }
        private bool _talent1Index1Enabled;
        private bool _talent1Index2Enabled;
        private bool _talent1Index3Enabled;
        private bool _talent1Index4Enabled;
        private bool _talent1Index5Enabled;
        private bool _talent1Index6Enabled;
        private bool _talent1Index7Enabled;
        private bool _talent1Index8Enabled;
        private bool _talent1Index9Enabled;
        private bool _talent1Index10Enabled;
        private bool _talent1Index11Enabled;
        private bool _talent1Index12Enabled;
        private bool _talent1Index13Enabled;
        private bool _talent1Index14Enabled;
        private bool _talent1Index15Enabled;
        private bool _talent1Index16Enabled;
        private bool _talent1Index17Enabled;
        private bool _talent1Index18Enabled;
        private bool _talent2Index1Enabled;
        private bool _talent2Index2Enabled;
        private bool _talent2Index3Enabled;
        private bool _talent2Index4Enabled;
        private bool _talent2Index5Enabled;
        private bool _talent2Index6Enabled;
        private bool _talent2Index7Enabled;
        private bool _talent2Index8Enabled;
        private bool _talent2Index9Enabled;
        private bool _talent2Index10Enabled;
        private bool _talent2Index11Enabled;
        private bool _talent2Index12Enabled;
        private bool _talent2Index13Enabled;
        private bool _talent2Index14Enabled;
        private bool _talent2Index15Enabled;
        private bool _talent2Index16Enabled;
        private bool _talent2Index17Enabled;
        private bool _talent2Index18Enabled;
        private bool _talent2Index19Enabled;
        private bool _talent3Index1Enabled;
        private bool _talent3Index2Enabled;
        private bool _talent3Index3Enabled;
        private bool _talent3Index4Enabled;
        private bool _talent3Index5Enabled;
        private bool _talent3Index6Enabled;
        private bool _talent3Index7Enabled;
        private bool _talent3Index8Enabled;
        private bool _talent3Index9Enabled;
        private bool _talent3Index10Enabled;
        private bool _talent3Index11Enabled;
        private bool _talent3Index12Enabled;
        private bool _talent3Index13Enabled;
        private bool _talent3Index14Enabled;
        private bool _talent3Index15Enabled;
        private bool _talent3Index16Enabled;
        private bool _talent3Index17Enabled;
        private bool _talent3Index18Enabled;
        private int _talent1Index1Column;
        private int _talent1Index2Column;
        private int _talent1Index3Column;
        private int _talent1Index4Column;
        private int _talent1Index5Column;
        private int _talent1Index6Column;
        private int _talent1Index7Column;
        private int _talent1Index8Column;
        private int _talent1Index9Column;
        private int _talent1Index10Column;
        private int _talent1Index11Column;
        private int _talent1Index12Column;
        private int _talent1Index13Column;
        private int _talent1Index14Column;
        private int _talent1Index15Column;
        private int _talent1Index16Column;
        private int _talent1Index17Column;
        private int _talent1Index18Column;
        private int _talent1Index1Row;
        private int _talent1Index2Row;
        private int _talent1Index3Row;
        private int _talent1Index4Row;
        private int _talent1Index5Row;
        private int _talent1Index6Row;
        private int _talent1Index7Row;
        private int _talent1Index8Row;
        private int _talent1Index9Row;
        private int _talent1Index10Row;
        private int _talent1Index11Row;
        private int _talent1Index12Row;
        private int _talent1Index13Row;
        private int _talent1Index14Row;
        private int _talent1Index15Row;
        private int _talent1Index16Row;
        private int _talent1Index17Row;
        private int _talent1Index18Row;
        public string TalentTree1Header { get => _talentTree1Header; set { _talentTree1Header = value; OnPropertyChanged(nameof(TalentTree1Header)); } }
        public string TalentTree2Header { get => _talentTree2Header; set { _talentTree2Header = value; OnPropertyChanged(nameof(TalentTree2Header)); } }
        public string TalentTree3Header { get => _talentTree3Header; set { _talentTree3Header = value; OnPropertyChanged(nameof(TalentTree3Header)); } }
        public string TalentTree1TooltipHeader { get => _talentTree1TooltipHeader; set { _talentTree1TooltipHeader = value; OnPropertyChanged(nameof(TalentTree1TooltipHeader)); } }
        public string TalentTree2TooltipHeader { get => _talentTree2TooltipHeader; set { _talentTree2TooltipHeader = value; OnPropertyChanged(nameof(TalentTree2TooltipHeader)); } }
        public string TalentTree3TooltipHeader { get => _talentTree3TooltipHeader; set { _talentTree3TooltipHeader = value; OnPropertyChanged(nameof(TalentTree3TooltipHeader)); } }
        public string TalentTree1TooltipSubtext { get => _talentTree1TooltipSubtext; set { _talentTree1TooltipSubtext = value; OnPropertyChanged(nameof(TalentTree1TooltipSubtext)); } }
        public string TalentTree2TooltipSubtext { get => _talentTree2TooltipSubtext; set { _talentTree2TooltipSubtext = value; OnPropertyChanged(nameof(TalentTree2TooltipSubtext)); } }
        public string TalentTree3TooltipSubtext { get => _talentTree3TooltipSubtext; set { _talentTree3TooltipSubtext = value; OnPropertyChanged(nameof(TalentTree3TooltipSubtext)); } }
        public int Talent1Index1Column { get => _talent1Index1Column; set { _talent1Index1Column = value; OnPropertyChanged(nameof(Talent1Index1Column)); } }
        public int Talent1Index2Column { get => _talent1Index2Column; set { _talent1Index2Column = value; OnPropertyChanged(nameof(Talent1Index2Column)); } }
        public int Talent1Index3Column { get => _talent1Index3Column; set { _talent1Index3Column = value; OnPropertyChanged(nameof(Talent1Index3Column)); } }
        public int Talent1Index4Column { get => _talent1Index4Column; set { _talent1Index4Column = value; OnPropertyChanged(nameof(Talent1Index4Column)); } }
        public int Talent1Index5Column { get => _talent1Index5Column; set { _talent1Index5Column = value; OnPropertyChanged(nameof(Talent1Index5Column)); } }
        public int Talent1Index6Column { get => _talent1Index6Column; set { _talent1Index6Column = value; OnPropertyChanged(nameof(Talent1Index6Column)); } }
        public int Talent1Index7Column { get => _talent1Index7Column; set { _talent1Index7Column = value; OnPropertyChanged(nameof(Talent1Index7Column)); } }
        public int Talent1Index8Column { get => _talent1Index8Column; set { _talent1Index8Column = value; OnPropertyChanged(nameof(Talent1Index8Column)); } }
        public int Talent1Index9Column { get => _talent1Index9Column; set { _talent1Index9Column = value; OnPropertyChanged(nameof(Talent1Index9Column)); } }
        public int Talent1Index10Column { get => _talent1Index10Column; set { _talent1Index10Column = value; OnPropertyChanged(nameof(Talent1Index10Column)); } }
        public int Talent1Index11Column { get => _talent1Index11Column; set { _talent1Index11Column = value; OnPropertyChanged(nameof(Talent1Index11Column)); } }
        public int Talent1Index12Column { get => _talent1Index12Column; set { _talent1Index12Column = value; OnPropertyChanged(nameof(Talent1Index12Column)); } }
        public int Talent1Index13Column { get => _talent1Index13Column; set { _talent1Index13Column = value; OnPropertyChanged(nameof(Talent1Index13Column)); } }
        public int Talent1Index14Column { get => _talent1Index14Column; set { _talent1Index14Column = value; OnPropertyChanged(nameof(Talent1Index14Column)); } }
        public int Talent1Index15Column { get => _talent1Index15Column; set { _talent1Index15Column = value; OnPropertyChanged(nameof(Talent1Index15Column)); } }
        public int Talent1Index16Column { get => _talent1Index16Column; set { _talent1Index16Column = value; OnPropertyChanged(nameof(Talent1Index16Column)); } }
        public int Talent1Index17Column { get => _talent1Index17Column; set { _talent1Index17Column = value; OnPropertyChanged(nameof(Talent1Index17Column)); } }
        public int Talent1Index18Column { get => _talent1Index18Column; set { _talent1Index18Column = value; OnPropertyChanged(nameof(Talent1Index18Column)); } }
        public bool Talent1Index1Enabled { get => _talent1Index1Enabled; set { _talent1Index1Enabled = value; OnPropertyChanged(nameof(Talent1Index1Enabled)); } }
        public bool Talent1Index2Enabled { get => _talent1Index2Enabled; set { _talent1Index2Enabled = value; OnPropertyChanged(nameof(Talent1Index2Enabled)); } }
        public bool Talent1Index3Enabled { get => _talent1Index3Enabled; set { _talent1Index3Enabled = value; OnPropertyChanged(nameof(Talent1Index3Enabled)); } }
        public bool Talent1Index4Enabled { get => _talent1Index4Enabled; set { _talent1Index4Enabled = value; OnPropertyChanged(nameof(Talent1Index4Enabled)); } }
        public bool Talent1Index5Enabled { get => _talent1Index5Enabled; set { _talent1Index5Enabled = value; OnPropertyChanged(nameof(Talent1Index5Enabled)); } }
        public bool Talent1Index6Enabled { get => _talent1Index6Enabled; set { _talent1Index6Enabled = value; OnPropertyChanged(nameof(Talent1Index6Enabled)); } }
        public bool Talent1Index7Enabled { get => _talent1Index7Enabled; set { _talent1Index7Enabled = value; OnPropertyChanged(nameof(Talent1Index7Enabled)); } }
        public bool Talent1Index8Enabled { get => _talent1Index8Enabled; set { _talent1Index8Enabled = value; OnPropertyChanged(nameof(Talent1Index8Enabled)); } }
        public bool Talent1Index9Enabled { get => _talent1Index9Enabled; set { _talent1Index9Enabled = value; OnPropertyChanged(nameof(Talent1Index9Enabled)); } }
        public bool Talent1Index10Enabled { get => _talent1Index10Enabled; set { _talent1Index10Enabled = value; OnPropertyChanged(nameof(Talent1Index10Enabled)); } }
        public bool Talent1Index11Enabled { get => _talent1Index11Enabled; set { _talent1Index11Enabled = value; OnPropertyChanged(nameof(Talent1Index11Enabled)); } }
        public bool Talent1Index12Enabled { get => _talent1Index12Enabled; set { _talent1Index12Enabled = value; OnPropertyChanged(nameof(Talent1Index12Enabled)); } }
        public bool Talent1Index13Enabled { get => _talent1Index13Enabled; set { _talent1Index13Enabled = value; OnPropertyChanged(nameof(Talent1Index13Enabled)); } }
        public bool Talent1Index14Enabled { get => _talent1Index14Enabled; set { _talent1Index14Enabled = value; OnPropertyChanged(nameof(Talent1Index14Enabled)); } }
        public bool Talent1Index15Enabled { get => _talent1Index15Enabled; set { _talent1Index15Enabled = value; OnPropertyChanged(nameof(Talent1Index15Enabled)); } }
        public bool Talent1Index16Enabled { get => _talent1Index16Enabled; set { _talent1Index16Enabled = value; OnPropertyChanged(nameof(Talent1Index16Enabled)); } }
        public bool Talent1Index17Enabled { get => _talent1Index17Enabled; set { _talent1Index17Enabled = value; OnPropertyChanged(nameof(Talent1Index17Enabled)); } }
        public bool Talent1Index18Enabled { get => _talent1Index18Enabled; set { _talent1Index18Enabled = value; OnPropertyChanged(nameof(Talent1Index18Enabled)); } }
        public int Talent1Index1Row { get => _talent1Index1Row; set { _talent1Index1Row = value; OnPropertyChanged(nameof(Talent1Index1Row)); } }
        public int Talent1Index2Row { get => _talent1Index2Row; set { _talent1Index2Row = value; OnPropertyChanged(nameof(Talent1Index2Row)); } }
        public int Talent1Index3Row { get => _talent1Index3Row; set { _talent1Index3Row = value; OnPropertyChanged(nameof(Talent1Index3Row)); } }
        public int Talent1Index4Row { get => _talent1Index4Row; set { _talent1Index4Row = value; OnPropertyChanged(nameof(Talent1Index4Row)); } }
        public int Talent1Index5Row { get => _talent1Index5Row; set { _talent1Index5Row = value; OnPropertyChanged(nameof(Talent1Index5Row)); } }
        public int Talent1Index6Row { get => _talent1Index6Row; set { _talent1Index6Row = value; OnPropertyChanged(nameof(Talent1Index6Row)); } }
        public int Talent1Index7Row { get => _talent1Index7Row; set { _talent1Index7Row = value; OnPropertyChanged(nameof(Talent1Index7Row)); } }
        public int Talent1Index8Row { get => _talent1Index8Row; set { _talent1Index8Row = value; OnPropertyChanged(nameof(Talent1Index8Row)); } }
        public int Talent1Index9Row { get => _talent1Index9Row; set { _talent1Index9Row = value; OnPropertyChanged(nameof(Talent1Index9Row)); } }
        public int Talent1Index10Row { get => _talent1Index10Row; set { _talent1Index10Row = value; OnPropertyChanged(nameof(Talent1Index10Row)); } }
        public int Talent1Index11Row { get => _talent1Index11Row; set { _talent1Index11Row = value; OnPropertyChanged(nameof(Talent1Index11Row)); } }
        public int Talent1Index12Row { get => _talent1Index12Row; set { _talent1Index12Row = value; OnPropertyChanged(nameof(Talent1Index12Row)); } }
        public int Talent1Index13Row { get => _talent1Index13Row; set { _talent1Index13Row = value; OnPropertyChanged(nameof(Talent1Index13Row)); } }
        public int Talent1Index14Row { get => _talent1Index14Row; set { _talent1Index14Row = value; OnPropertyChanged(nameof(Talent1Index14Row)); } }
        public int Talent1Index15Row { get => _talent1Index15Row; set { _talent1Index15Row = value; OnPropertyChanged(nameof(Talent1Index15Row)); } }
        public int Talent1Index16Row { get => _talent1Index16Row; set { _talent1Index16Row = value; OnPropertyChanged(nameof(Talent1Index16Row)); } }
        public int Talent1Index17Row { get => _talent1Index17Row; set { _talent1Index17Row = value; OnPropertyChanged(nameof(Talent1Index17Row)); } }
        public int Talent1Index18Row { get => _talent1Index18Row; set { _talent1Index18Row = value; OnPropertyChanged(nameof(Talent1Index18Row)); } }
        public bool Talent2Index1Enabled { get => _talent2Index1Enabled; set { _talent2Index1Enabled = value; OnPropertyChanged(nameof(Talent2Index1Enabled)); } }
        public bool Talent2Index2Enabled { get => _talent2Index2Enabled; set { _talent2Index2Enabled = value; OnPropertyChanged(nameof(Talent2Index2Enabled)); } }
        public bool Talent2Index3Enabled { get => _talent2Index3Enabled; set { _talent2Index3Enabled = value; OnPropertyChanged(nameof(Talent2Index3Enabled)); } }
        public bool Talent2Index4Enabled { get => _talent2Index4Enabled; set { _talent2Index4Enabled = value; OnPropertyChanged(nameof(Talent2Index4Enabled)); } }
        public bool Talent2Index5Enabled { get => _talent2Index5Enabled; set { _talent2Index5Enabled = value; OnPropertyChanged(nameof(Talent2Index5Enabled)); } }
        public bool Talent2Index6Enabled { get => _talent2Index6Enabled; set { _talent2Index6Enabled = value; OnPropertyChanged(nameof(Talent2Index6Enabled)); } }
        public bool Talent2Index7Enabled { get => _talent2Index7Enabled; set { _talent2Index7Enabled = value; OnPropertyChanged(nameof(Talent2Index7Enabled)); } }
        public bool Talent2Index8Enabled { get => _talent2Index8Enabled; set { _talent2Index8Enabled = value; OnPropertyChanged(nameof(Talent2Index8Enabled)); } }
        public bool Talent2Index9Enabled { get => _talent2Index9Enabled; set { _talent2Index9Enabled = value; OnPropertyChanged(nameof(Talent2Index9Enabled)); } }
        public bool Talent2Index10Enabled { get => _talent2Index10Enabled; set { _talent2Index10Enabled = value; OnPropertyChanged(nameof(Talent2Index10Enabled)); } }
        public bool Talent2Index11Enabled { get => _talent2Index11Enabled; set { _talent2Index11Enabled = value; OnPropertyChanged(nameof(Talent2Index11Enabled)); } }
        public bool Talent2Index12Enabled { get => _talent2Index12Enabled; set { _talent2Index12Enabled = value; OnPropertyChanged(nameof(Talent2Index12Enabled)); } }
        public bool Talent2Index13Enabled { get => _talent2Index13Enabled; set { _talent2Index13Enabled = value; OnPropertyChanged(nameof(Talent2Index13Enabled)); } }
        public bool Talent2Index14Enabled { get => _talent2Index14Enabled; set { _talent2Index14Enabled = value; OnPropertyChanged(nameof(Talent2Index14Enabled)); } }
        public bool Talent2Index15Enabled { get => _talent2Index15Enabled; set { _talent2Index15Enabled = value; OnPropertyChanged(nameof(Talent2Index15Enabled)); } }
        public bool Talent2Index16Enabled { get => _talent2Index16Enabled; set { _talent2Index16Enabled = value; OnPropertyChanged(nameof(Talent2Index16Enabled)); } }
        public bool Talent2Index17Enabled { get => _talent2Index17Enabled; set { _talent2Index17Enabled = value; OnPropertyChanged(nameof(Talent2Index17Enabled)); } }
        public bool Talent2Index18Enabled { get => _talent2Index18Enabled; set { _talent2Index18Enabled = value; OnPropertyChanged(nameof(Talent2Index18Enabled)); } }
        public bool Talent2Index19Enabled { get => _talent2Index19Enabled; set { _talent2Index19Enabled = value; OnPropertyChanged(nameof(Talent2Index19Enabled)); } }

        private int _talent2Index1Column;
        public int Talent2Index1Column { get => _talent2Index1Column; set { _talent2Index1Column = value; OnPropertyChanged(nameof(Talent2Index1Column)); } }
        private int _talent2Index2Column;
        public int Talent2Index2Column { get => _talent2Index2Column; set { _talent2Index2Column = value; OnPropertyChanged(nameof(Talent2Index2Column)); } }
        private int _talent2Index3Column;
        public int Talent2Index3Column { get => _talent2Index3Column; set { _talent2Index3Column = value; OnPropertyChanged(nameof(Talent2Index3Column)); } }
        private int _talent2Index4Column;
        public int Talent2Index4Column { get => _talent2Index4Column; set { _talent2Index4Column = value; OnPropertyChanged(nameof(Talent2Index4Column)); } }
        private int _talent2Index5Column;
        public int Talent2Index5Column { get => _talent2Index5Column; set { _talent2Index5Column = value; OnPropertyChanged(nameof(Talent2Index5Column)); } }
        private int _talent2Index6Column;
        public int Talent2Index6Column { get => _talent2Index6Column; set { _talent2Index6Column = value; OnPropertyChanged(nameof(Talent2Index6Column)); } }
        private int _talent2Index7Column;
        public int Talent2Index7Column { get => _talent2Index7Column; set { _talent2Index7Column = value; OnPropertyChanged(nameof(Talent2Index7Column)); } }
        private int _talent2Index8Column;
        public int Talent2Index8Column { get => _talent2Index8Column; set { _talent2Index8Column = value; OnPropertyChanged(nameof(Talent2Index8Column)); } }
        private int _talent2Index9Column;
        public int Talent2Index9Column { get => _talent2Index9Column; set { _talent2Index9Column = value; OnPropertyChanged(nameof(Talent2Index9Column)); } }
        private int _talent2Index10Column;
        public int Talent2Index10Column { get => _talent2Index10Column; set { _talent2Index10Column = value; OnPropertyChanged(nameof(Talent2Index10Column)); } }
        private int _talent2Index11Column;
        public int Talent2Index11Column { get => _talent2Index11Column; set { _talent2Index11Column = value; OnPropertyChanged(nameof(Talent2Index11Column)); } }
        private int _talent2Index12Column;
        public int Talent2Index12Column { get => _talent2Index12Column; set { _talent2Index12Column = value; OnPropertyChanged(nameof(Talent2Index12Column)); } }
        private int _talent2Index13Column;
        public int Talent2Index13Column { get => _talent2Index13Column; set { _talent2Index13Column = value; OnPropertyChanged(nameof(Talent2Index13Column)); } }
        private int _talent2Index14Column;
        public int Talent2Index14Column { get => _talent2Index14Column; set { _talent2Index14Column = value; OnPropertyChanged(nameof(Talent2Index14Column)); } }
        private int _talent2Index15Column;
        public int Talent2Index15Column { get => _talent2Index15Column; set { _talent2Index15Column = value; OnPropertyChanged(nameof(Talent2Index15Column)); } }
        private int _talent2Index16Column;
        public int Talent2Index16Column { get => _talent2Index16Column; set { _talent2Index16Column = value; OnPropertyChanged(nameof(Talent2Index16Column)); } }
        private int _talent2Index17Column;
        public int Talent2Index17Column { get => _talent2Index17Column; set { _talent2Index17Column = value; OnPropertyChanged(nameof(Talent2Index17Column)); } }
        private int _talent2Index18Column;
        public int Talent2Index18Column { get => _talent2Index18Column; set { _talent2Index18Column = value; OnPropertyChanged(nameof(Talent2Index18Column)); } }
        private int _talent2Index19Column;
        public int Talent2Index19Column { get => _talent2Index19Column; set { _talent2Index19Column = value; OnPropertyChanged(nameof(Talent2Index19Column)); } }
        private int _talent2Index1Row;
        public int Talent2Index1Row { get => _talent2Index1Row; set { _talent2Index1Row = value; OnPropertyChanged(nameof(Talent2Index1Row)); } }
        private int _talent2Index2Row;
        public int Talent2Index2Row { get => _talent2Index2Row; set { _talent2Index2Row = value; OnPropertyChanged(nameof(Talent2Index2Row)); } }
        private int _talent2Index3Row;
        public int Talent2Index3Row { get => _talent2Index3Row; set { _talent2Index3Row = value; OnPropertyChanged(nameof(Talent2Index3Row)); } }
        private int _talent2Index4Row;
        public int Talent2Index4Row { get => _talent2Index4Row; set { _talent2Index4Row = value; OnPropertyChanged(nameof(Talent2Index4Row)); } }
        private int _talent2Index5Row;
        public int Talent2Index5Row { get => _talent2Index5Row; set { _talent2Index5Row = value; OnPropertyChanged(nameof(Talent2Index5Row)); } }
        private int _talent2Index6Row;
        public int Talent2Index6Row { get => _talent2Index6Row; set { _talent2Index6Row = value; OnPropertyChanged(nameof(Talent2Index6Row)); } }
        private int _talent2Index7Row;
        public int Talent2Index7Row { get => _talent2Index7Row; set { _talent2Index7Row = value; OnPropertyChanged(nameof(Talent2Index7Row)); } }
        private int _talent2Index8Row;
        public int Talent2Index8Row { get => _talent2Index8Row; set { _talent2Index8Row = value; OnPropertyChanged(nameof(Talent2Index8Row)); } }
        private int _talent2Index9Row;
        public int Talent2Index9Row { get => _talent2Index9Row; set { _talent2Index9Row = value; OnPropertyChanged(nameof(Talent2Index9Row)); } }
        private int _talent2Index10Row;
        public int Talent2Index10Row { get => _talent2Index10Row; set { _talent2Index10Row = value; OnPropertyChanged(nameof(Talent2Index10Row)); } }
        private int _talent2Index11Row;
        public int Talent2Index11Row { get => _talent2Index11Row; set { _talent2Index11Row = value; OnPropertyChanged(nameof(Talent2Index11Row)); } }
        private int _talent2Index12Row;
        public int Talent2Index12Row { get => _talent2Index12Row; set { _talent2Index12Row = value; OnPropertyChanged(nameof(Talent2Index12Row)); } }
        private int _talent2Index13Row;
        public int Talent2Index13Row { get => _talent2Index13Row; set { _talent2Index13Row = value; OnPropertyChanged(nameof(Talent2Index13Row)); } }
        private int _talent2Index14Row;
        public int Talent2Index14Row { get => _talent2Index14Row; set { _talent2Index14Row = value; OnPropertyChanged(nameof(Talent2Index14Row)); } }
        private int _talent2Index15Row;
        public int Talent2Index15Row { get => _talent2Index15Row; set { _talent2Index15Row = value; OnPropertyChanged(nameof(Talent2Index15Row)); } }
        private int _talent2Index16Row;
        public int Talent2Index16Row { get => _talent2Index16Row; set { _talent2Index16Row = value; OnPropertyChanged(nameof(Talent2Index16Row)); } }
        private int _talent2Index17Row;
        public int Talent2Index17Row { get => _talent2Index17Row; set { _talent2Index17Row = value; OnPropertyChanged(nameof(Talent2Index17Row)); } }
        private int _talent2Index18Row;
        public int Talent2Index18Row { get => _talent2Index18Row; set { _talent2Index18Row = value; OnPropertyChanged(nameof(Talent2Index18Row)); } }
        private int _talent2Index19Row;
        public int Talent2Index19Row { get => _talent2Index19Row; set { _talent2Index19Row = value; OnPropertyChanged(nameof(Talent2Index19Row)); } }
        public bool Talent3Index1Enabled { get => _talent3Index1Enabled; set { _talent3Index1Enabled = value; OnPropertyChanged(nameof(Talent3Index1Enabled)); } }
        public bool Talent3Index2Enabled { get => _talent3Index2Enabled; set { _talent3Index2Enabled = value; OnPropertyChanged(nameof(Talent3Index2Enabled)); } }
        public bool Talent3Index3Enabled { get => _talent3Index3Enabled; set { _talent3Index3Enabled = value; OnPropertyChanged(nameof(Talent3Index3Enabled)); } }
        public bool Talent3Index4Enabled { get => _talent3Index4Enabled; set { _talent3Index4Enabled = value; OnPropertyChanged(nameof(Talent3Index4Enabled)); } }
        public bool Talent3Index5Enabled { get => _talent3Index5Enabled; set { _talent3Index5Enabled = value; OnPropertyChanged(nameof(Talent3Index5Enabled)); } }
        public bool Talent3Index6Enabled { get => _talent3Index6Enabled; set { _talent3Index6Enabled = value; OnPropertyChanged(nameof(Talent3Index6Enabled)); } }
        public bool Talent3Index7Enabled { get => _talent3Index7Enabled; set { _talent3Index7Enabled = value; OnPropertyChanged(nameof(Talent3Index7Enabled)); } }
        public bool Talent3Index8Enabled { get => _talent3Index8Enabled; set { _talent3Index8Enabled = value; OnPropertyChanged(nameof(Talent3Index8Enabled)); } }
        public bool Talent3Index9Enabled { get => _talent3Index9Enabled; set { _talent3Index9Enabled = value; OnPropertyChanged(nameof(Talent3Index9Enabled)); } }
        public bool Talent3Index10Enabled { get => _talent3Index10Enabled; set { _talent3Index10Enabled = value; OnPropertyChanged(nameof(Talent3Index10Enabled)); } }
        public bool Talent3Index11Enabled { get => _talent3Index11Enabled; set { _talent3Index11Enabled = value; OnPropertyChanged(nameof(Talent3Index11Enabled)); } }
        public bool Talent3Index12Enabled { get => _talent3Index12Enabled; set { _talent3Index12Enabled = value; OnPropertyChanged(nameof(Talent3Index12Enabled)); } }
        public bool Talent3Index13Enabled { get => _talent3Index13Enabled; set { _talent3Index13Enabled = value; OnPropertyChanged(nameof(Talent3Index13Enabled)); } }
        public bool Talent3Index14Enabled { get => _talent3Index14Enabled; set { _talent3Index14Enabled = value; OnPropertyChanged(nameof(Talent3Index14Enabled)); } }
        public bool Talent3Index15Enabled { get => _talent3Index15Enabled; set { _talent3Index15Enabled = value; OnPropertyChanged(nameof(Talent3Index15Enabled)); } }
        public bool Talent3Index16Enabled { get => _talent3Index16Enabled; set { _talent3Index16Enabled = value; OnPropertyChanged(nameof(Talent3Index16Enabled)); } }
        public bool Talent3Index17Enabled { get => _talent3Index17Enabled; set { _talent3Index17Enabled = value; OnPropertyChanged(nameof(Talent3Index17Enabled)); } }
        public bool Talent3Index18Enabled { get => _talent3Index18Enabled; set { _talent3Index18Enabled = value; OnPropertyChanged(nameof(Talent3Index18Enabled)); } }
        private int _talent3Index1Column;
        public int Talent3Index1Column { get => _talent3Index1Column; set { _talent3Index1Column = value; OnPropertyChanged(nameof(Talent3Index1Column)); } }
        private int _talent3Index2Column;
        public int Talent3Index2Column { get => _talent3Index2Column; set { _talent3Index2Column = value; OnPropertyChanged(nameof(Talent3Index2Column)); } }
        private int _talent3Index3Column;
        public int Talent3Index3Column { get => _talent3Index3Column; set { _talent3Index3Column = value; OnPropertyChanged(nameof(Talent3Index3Column)); } }
        private int _talent3Index4Column;
        public int Talent3Index4Column { get => _talent3Index4Column; set { _talent3Index4Column = value; OnPropertyChanged(nameof(Talent3Index4Column)); } }
        private int _talent3Index5Column;
        public int Talent3Index5Column { get => _talent3Index5Column; set { _talent3Index5Column = value; OnPropertyChanged(nameof(Talent3Index5Column)); } }
        private int _talent3Index6Column;
        public int Talent3Index6Column { get => _talent3Index6Column; set { _talent3Index6Column = value; OnPropertyChanged(nameof(Talent3Index6Column)); } }
        private int _talent3Index7Column;
        public int Talent3Index7Column { get => _talent3Index7Column; set { _talent3Index7Column = value; OnPropertyChanged(nameof(Talent3Index7Column)); } }
        private int _talent3Index8Column;
        public int Talent3Index8Column { get => _talent3Index8Column; set { _talent3Index8Column = value; OnPropertyChanged(nameof(Talent3Index8Column)); } }
        private int _talent3Index9Column;
        public int Talent3Index9Column { get => _talent3Index9Column; set { _talent3Index9Column = value; OnPropertyChanged(nameof(Talent3Index9Column)); } }
        private int _talent3Index10Column;
        public int Talent3Index10Column { get => _talent3Index10Column; set { _talent3Index10Column = value; OnPropertyChanged(nameof(Talent3Index10Column)); } }
        private int _talent3Index11Column;
        public int Talent3Index11Column { get => _talent3Index11Column; set { _talent3Index11Column = value; OnPropertyChanged(nameof(Talent3Index11Column)); } }
        private int _talent3Index12Column;
        public int Talent3Index12Column { get => _talent3Index12Column; set { _talent3Index12Column = value; OnPropertyChanged(nameof(Talent3Index12Column)); } }
        private int _talent3Index13Column;
        public int Talent3Index13Column { get => _talent3Index13Column; set { _talent3Index13Column = value; OnPropertyChanged(nameof(Talent3Index13Column)); } }
        private int _talent3Index14Column;
        public int Talent3Index14Column { get => _talent3Index14Column; set { _talent3Index14Column = value; OnPropertyChanged(nameof(Talent3Index14Column)); } }
        private int _talent3Index15Column;
        public int Talent3Index15Column { get => _talent3Index15Column; set { _talent3Index15Column = value; OnPropertyChanged(nameof(Talent3Index15Column)); } }
        private int _talent3Index16Column;
        public int Talent3Index16Column { get => _talent3Index16Column; set { _talent3Index16Column = value; OnPropertyChanged(nameof(Talent3Index16Column)); } }
        private int _talent3Index17Column;
        public int Talent3Index17Column { get => _talent3Index17Column; set { _talent3Index17Column = value; OnPropertyChanged(nameof(Talent3Index17Column)); } }
        private int _talent3Index18Column;
        public int Talent3Index18Column { get => _talent3Index18Column; set { _talent3Index18Column = value; OnPropertyChanged(nameof(Talent3Index18Column)); } }
        private int _talent3Index1Row;
        public int Talent3Index1Row { get => _talent3Index1Row; set { _talent3Index1Row = value; OnPropertyChanged(nameof(Talent3Index1Row)); } }
        private int _talent3Index2Row;
        public int Talent3Index2Row { get => _talent3Index2Row; set { _talent3Index2Row = value; OnPropertyChanged(nameof(Talent3Index2Row)); } }
        private int _talent3Index3Row;
        public int Talent3Index3Row { get => _talent3Index3Row; set { _talent3Index3Row = value; OnPropertyChanged(nameof(Talent3Index3Row)); } }
        private int _talent3Index4Row;
        public int Talent3Index4Row { get => _talent3Index4Row; set { _talent3Index4Row = value; OnPropertyChanged(nameof(Talent3Index4Row)); } }
        private int _talent3Index5Row;
        public int Talent3Index5Row { get => _talent3Index5Row; set { _talent3Index5Row = value; OnPropertyChanged(nameof(Talent3Index5Row)); } }
        private int _talent3Index6Row;
        public int Talent3Index6Row { get => _talent3Index6Row; set { _talent3Index6Row = value; OnPropertyChanged(nameof(Talent3Index6Row)); } }
        private int _talent3Index7Row;
        public int Talent3Index7Row { get => _talent3Index7Row; set { _talent3Index7Row = value; OnPropertyChanged(nameof(Talent3Index7Row)); } }
        private int _talent3Index8Row;
        public int Talent3Index8Row { get => _talent3Index8Row; set { _talent3Index8Row = value; OnPropertyChanged(nameof(Talent3Index8Row)); } }
        private int _talent3Index9Row;
        public int Talent3Index9Row { get => _talent3Index9Row; set { _talent3Index9Row = value; OnPropertyChanged(nameof(Talent3Index9Row)); } }
        private int _talent3Index10Row;
        public int Talent3Index10Row { get => _talent3Index10Row; set { _talent3Index10Row = value; OnPropertyChanged(nameof(Talent3Index10Row)); } }
        private int _talent3Index11Row;
        public int Talent3Index11Row { get => _talent3Index11Row; set { _talent3Index11Row = value; OnPropertyChanged(nameof(Talent3Index11Row)); } }
        private int _talent3Index12Row;
        public int Talent3Index12Row { get => _talent3Index12Row; set { _talent3Index12Row = value; OnPropertyChanged(nameof(Talent3Index12Row)); } }
        private int _talent3Index13Row;
        public int Talent3Index13Row { get => _talent3Index13Row; set { _talent3Index13Row = value; OnPropertyChanged(nameof(Talent3Index13Row)); } }
        private int _talent3Index14Row;
        public int Talent3Index14Row { get => _talent3Index14Row; set { _talent3Index14Row = value; OnPropertyChanged(nameof(Talent3Index14Row)); } }
        private int _talent3Index15Row;
        public int Talent3Index15Row { get => _talent3Index15Row; set { _talent3Index15Row = value; OnPropertyChanged(nameof(Talent3Index15Row)); } }
        private int _talent3Index16Row;
        public int Talent3Index16Row { get => _talent3Index16Row; set { _talent3Index16Row = value; OnPropertyChanged(nameof(Talent3Index16Row)); } }
        private int _talent3Index17Row;
        public int Talent3Index17Row { get => _talent3Index17Row; set { _talent3Index17Row = value; OnPropertyChanged(nameof(Talent3Index17Row)); } }
        private int _talent3Index18Row;
        public int Talent3Index18Row { get => _talent3Index18Row; set { _talent3Index18Row = value; OnPropertyChanged(nameof(Talent3Index18Row)); } }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
