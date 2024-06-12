using RaidLeaderBot.UI.Views.Talents;
using RaidMemberBot.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using static RaidMemberBot.Constants.Enums;

namespace RaidLeaderBot
{
    public sealed class RaidMemberViewModel : INotifyPropertyChanged
    {
        public int Index { get; set; }
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
            RefreshItemLists();
        }
        public bool IsAlliance { get; set; }
        public ObservableCollection<ItemTemplate> HeadItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> NeckItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> ShoulderItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> ShirtItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> ChestItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> WaistItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> LegItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> FeetItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> WristItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> HandItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> FingerItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> TrinketItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> BackItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> TabardItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> MainHandItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> OffHandItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public ObservableCollection<ItemTemplate> RangedItemTemplates { get; set; } = new ObservableCollection<ItemTemplate>();
        public IEnumerable<TargetMarker> EnumTargetMarkers
        {
            get
            {
                return Enum.GetValues(typeof(TargetMarker)).Cast<TargetMarker>();
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
                    Race.Gnome => new List<Class>() { Class.Mage, Class.Rogue, Class.Warlock, Class.Warrior },
                    Race.Orc => new List<Class>() { Class.Hunter, Class.Rogue, Class.Shaman, Class.Warlock, Class.Warrior },
                    Race.Undead => new List<Class>() { Class.Mage, Class.Priest, Class.Rogue, Class.Warlock, Class.Warrior },
                    Race.Tauren => new List<Class>() { Class.Druid, Class.Hunter, Class.Shaman, Class.Warrior },
                    Race.Troll => new List<Class>() { Class.Hunter, Class.Mage, Class.Priest, Class.Rogue, Class.Shaman, Class.Warrior },
                    _ => Enum.GetValues(typeof(Class)).Cast<Class>(),
                };
            }
        }
        public int Level
        {
            get => RaidMemberPreset.Level;
            set
            {
                RaidMemberPreset.Level = value;
                RefreshItemLists();
            }
        }

        private void RefreshItemLists()
        {
            HeadItemTemplates.Clear();
            NeckItemTemplates.Clear();
            ShoulderItemTemplates.Clear();
            BackItemTemplates.Clear();
            ChestItemTemplates.Clear();
            ShirtItemTemplates.Clear();
            RangedItemTemplates.Clear();
            WaistItemTemplates.Clear();
            LegItemTemplates.Clear();
            FeetItemTemplates.Clear();
            WristItemTemplates.Clear();
            HandItemTemplates.Clear();
            FingerItemTemplates.Clear();
            TrinketItemTemplates.Clear();

            MainHandItemTemplates.Clear();
            OffHandItemTemplates.Clear();
            RangedItemTemplates.Clear();

            HeadItemTemplates.Add(new ItemTemplate());
            NeckItemTemplates.Add(new ItemTemplate());
            ShoulderItemTemplates.Add(new ItemTemplate());
            BackItemTemplates.Add(new ItemTemplate());
            ChestItemTemplates.Add(new ItemTemplate());
            TabardItemTemplates.Add(new ItemTemplate());
            ShirtItemTemplates.Add(new ItemTemplate());
            WaistItemTemplates.Add(new ItemTemplate());
            LegItemTemplates.Add(new ItemTemplate());
            FeetItemTemplates.Add(new ItemTemplate());
            WristItemTemplates.Add(new ItemTemplate());
            HandItemTemplates.Add(new ItemTemplate());
            FingerItemTemplates.Add(new ItemTemplate());
            TrinketItemTemplates.Add(new ItemTemplate());

            MainHandItemTemplates.Add(new ItemTemplate());
            OffHandItemTemplates.Add(new ItemTemplate());
            RangedItemTemplates.Add(new ItemTemplate());

            List<ItemTemplate> headTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Head).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> neckTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 0, InventoryType.Neck).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> shouldersTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 0, InventoryType.Shoulders).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> shirtTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 0, InventoryType.Shirt).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> chestTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Chest).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> waistTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Waist).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> legTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Legs).OrderBy(x => x.RequiredLevel).ToList(); ;
            List<ItemTemplate> feetTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Feet).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> wristTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Wrists).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> handTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, ArmorSubClass, InventoryType.Hands).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> fingerTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 0, InventoryType.Finger).OrderBy(x => x.RequiredLevel).ToList(); ;
            List<ItemTemplate> trinketTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 0, InventoryType.Trinket).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> cloakTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 1, InventoryType.Cloak).OrderBy(x => x.RequiredLevel).ToList();
            List<ItemTemplate> offHanderTemplates = new List<ItemTemplate>();

            if (Class == Class.Warrior || Class == Class.Paladin || Class == Class.Shaman)
            {
                offHanderTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, 6, InventoryType.Shield);
            }

            List<ItemTemplate> mainHanderTemplates = new List<ItemTemplate>();
            List<ItemTemplate> rangedTemplates = new List<ItemTemplate>();

            List<short> mainHandSubClasses = MainHandSubClasses.ToList();
            List<short> offHandSubClasses = OffHandSubClasses.ToList();
            List<short> rangedSubClasses = RangedSubClasses.ToList();

            for (int i = 0; i < mainHandSubClasses.Count; i++)
            {
                List<ItemTemplate> weaponTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, mainHandSubClasses[i], InventoryType.Weapon);
                List<ItemTemplate> twoHanderTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, mainHandSubClasses[i], InventoryType.TwoHander);
                List<ItemTemplate> mainHandTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, mainHandSubClasses[i], InventoryType.MainHand);

                for (int j = 0; j < weaponTemplates.Count; j++)
                {
                    mainHanderTemplates.Add(weaponTemplates[j]);
                }
                for (int j = 0; j < twoHanderTemplates.Count; j++)
                {
                    mainHanderTemplates.Add(twoHanderTemplates[j]);
                }
                for (int j = 0; j < mainHandTemplates.Count; j++)
                {
                    mainHanderTemplates.Add(mainHandTemplates[j]);
                }
            }

            for (int i = 0; i < offHandSubClasses.Count; i++)
            {
                List<ItemTemplate> weaponTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, offHandSubClasses[i], InventoryType.Weapon);
                List<ItemTemplate> offhandTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, offHandSubClasses[i], InventoryType.Offhand);
                List<ItemTemplate> holdableTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, offHandSubClasses[i], InventoryType.Holdable);

                for (int j = 0; j < weaponTemplates.Count; j++)
                {
                    offHanderTemplates.Add(weaponTemplates[j]);
                }
                for (int j = 0; j < offhandTemplates.Count; j++)
                {
                    offHanderTemplates.Add(offhandTemplates[j]);
                }
                for (int j = 0; j < holdableTemplates.Count; j++)
                {
                    offHanderTemplates.Add(holdableTemplates[j]);
                }
            }

            for (int i = 0; i < offHandSubClasses.Count; i++)
            {
                List<ItemTemplate> weaponTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, offHandSubClasses[i], InventoryType.Weapon);
                List<ItemTemplate> offhandTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, offHandSubClasses[i], InventoryType.Offhand);
                List<ItemTemplate> holdableTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, offHandSubClasses[i], InventoryType.Holdable);

                for (int j = 0; j < weaponTemplates.Count; j++)
                {
                    offHanderTemplates.Add(weaponTemplates[j]);
                }
                for (int j = 0; j < offhandTemplates.Count; j++)
                {
                    offHanderTemplates.Add(offhandTemplates[j]);
                }
                for (int j = 0; j < holdableTemplates.Count; j++)
                {
                    offHanderTemplates.Add(holdableTemplates[j]);
                }
            }

            if (Class == Class.Druid || Class == Class.Paladin || Class == Class.Shaman)
            {
                for (int i = 0; i < rangedSubClasses.Count; i++)
                {
                    List<ItemTemplate> relicTemplates = MangosRepository.GetEquipmentByRequirements(Level, 4, rangedSubClasses[i], InventoryType.Relic);

                    for (int j = 0; j < relicTemplates.Count; j++)
                    {
                        rangedTemplates.Add(relicTemplates[j]);
                    }
                }
            }
            else
            {
                for (int i = 0; i < rangedSubClasses.Count; i++)
                {
                    List<ItemTemplate> thrownTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, rangedSubClasses[i], InventoryType.Thrown);
                    List<ItemTemplate> rangedLeftTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, rangedSubClasses[i], InventoryType.Ranged);
                    List<ItemTemplate> rangedRightTemplates = MangosRepository.GetEquipmentByRequirements(Level, 2, rangedSubClasses[i], InventoryType.RangedRight);

                    for (int j = 0; j < thrownTemplates.Count; j++)
                    {
                        rangedTemplates.Add(thrownTemplates[j]);
                    }
                    for (int j = 0; j < rangedLeftTemplates.Count; j++)
                    {
                        rangedTemplates.Add(rangedLeftTemplates[j]);
                    }
                    for (int j = 0; j < rangedRightTemplates.Count; j++)
                    {
                        rangedTemplates.Add(rangedRightTemplates[j]);
                    }
                }
            }

            mainHanderTemplates = mainHanderTemplates.OrderBy(x => x.RequiredLevel).ToList();
            offHanderTemplates = offHanderTemplates.OrderBy(x => x.RequiredLevel).ToList();
            rangedTemplates = rangedTemplates.OrderBy(x => x.RequiredLevel).ToList();

            for (int i = 0; i < mainHanderTemplates.Count; i++)
            {
                MainHandItemTemplates.Add(mainHanderTemplates[i]);
            }

            for (int i = 0; i < offHanderTemplates.Count; i++)
            {
                OffHandItemTemplates.Add(offHanderTemplates[i]);
            }

            for (int i = 0; i < rangedTemplates.Count; i++)
            {
                RangedItemTemplates.Add(rangedTemplates[i]);
            }

            for (int i = 0; i < headTemplates.Count; i++)
            {
                HeadItemTemplates.Add(headTemplates[i]);
            }

            for (int i = 0; i < neckTemplates.Count; i++)
            {
                NeckItemTemplates.Add(neckTemplates[i]);
            }

            for (int i = 0; i < shouldersTemplates.Count; i++)
            {
                ShoulderItemTemplates.Add(shouldersTemplates[i]);
            }

            for (int i = 0; i < cloakTemplates.Count; i++)
            {
                BackItemTemplates.Add(cloakTemplates[i]);
            }

            for (int i = 0; i < shirtTemplates.Count; i++)
            {
                ShirtItemTemplates.Add(shirtTemplates[i]);
            }

            for (int i = 0; i < chestTemplates.Count; i++)
            {
                ChestItemTemplates.Add(chestTemplates[i]);
            }

            for (int i = 0; i < handTemplates.Count; i++)
            {
                HandItemTemplates.Add(handTemplates[i]);
            }

            for (int i = 0; i < wristTemplates.Count; i++)
            {
                WristItemTemplates.Add(wristTemplates[i]);
            }

            for (int i = 0; i < waistTemplates.Count; i++)
            {
                WaistItemTemplates.Add(waistTemplates[i]);
            }

            for (int i = 0; i < legTemplates.Count; i++)
            {
                LegItemTemplates.Add(legTemplates[i]);
            }

            for (int i = 0; i < feetTemplates.Count; i++)
            {
                FeetItemTemplates.Add(feetTemplates[i]);
            }

            for (int i = 0; i < wristTemplates.Count; i++)
            {
                WristItemTemplates.Add(wristTemplates[i]);
            }

            for (int i = 0; i < handTemplates.Count; i++)
            {
                HandItemTemplates.Add(handTemplates[i]);
            }

            for (int i = 0; i < fingerTemplates.Count; i++)
            {
                FingerItemTemplates.Add(fingerTemplates[i]);
            }

            for (int i = 0; i < trinketTemplates.Count; i++)
            {
                TrinketItemTemplates.Add(trinketTemplates[i]);
            }

            OnPropertyChanged(nameof(HeadItem));
            OnPropertyChanged(nameof(NeckItem));
            OnPropertyChanged(nameof(ShoulderItem));
            OnPropertyChanged(nameof(BackItem));
            OnPropertyChanged(nameof(ChestItem));
            OnPropertyChanged(nameof(ShirtItem));
            OnPropertyChanged(nameof(HandsItem));
            OnPropertyChanged(nameof(WaistItem));
            OnPropertyChanged(nameof(WristsItem));
            OnPropertyChanged(nameof(LegsItem));
            OnPropertyChanged(nameof(FeetItem));
            OnPropertyChanged(nameof(WristsItem));
            OnPropertyChanged(nameof(Finger1Item));
            OnPropertyChanged(nameof(Finger2Item));
            OnPropertyChanged(nameof(Trinket1Item));
            OnPropertyChanged(nameof(Trinket2Item));
            OnPropertyChanged(nameof(MainHandItem));
            OnPropertyChanged(nameof(OffHandItem));
            OnPropertyChanged(nameof(RangedItem));
        }

        private short ArmorSubClass
        {
            get
            {
                short requiredSkill = 1;
                switch (Class)
                {
                    case Class.Druid:
                    case Class.Rogue:
                        requiredSkill = 2;
                        break;
                    case Class.Hunter:
                    case Class.Shaman:
                        if (Level < 40)
                        {
                            requiredSkill = 2;
                        }
                        else
                        {
                            requiredSkill = 3;
                        }
                        break;
                    case Class.Paladin:
                    case Class.Warrior:
                        if (Level < 40)
                        {
                            requiredSkill = 3;
                        }
                        else
                        {
                            requiredSkill = 4;
                        }
                        break;
                }

                return requiredSkill;
            }
        }
        private List<short> MainHandSubClasses
        {
            get
            {
                List<short> weaponSubClasses = new List<short>();
                switch (Class)
                {
                    case Class.Druid:
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(5);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        break;
                    case Class.Hunter:
                        if (Level > 19)
                        {
                            weaponSubClasses.Add(6);
                        }

                        weaponSubClasses.Add(0);
                        weaponSubClasses.Add(1);
                        weaponSubClasses.Add(2);
                        weaponSubClasses.Add(3);
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(8);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        weaponSubClasses.Add(16);
                        weaponSubClasses.Add(18);
                        break;
                    case Class.Mage:
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(15);
                        weaponSubClasses.Add(19);
                        break;
                    case Class.Paladin:
                        weaponSubClasses.Add(0);
                        weaponSubClasses.Add(1);
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(5);
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(8);
                        break;
                    case Class.Priest:
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(15);
                        weaponSubClasses.Add(19);
                        break;
                    case Class.Rogue:
                        weaponSubClasses.Add(2);
                        weaponSubClasses.Add(3);
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        weaponSubClasses.Add(16);
                        weaponSubClasses.Add(18);
                        break;
                    case Class.Shaman:
                        weaponSubClasses.Add(0);
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        break;
                    case Class.Warlock:
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(15);
                        weaponSubClasses.Add(19);
                        break;
                    case Class.Warrior:
                        if (Level > 19)
                        {
                            weaponSubClasses.Add(6);
                        }

                        weaponSubClasses.Add(0);
                        weaponSubClasses.Add(1);
                        weaponSubClasses.Add(2);
                        weaponSubClasses.Add(3);
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(5);
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(8);
                        weaponSubClasses.Add(10);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        weaponSubClasses.Add(16);
                        weaponSubClasses.Add(17);
                        weaponSubClasses.Add(20);
                        break;
                }

                return weaponSubClasses;
            }
        }
        private List<short> OffHandSubClasses
        {
            get
            {
                List<short> weaponSubClasses = new List<short>
                {
                    14
                };
                switch (Class)
                {
                    case Class.Druid:
                        break;
                    case Class.Hunter:
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        break;
                    case Class.Mage:
                        break;
                    case Class.Paladin:
                        break;
                    case Class.Priest:
                        break;
                    case Class.Rogue:
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        break;
                    case Class.Shaman:
                        break;
                    case Class.Warlock:
                        break;
                    case Class.Warrior:
                        weaponSubClasses.Add(0);
                        weaponSubClasses.Add(4);
                        weaponSubClasses.Add(7);
                        weaponSubClasses.Add(13);
                        weaponSubClasses.Add(15);
                        break;
                }

                return weaponSubClasses;
            }
        }
        private List<short> RangedSubClasses
        {
            get
            {
                List<short> weaponSubClasses = new List<short>();
                switch (Class)
                {
                    case Class.Druid:
                        weaponSubClasses.Add(8);
                        break;
                    case Class.Mage:
                    case Class.Priest:
                    case Class.Warlock:
                        weaponSubClasses.Add(19);
                        break;
                    case Class.Paladin:
                        weaponSubClasses.Add(7);
                        break;
                    case Class.Shaman:
                        weaponSubClasses.Add(9);
                        break;
                    case Class.Hunter:
                    case Class.Rogue:
                    case Class.Warrior:
                        weaponSubClasses.Add(2);
                        weaponSubClasses.Add(3);
                        weaponSubClasses.Add(16);
                        weaponSubClasses.Add(18);
                        break;
                }

                return weaponSubClasses;
            }
        }

        public IEnumerable<Race> Races
        {
            get
            {
                return IsAlliance ? new List<Race>() { Race.Human, Race.Dwarf, Race.NightElf, Race.Gnome } : new List<Race>() { Race.Orc, Race.Undead, Race.Tauren, Race.Troll };
            }
        }
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
                RefreshItemLists();
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

        public ItemTemplate HeadItem
        {
            get => HeadItemTemplates.First(x => x.Entry == _raidMemberPreset.HeadItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.HeadItem = value.Entry;
                else
                    _raidMemberPreset.HeadItem = 0;
            }
        }
        public ItemTemplate NeckItem
        {
            get => NeckItemTemplates.First(x => x.Entry == _raidMemberPreset.NeckItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.NeckItem = value.Entry;
                else
                    _raidMemberPreset.NeckItem = 0;
            }
        }
        public ItemTemplate ShoulderItem
        {
            get => ShoulderItemTemplates.First(x => x.Entry == _raidMemberPreset.ShoulderItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.ShoulderItem = value.Entry;
                else
                    _raidMemberPreset.ShoulderItem = 0;
            }
        }
        public ItemTemplate ChestItem
        {
            get => ChestItemTemplates.First(x => x.Entry == _raidMemberPreset.ChestItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.ChestItem = value.Entry;
                else
                    _raidMemberPreset.ChestItem = 0;
            }
        }
        public ItemTemplate BackItem
        {
            get => BackItemTemplates.First(x => x.Entry == _raidMemberPreset.BackItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.BackItem = value.Entry;
                else
                    _raidMemberPreset.BackItem = 0;
            }
        }
        public ItemTemplate TabardItem
        {
            get => TabardItemTemplates.First(x => x.Entry == _raidMemberPreset.RobeItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.RobeItem = value.Entry;
                else
                    _raidMemberPreset.RobeItem = 0;
            }
        }
        public ItemTemplate ShirtItem
        {
            get => ShirtItemTemplates.First(x => x.Entry == _raidMemberPreset.ShirtItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.ShirtItem = value.Entry;
                else
                    _raidMemberPreset.ShirtItem = 0;
            }
        }
        public ItemTemplate WristsItem
        {
            get => WristItemTemplates.First(x => x.Entry == _raidMemberPreset.WristsItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.WristsItem = value.Entry;
                else
                    _raidMemberPreset.WristsItem = 0;
            }
        }
        public ItemTemplate HandsItem
        {
            get => HandItemTemplates.First(x => x.Entry == _raidMemberPreset.HandsItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.HandsItem = value.Entry;
                else
                    _raidMemberPreset.HandsItem = 0;
            }
        }
        public ItemTemplate WaistItem
        {
            get => WaistItemTemplates.First(x => x.Entry == _raidMemberPreset.WaistItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.WaistItem = value.Entry;
                else
                    _raidMemberPreset.WaistItem = 0;
            }
        }
        public ItemTemplate LegsItem
        {
            get => LegItemTemplates.First(x => x.Entry == _raidMemberPreset.LegsItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.LegsItem = value.Entry;
                else
                    _raidMemberPreset.LegsItem = 0;
            }
        }
        public ItemTemplate FeetItem
        {
            get => FeetItemTemplates.First(x => x.Entry == _raidMemberPreset.FeetItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.FeetItem = value.Entry;
                else
                    _raidMemberPreset.FeetItem = 0;
            }
        }
        public ItemTemplate Finger1Item
        {
            get => FingerItemTemplates.First(x => x.Entry == _raidMemberPreset.Finger1Item);
            set
            {
                if (value != null)
                    _raidMemberPreset.Finger1Item = value.Entry;
                else
                    _raidMemberPreset.Finger1Item = 0;
            }
        }
        public ItemTemplate Finger2Item
        {
            get => FingerItemTemplates.First(x => x.Entry == _raidMemberPreset.Finger2Item);
            set
            {
                if (value != null)
                    _raidMemberPreset.Finger2Item = value.Entry;
                else
                    _raidMemberPreset.Finger2Item = 0;
            }
        }
        public ItemTemplate Trinket1Item
        {
            get => TrinketItemTemplates.First(x => x.Entry == _raidMemberPreset.Trinket1Item);
            set
            {
                if (value != null)
                    _raidMemberPreset.Trinket1Item = value.Entry;
                else
                    _raidMemberPreset.Trinket1Item = 0;
            }
        }
        public ItemTemplate Trinket2Item
        {
            get => TrinketItemTemplates.First(x => x.Entry == _raidMemberPreset.Trinket2Item);
            set
            {
                if (value != null)
                    _raidMemberPreset.Trinket2Item = value.Entry;
                else
                    _raidMemberPreset.Trinket2Item = 0;
            }
        }
        public ItemTemplate MainHandItem
        {
            get => MainHandItemTemplates.First(x => x.Entry == _raidMemberPreset.MainHandItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.MainHandItem = value.Entry;
                else
                    _raidMemberPreset.MainHandItem = 0;
            }
        }
        public ItemTemplate OffHandItem
        {
            get => OffHandItemTemplates.First(x => x.Entry == _raidMemberPreset.OffHandItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.OffHandItem = value.Entry;
                else
                    _raidMemberPreset.OffHandItem = 0;
            }
        }
        public ItemTemplate RangedItem
        {
            get => RangedItemTemplates.First(x => x.Entry == _raidMemberPreset.RangedItem);
            set
            {
                if (value != null)
                    _raidMemberPreset.RangedItem = value.Entry;
                else
                    _raidMemberPreset.RangedItem = 0;
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
        public List<int> Skills => _raidMemberPreset.Skills;
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
