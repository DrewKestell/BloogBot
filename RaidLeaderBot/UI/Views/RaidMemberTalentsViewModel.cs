using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static RaidMemberBot.Constants.Enums;
using System.Windows;
using System.Windows.Input;
using Newtonsoft.Json;

namespace RaidLeaderBot
{
    public abstract class RaidMemberTalentsViewModel : INotifyPropertyChanged
    {
        public RaidMemberTalentsViewModel()
        {
            _raidMemberPreset = new RaidMemberPreset();
        }
        private string _talent1Header;
        public string Talent1Header
        {
            get => _talent1Header;
            set
            {
                _talent1Header = value;
                OnPropertyChanged(Talent1Header);
            }
        }
        private string _talent2Header;
        public string Talent2Header
        {
            get => _talent2Header;
            set
            {
                _talent2Header = value;
                OnPropertyChanged(Talent2Header);
            }
        }
        private string _talent3Header;
        public string Talent3Header
        {
            get => _talent3Header;
            set
            {
                _talent3Header = value;
                OnPropertyChanged(Talent3Header);
            }
        }
        private RaidMemberPreset _raidMemberPreset;
        public RaidMemberPreset RaidMemberPreset
        {
            set => _raidMemberPreset = value;
            get => _raidMemberPreset;
        }
        public void RefreshButtonEnables()
        {
            OnPropertyChanged(nameof(Talent1Index1Enabled));
            OnPropertyChanged(nameof(Talent1Index2Enabled));
            OnPropertyChanged(nameof(Talent1Index3Enabled));
            OnPropertyChanged(nameof(Talent1Index4Enabled));
            OnPropertyChanged(nameof(Talent1Index5Enabled));
            OnPropertyChanged(nameof(Talent1Index6Enabled));
            OnPropertyChanged(nameof(Talent1Index7Enabled));
            OnPropertyChanged(nameof(Talent1Index8Enabled));
            OnPropertyChanged(nameof(Talent1Index9Enabled));
            OnPropertyChanged(nameof(Talent1Index10Enabled));
            OnPropertyChanged(nameof(Talent1Index11Enabled));
            OnPropertyChanged(nameof(Talent1Index12Enabled));
            OnPropertyChanged(nameof(Talent1Index13Enabled));
            OnPropertyChanged(nameof(Talent1Index14Enabled));
            OnPropertyChanged(nameof(Talent1Index15Enabled));
            OnPropertyChanged(nameof(Talent1Index16Enabled));
            OnPropertyChanged(nameof(Talent1Index17Enabled));
            OnPropertyChanged(nameof(Talent1Index18Enabled));

            OnPropertyChanged(nameof(Talent2Index1Enabled));
            OnPropertyChanged(nameof(Talent2Index2Enabled));
            OnPropertyChanged(nameof(Talent2Index3Enabled));
            OnPropertyChanged(nameof(Talent2Index4Enabled));
            OnPropertyChanged(nameof(Talent2Index5Enabled));
            OnPropertyChanged(nameof(Talent2Index6Enabled));
            OnPropertyChanged(nameof(Talent2Index7Enabled));
            OnPropertyChanged(nameof(Talent2Index8Enabled));
            OnPropertyChanged(nameof(Talent2Index9Enabled));
            OnPropertyChanged(nameof(Talent2Index10Enabled));
            OnPropertyChanged(nameof(Talent2Index11Enabled));
            OnPropertyChanged(nameof(Talent2Index12Enabled));
            OnPropertyChanged(nameof(Talent2Index13Enabled));
            OnPropertyChanged(nameof(Talent2Index14Enabled));
            OnPropertyChanged(nameof(Talent2Index15Enabled));
            OnPropertyChanged(nameof(Talent2Index16Enabled));
            OnPropertyChanged(nameof(Talent2Index17Enabled));
            OnPropertyChanged(nameof(Talent2Index18Enabled));
            OnPropertyChanged(nameof(Talent2Index19Enabled));

            OnPropertyChanged(nameof(Talent3Index1Enabled));
            OnPropertyChanged(nameof(Talent3Index2Enabled));
            OnPropertyChanged(nameof(Talent3Index3Enabled));
            OnPropertyChanged(nameof(Talent3Index4Enabled));
            OnPropertyChanged(nameof(Talent3Index5Enabled));
            OnPropertyChanged(nameof(Talent3Index6Enabled));
            OnPropertyChanged(nameof(Talent3Index7Enabled));
            OnPropertyChanged(nameof(Talent3Index8Enabled));
            OnPropertyChanged(nameof(Talent3Index9Enabled));
            OnPropertyChanged(nameof(Talent3Index10Enabled));
            OnPropertyChanged(nameof(Talent3Index11Enabled));
            OnPropertyChanged(nameof(Talent3Index12Enabled));
            OnPropertyChanged(nameof(Talent3Index13Enabled));
            OnPropertyChanged(nameof(Talent3Index14Enabled));
            OnPropertyChanged(nameof(Talent3Index15Enabled));
            OnPropertyChanged(nameof(Talent3Index16Enabled));
            OnPropertyChanged(nameof(Talent3Index17Enabled));
            OnPropertyChanged(nameof(Talent3Index18Enabled));

            Console.WriteLine(JsonConvert.SerializeObject(Talent1Spells));
            Console.WriteLine(JsonConvert.SerializeObject(Talent2Spells));
            Console.WriteLine(JsonConvert.SerializeObject(Talent3Spells));
        }
        public List<int> Talent1Spells { get => _raidMemberPreset.Talent1Spells; }
        public List<int> Talent2Spells { get => _raidMemberPreset.Talent2Spells; }
        public List<int> Talent3Spells { get => _raidMemberPreset.Talent3Spells; }

        private string _talentTree1TooltipHeader;
        private string _talentTree2TooltipHeader;
        private string _talentTree3TooltipHeader;
        private string _talentTree1TooltipSubtext;
        private string _talentTree2TooltipSubtext;
        private string _talentTree3TooltipSubtext;
        public string TalentTree1TooltipHeader { get => _talentTree1TooltipHeader; set { _talentTree1TooltipHeader = value; OnPropertyChanged(nameof(TalentTree1TooltipHeader)); } }
        public string TalentTree2TooltipHeader { get => _talentTree2TooltipHeader; set { _talentTree2TooltipHeader = value; OnPropertyChanged(nameof(TalentTree2TooltipHeader)); } }
        public string TalentTree3TooltipHeader { get => _talentTree3TooltipHeader; set { _talentTree3TooltipHeader = value; OnPropertyChanged(nameof(TalentTree3TooltipHeader)); } }
        public string TalentTree1TooltipSubtext { get => _talentTree1TooltipSubtext; set { _talentTree1TooltipSubtext = value; OnPropertyChanged(nameof(TalentTree1TooltipSubtext)); } }
        public string TalentTree2TooltipSubtext { get => _talentTree2TooltipSubtext; set { _talentTree2TooltipSubtext = value; OnPropertyChanged(nameof(TalentTree2TooltipSubtext)); } }
        public string TalentTree3TooltipSubtext { get => _talentTree3TooltipSubtext; set { _talentTree3TooltipSubtext = value; OnPropertyChanged(nameof(TalentTree3TooltipSubtext)); } }
        public Visibility Talent1Index15Visibility { get; set; }
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

        public List<int> Talent1Row1Spells = new List<int>();
        public List<int> Talent1Row2Spells = new List<int>();
        public List<int> Talent1Row3Spells = new List<int>();
        public List<int> Talent1Row4Spells = new List<int>();
        public List<int> Talent1Row5Spells = new List<int>();
        public List<int> Talent1Row6Spells = new List<int>();
        public List<int> Talent1Row7Spells = new List<int>();

        public List<int> Talent1Index1Spells = new List<int>();
        public List<int> Talent1Index2Spells = new List<int>();
        public List<int> Talent1Index3Spells = new List<int>();
        public List<int> Talent1Index4Spells = new List<int>();
        public List<int> Talent1Index5Spells = new List<int>();
        public List<int> Talent1Index6Spells = new List<int>();
        public List<int> Talent1Index7Spells = new List<int>();
        public List<int> Talent1Index8Spells = new List<int>();
        public List<int> Talent1Index9Spells = new List<int>();
        public List<int> Talent1Index10Spells = new List<int>();
        public List<int> Talent1Index11Spells = new List<int>();
        public List<int> Talent1Index12Spells = new List<int>();
        public List<int> Talent1Index13Spells = new List<int>();
        public List<int> Talent1Index14Spells = new List<int>();
        public List<int> Talent1Index15Spells = new List<int>();
        public List<int> Talent1Index16Spells = new List<int>();
        public List<int> Talent1Index17Spells = new List<int>();
        public List<int> Talent1Index18Spells = new List<int>();
        public void AddTalent1Index1()
        {
            for (int i = 0; i < Talent1Index1Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index1Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index1Spells[i]);
                    Talent1Index1Content = $"{i + 1} / {Talent1Index1Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }

        public void RemoveTalent1Index1()
        {
            if (Talent1Index1CanRemove)
            {
                for (int i = Talent1Index1Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index1Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index1Spells[i]);
                        Talent1Index1Content = $"{i} / {Talent1Index1Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index2()
        {
            for (int i = 0; i < Talent1Index2Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index2Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index2Spells[i]);
                    Talent1Index2Content = $"{i + 1} / {Talent1Index2Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index2()
        {
            if (Talent1Index2CanRemove)
            {
                for (int i = Talent1Index2Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index2Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index2Spells[i]);
                        Talent1Index2Content = $"{i} / {Talent1Index2Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }

        public void AddTalent1Index3()
        {
            for (int i = 0; i < Talent1Index3Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index3Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index3Spells[i]);
                    Talent1Index3Content = $"{i + 1} / {Talent1Index3Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index3()
        {
            if (Talent1Index3CanRemove)
            {
                for (int i = Talent1Index3Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index3Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index3Spells[i]);
                        Talent1Index3Content = $"{i} / {Talent1Index3Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index4()
        {
            for (int i = 0; i < Talent1Index4Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index4Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index4Spells[i]);
                    Talent1Index4Content = $"{i + 1} / {Talent1Index4Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index4()
        {
            if (Talent1Index4CanRemove)
            {
                for (int i = Talent1Index4Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index4Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index4Spells[i]);
                        Talent1Index4Content = $"{i} / {Talent1Index4Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index5()
        {
            for (int i = 0; i < Talent1Index5Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index5Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index5Spells[i]);
                    Talent1Index5Content = $"{i + 1} / {Talent1Index5Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index5()
        {
            if (Talent1Index5CanRemove)
            {
                for (int i = Talent1Index5Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index5Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index5Spells[i]);
                        Talent1Index5Content = $"{i} / {Talent1Index6Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index6()
        {
            for (int i = 0; i < Talent1Index6Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index6Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index6Spells[i]);
                    Talent1Index6Content = $"{i + 1} / {Talent1Index6Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index6()
        {
            if (Talent1Index6CanRemove)
            {
                for (int i = Talent1Index6Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index6Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index6Spells[i]);
                        Talent1Index7Content = $"{i} / {Talent1Index7Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index7()
        {
            for (int i = 0; i < Talent1Index7Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index7Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index7Spells[i]);
                    Talent1Index7Content = $"{i + 1} / {Talent1Index7Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index7()
        {
            if (Talent1Index7CanRemove)
            {
                for (int i = Talent1Index7Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index7Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index7Spells[i]);
                        Talent1Index7Content = $"{i} / {Talent1Index7Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index8()
        {
            for (int i = 0; i < Talent1Index8Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index8Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index8Spells[i]);
                    Talent1Index8Content = $"{i + 1} / {Talent1Index8Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index8()
        {
            if (Talent1Index8CanRemove)
            {
                for (int i = Talent1Index8Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index8Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index8Spells[i]);
                        Talent1Index8Content = $"{i} / {Talent1Index8Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index9()
        {
            for (int i = 0; i < Talent1Index9Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index9Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index9Spells[i]);
                    Talent1Index9Content = $"{i + 1} / {Talent1Index9Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index9()
        {
            if (Talent1Index9CanRemove)
            {
                for (int i = Talent1Index9Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index9Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index9Spells[i]);
                        Talent1Index9Content = $"{i} / {Talent1Index9Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index10()
        {
            for (int i = 0; i < Talent1Index10Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index10Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index10Spells[i]);
                    Talent1Index10Content = $"{i + 1} / {Talent1Index10Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index10()
        {
            if (Talent1Index10CanRemove)
            {
                for (int i = Talent1Index10Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index10Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index10Spells[i]);
                        Talent1Index10Content = $"{i} / {Talent1Index10Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index11()
        {
            for (int i = 0; i < Talent1Index11Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index11Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index11Spells[i]);
                    Talent1Index11Content = $"{i + 1} / {Talent1Index11Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index11()
        {
            if (Talent1Index11CanRemove)
            {
                for (int i = Talent1Index11Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index11Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index11Spells[i]);
                        Talent1Index11Content = $"{i} / {Talent1Index11Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index12()
        {
            for (int i = 0; i < Talent1Index12Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index12Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index12Spells[i]);
                    Talent1Index12Content = $"{i + 1} / {Talent1Index12Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index12()
        {
            if (Talent1Index12CanRemove)
            {
                for (int i = Talent1Index12Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index12Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index12Spells[i]);
                        Talent1Index12Content = $"{i} / {Talent1Index12Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index13()
        {
            for (int i = 0; i < Talent1Index13Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index13Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index13Spells[i]);
                    Talent1Index13Content = $"{i + 1} / {Talent1Index13Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index13()
        {
            if (Talent1Index13CanRemove)
            {
                for (int i = Talent1Index13Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index13Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index13Spells[i]);
                        Talent1Index13Content = $"{i} / {Talent1Index13Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index14()
        {
            for (int i = 0; i < Talent1Index14Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index14Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index14Spells[i]);
                    Talent1Index14Content = $"{i + 1} / {Talent1Index14Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index14()
        {
            if (Talent1Index14CanRemove)
            {
                for (int i = Talent1Index14Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index14Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index14Spells[i]);
                        Talent1Index14Content = $"{i} / {Talent1Index14Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index15()
        {
            for (int i = 0; i < Talent1Index15Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index15Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index15Spells[i]);
                    Talent1Index15Content = $"{i + 1} / {Talent1Index15Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index15()
        {
            if (Talent1Index15CanRemove)
            {
                for (int i = Talent1Index15Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index15Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index15Spells[i]);
                        Talent1Index15Content = $"{i} / {Talent1Index15Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index16()
        {
            for (int i = 0; i < Talent1Index16Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index16Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index16Spells[i]);
                    Talent1Index16Content = $"{i + 1} / {Talent1Index16Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index16()
        {
            if (Talent1Index16CanRemove)
            {
                for (int i = Talent1Index16Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index16Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index16Spells[i]);
                        Talent1Index16Content = $"{i} / {Talent1Index16Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index17()
        {
            for (int i = 0; i < Talent1Index17Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index17Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index17Spells[i]);
                    Talent1Index17Content = $"{i + 1} / {Talent1Index17Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index17()
        {
            if (Talent1Index17CanRemove)
            {
                for (int i = Talent1Index17Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index17Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index17Spells[i]);
                        Talent1Index17Content = $"{i} / {Talent1Index17Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent1Index18()
        {
            for (int i = 0; i < Talent1Index18Spells.Count; i++)
            {
                if (!Talent1Spells.Contains(Talent1Index18Spells[i]))
                {
                    Talent1Spells.Add(Talent1Index18Spells[i]);
                    Talent1Index18Content = $"{i + 1} / {Talent1Index18Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent1Index18()
        {
            if (Talent1Index18CanRemove)
            {
                for (int i = Talent1Index18Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent1Spells.Contains(Talent1Index18Spells[i]))
                    {
                        Talent1Spells.Remove(Talent1Index18Spells[i]);
                        Talent1Index18Content = $"{i} / {Talent1Index18Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public ICommand Talent1Index1AddCommand => _talent1Index1AddCommand ??= new CommandHandler(AddTalent1Index1, true);
        public ICommand Talent1Index2AddCommand => _talent1Index2AddCommand ??= new CommandHandler(AddTalent1Index2, true);
        public ICommand Talent1Index3AddCommand => _talent1Index3AddCommand ??= new CommandHandler(AddTalent1Index3, true);
        public ICommand Talent1Index4AddCommand => _talent1Index4AddCommand ??= new CommandHandler(AddTalent1Index4, true);
        public ICommand Talent1Index5AddCommand => _talent1Index5AddCommand ??= new CommandHandler(AddTalent1Index5, true);
        public ICommand Talent1Index6AddCommand => _talent1Index6AddCommand ??= new CommandHandler(AddTalent1Index6, true);
        public ICommand Talent1Index7AddCommand => _talent1Index7AddCommand ??= new CommandHandler(AddTalent1Index7, true);
        public ICommand Talent1Index8AddCommand => _talent1Index8AddCommand ??= new CommandHandler(AddTalent1Index8, true);
        public ICommand Talent1Index9AddCommand => _talent1Index9AddCommand ??= new CommandHandler(AddTalent1Index9, true);
        public ICommand Talent1Index10AddCommand => _talent1Index10AddCommand ??= new CommandHandler(AddTalent1Index10, true);
        public ICommand Talent1Index11AddCommand => _talent1Index11AddCommand ??= new CommandHandler(AddTalent1Index11, true);
        public ICommand Talent1Index12AddCommand => _talent1Index12AddCommand ??= new CommandHandler(AddTalent1Index12, true);
        public ICommand Talent1Index13AddCommand => _talent1Index13AddCommand ??= new CommandHandler(AddTalent1Index13, true);
        public ICommand Talent1Index14AddCommand => _talent1Index14AddCommand ??= new CommandHandler(AddTalent1Index14, true);
        public ICommand Talent1Index15AddCommand => _talent1Index15AddCommand ??= new CommandHandler(AddTalent1Index15, true);
        public ICommand Talent1Index16AddCommand => _talent1Index16AddCommand ??= new CommandHandler(AddTalent1Index16, true);
        public ICommand Talent1Index17AddCommand => _talent1Index17AddCommand ??= new CommandHandler(AddTalent1Index17, true);
        public ICommand Talent1Index18AddCommand => _talent1Index18AddCommand ??= new CommandHandler(AddTalent1Index18, true);

        public ICommand Talent1Index1RemoveCommand => _talent1Index1RemoveCommand ??= new CommandHandler(RemoveTalent1Index1, true);
        public ICommand Talent1Index2RemoveCommand => _talent1Index2RemoveCommand ??= new CommandHandler(RemoveTalent1Index2, true);
        public ICommand Talent1Index3RemoveCommand => _talent1Index3RemoveCommand ??= new CommandHandler(RemoveTalent1Index3, true);
        public ICommand Talent1Index4RemoveCommand => _talent1Index4RemoveCommand ??= new CommandHandler(RemoveTalent1Index4, true);
        public ICommand Talent1Index5RemoveCommand => _talent1Index5RemoveCommand ??= new CommandHandler(RemoveTalent1Index5, true);
        public ICommand Talent1Index6RemoveCommand => _talent1Index6RemoveCommand ??= new CommandHandler(RemoveTalent1Index6, true);
        public ICommand Talent1Index7RemoveCommand => _talent1Index7RemoveCommand ??= new CommandHandler(RemoveTalent1Index7, true);
        public ICommand Talent1Index8RemoveCommand => _talent1Index8RemoveCommand ??= new CommandHandler(RemoveTalent1Index8, true);
        public ICommand Talent1Index9RemoveCommand => _talent1Index9RemoveCommand ??= new CommandHandler(RemoveTalent1Index9, true);
        public ICommand Talent1Index10RemoveCommand => _talent1Index10RemoveCommand ??= new CommandHandler(RemoveTalent1Index10, true);
        public ICommand Talent1Index11RemoveCommand => _talent1Index11RemoveCommand ??= new CommandHandler(RemoveTalent1Index11, true);
        public ICommand Talent1Index12RemoveCommand => _talent1Index12RemoveCommand ??= new CommandHandler(RemoveTalent1Index12, true);
        public ICommand Talent1Index13RemoveCommand => _talent1Index13RemoveCommand ??= new CommandHandler(RemoveTalent1Index13, true);
        public ICommand Talent1Index14RemoveCommand => _talent1Index14RemoveCommand ??= new CommandHandler(RemoveTalent1Index14, true);
        public ICommand Talent1Index15RemoveCommand => _talent1Index15RemoveCommand ??= new CommandHandler(RemoveTalent1Index15, true);
        public ICommand Talent1Index16RemoveCommand => _talent1Index16RemoveCommand ??= new CommandHandler(RemoveTalent1Index16, true);
        public ICommand Talent1Index17RemoveCommand => _talent1Index17RemoveCommand ??= new CommandHandler(RemoveTalent1Index17, true);
        public ICommand Talent1Index18RemoveCommand => _talent1Index18RemoveCommand ??= new CommandHandler(RemoveTalent1Index18, true);

        public List<int> Talent2Row1Spells = new List<int>();
        public List<int> Talent2Row2Spells = new List<int>();
        public List<int> Talent2Row3Spells = new List<int>();
        public List<int> Talent2Row4Spells = new List<int>();
        public List<int> Talent2Row5Spells = new List<int>();
        public List<int> Talent2Row6Spells = new List<int>();
        public List<int> Talent2Row7Spells = new List<int>();

        public List<int> Talent2Index1Spells = new List<int>();
        public List<int> Talent2Index2Spells = new List<int>();
        public List<int> Talent2Index3Spells = new List<int>();
        public List<int> Talent2Index4Spells = new List<int>();
        public List<int> Talent2Index5Spells = new List<int>();
        public List<int> Talent2Index6Spells = new List<int>();
        public List<int> Talent2Index7Spells = new List<int>();
        public List<int> Talent2Index8Spells = new List<int>();
        public List<int> Talent2Index9Spells = new List<int>();
        public List<int> Talent2Index10Spells = new List<int>();
        public List<int> Talent2Index11Spells = new List<int>();
        public List<int> Talent2Index12Spells = new List<int>();
        public List<int> Talent2Index13Spells = new List<int>();
        public List<int> Talent2Index14Spells = new List<int>();
        public List<int> Talent2Index15Spells = new List<int>();
        public List<int> Talent2Index16Spells = new List<int>();
        public List<int> Talent2Index17Spells = new List<int>();
        public List<int> Talent2Index18Spells = new List<int>();
        public List<int> Talent2Index19Spells = new List<int>();
        public void AddTalent2Index1()
        {
            for (int i = 0; i < Talent2Index1Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index1Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index1Spells[i]);
                    Talent2Index1Content = $"{i + 1} / {Talent2Index1Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }

        public void RemoveTalent2Index1()
        {
            if (Talent2Index1CanRemove)
            {
                for (int i = Talent2Index1Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index1Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index1Spells[i]);
                        Talent2Index1Content = $"{i} / {Talent2Index1Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index2()
        {
            for (int i = 0; i < Talent2Index2Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index2Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index2Spells[i]);
                    Talent2Index2Content = $"{i + 1} / {Talent2Index2Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index2()
        {
            if (Talent2Index2CanRemove)
            {
                for (int i = Talent2Index2Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index2Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index2Spells[i]);
                        Talent2Index2Content = $"{i} / {Talent2Index2Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }

        public void AddTalent2Index3()
        {
            for (int i = 0; i < Talent2Index3Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index3Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index3Spells[i]);
                    Talent2Index3Content = $"{i + 1} / {Talent2Index3Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index3()
        {
            if (Talent2Index3CanRemove)
            {
                for (int i = Talent2Index3Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index3Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index3Spells[i]);
                        Talent2Index3Content = $"{i} / {Talent2Index3Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index4()
        {
            for (int i = 0; i < Talent2Index4Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index4Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index4Spells[i]);
                    Talent2Index4Content = $"{i + 1} / {Talent2Index4Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index4()
        {
            if (Talent2Index4CanRemove)
            {
                for (int i = Talent2Index4Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index4Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index4Spells[i]);
                        Talent2Index4Content = $"{i} / {Talent2Index4Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index5()
        {
            for (int i = 0; i < Talent2Index5Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index5Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index5Spells[i]);
                    Talent2Index5Content = $"{i + 1} / {Talent2Index5Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index5()
        {
            if (Talent2Index5CanRemove)
            {
                for (int i = Talent2Index5Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index5Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index5Spells[i]);
                        Talent2Index5Content = $"{i} / {Talent2Index6Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index6()
        {
            for (int i = 0; i < Talent2Index6Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index6Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index6Spells[i]);
                    Talent2Index6Content = $"{i + 1} / {Talent2Index6Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index6()
        {
            if (Talent2Index6CanRemove)
            {
                for (int i = Talent2Index6Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index6Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index6Spells[i]);
                        Talent2Index7Content = $"{i} / {Talent2Index7Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index7()
        {
            for (int i = 0; i < Talent2Index7Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index7Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index7Spells[i]);
                    Talent2Index7Content = $"{i + 1} / {Talent2Index7Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index7()
        {
            if (Talent2Index7CanRemove)
            {
                for (int i = Talent2Index7Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index7Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index7Spells[i]);
                        Talent2Index7Content = $"{i} / {Talent2Index7Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index8()
        {
            for (int i = 0; i < Talent2Index8Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index8Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index8Spells[i]);
                    Talent2Index8Content = $"{i + 1} / {Talent2Index8Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index8()
        {
            if (Talent2Index8CanRemove)
            {
                for (int i = Talent2Index8Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index8Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index8Spells[i]);
                        Talent2Index8Content = $"{i} / {Talent2Index8Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index9()
        {
            for (int i = 0; i < Talent2Index9Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index9Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index9Spells[i]);
                    Talent2Index9Content = $"{i + 1} / {Talent2Index9Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index9()
        {
            if (Talent2Index9CanRemove)
            {
                for (int i = Talent2Index9Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index9Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index9Spells[i]);
                        Talent2Index9Content = $"{i} / {Talent2Index9Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index10()
        {
            for (int i = 0; i < Talent2Index10Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index10Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index10Spells[i]);
                    Talent2Index10Content = $"{i + 1} / {Talent2Index10Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index10()
        {
            if (Talent2Index10CanRemove)
            {
                for (int i = Talent2Index10Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index10Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index10Spells[i]);
                        Talent2Index10Content = $"{i} / {Talent2Index10Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index11()
        {
            for (int i = 0; i < Talent2Index11Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index11Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index11Spells[i]);
                    Talent2Index11Content = $"{i + 1} / {Talent2Index11Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index11()
        {
            if (Talent2Index11CanRemove)
            {
                for (int i = Talent2Index11Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index11Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index11Spells[i]);
                        Talent2Index11Content = $"{i} / {Talent2Index11Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index12()
        {
            for (int i = 0; i < Talent2Index12Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index12Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index12Spells[i]);
                    Talent2Index12Content = $"{i + 1} / {Talent2Index12Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index12()
        {
            if (Talent2Index12CanRemove)
            {
                for (int i = Talent2Index12Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index12Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index12Spells[i]);
                        Talent2Index12Content = $"{i} / {Talent2Index12Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index13()
        {
            for (int i = 0; i < Talent2Index13Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index13Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index13Spells[i]);
                    Talent2Index13Content = $"{i + 1} / {Talent2Index13Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index13()
        {
            if (Talent2Index13CanRemove)
            {
                for (int i = Talent2Index13Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index13Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index13Spells[i]);
                        Talent2Index13Content = $"{i} / {Talent2Index13Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index14()
        {
            for (int i = 0; i < Talent2Index14Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index14Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index14Spells[i]);
                    Talent2Index14Content = $"{i + 1} / {Talent2Index14Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index14()
        {
            if (Talent2Index14CanRemove)
            {
                for (int i = Talent2Index14Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index14Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index14Spells[i]);
                        Talent2Index14Content = $"{i} / {Talent2Index14Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index15()
        {
            for (int i = 0; i < Talent2Index15Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index15Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index15Spells[i]);
                    Talent2Index15Content = $"{i + 1} / {Talent2Index15Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index15()
        {
            if (Talent2Index15CanRemove)
            {
                for (int i = Talent2Index15Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index15Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index15Spells[i]);
                        Talent2Index15Content = $"{i} / {Talent2Index15Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index16()
        {
            for (int i = 0; i < Talent2Index16Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index16Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index16Spells[i]);
                    Talent2Index16Content = $"{i + 1} / {Talent2Index16Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index16()
        {
            if (Talent2Index16CanRemove)
            {
                for (int i = Talent2Index16Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index16Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index16Spells[i]);
                        Talent2Index16Content = $"{i} / {Talent2Index16Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index17()
        {
            for (int i = 0; i < Talent2Index17Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index17Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index17Spells[i]);
                    Talent2Index17Content = $"{i + 1} / {Talent2Index17Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index17()
        {
            if (Talent2Index17CanRemove)
            {
                for (int i = Talent2Index17Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index17Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index17Spells[i]);
                        Talent2Index17Content = $"{i} / {Talent2Index17Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index18()
        {
            for (int i = 0; i < Talent2Index18Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index18Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index18Spells[i]);
                    Talent2Index18Content = $"{i + 1} / {Talent2Index18Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index18()
        {
            if (Talent2Index18CanRemove)
            {
                for (int i = Talent2Index18Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index18Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index18Spells[i]);
                        Talent2Index18Content = $"{i} / {Talent2Index18Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent2Index19()
        {
            for (int i = 0; i < Talent2Index19Spells.Count; i++)
            {
                if (!Talent2Spells.Contains(Talent2Index19Spells[i]))
                {
                    Talent2Spells.Add(Talent2Index19Spells[i]);
                    Talent2Index19Content = $"{i + 1} / {Talent2Index19Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent2Index19()
        {
            if (Talent2Index19CanRemove)
            {
                for (int i = Talent2Index19Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent2Spells.Contains(Talent2Index19Spells[i]))
                    {
                        Talent2Spells.Remove(Talent2Index19Spells[i]);
                        Talent2Index19Content = $"{i} / {Talent2Index19Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public ICommand Talent2Index1AddCommand => _talent2Index1AddCommand ??= new CommandHandler(AddTalent2Index1, true);
        public ICommand Talent2Index2AddCommand => _talent2Index2AddCommand ??= new CommandHandler(AddTalent2Index2, true);
        public ICommand Talent2Index3AddCommand => _talent2Index3AddCommand ??= new CommandHandler(AddTalent2Index3, true);
        public ICommand Talent2Index4AddCommand => _talent2Index4AddCommand ??= new CommandHandler(AddTalent2Index4, true);
        public ICommand Talent2Index5AddCommand => _talent2Index5AddCommand ??= new CommandHandler(AddTalent2Index5, true);
        public ICommand Talent2Index6AddCommand => _talent2Index6AddCommand ??= new CommandHandler(AddTalent2Index6, true);
        public ICommand Talent2Index7AddCommand => _talent2Index7AddCommand ??= new CommandHandler(AddTalent2Index7, true);
        public ICommand Talent2Index8AddCommand => _talent2Index8AddCommand ??= new CommandHandler(AddTalent2Index8, true);
        public ICommand Talent2Index9AddCommand => _talent2Index9AddCommand ??= new CommandHandler(AddTalent2Index9, true);
        public ICommand Talent2Index10AddCommand => _talent2Index10AddCommand ??= new CommandHandler(AddTalent2Index10, true);
        public ICommand Talent2Index11AddCommand => _talent2Index11AddCommand ??= new CommandHandler(AddTalent2Index11, true);
        public ICommand Talent2Index12AddCommand => _talent2Index12AddCommand ??= new CommandHandler(AddTalent2Index12, true);
        public ICommand Talent2Index13AddCommand => _talent2Index13AddCommand ??= new CommandHandler(AddTalent2Index13, true);
        public ICommand Talent2Index14AddCommand => _talent2Index14AddCommand ??= new CommandHandler(AddTalent2Index14, true);
        public ICommand Talent2Index15AddCommand => _talent2Index15AddCommand ??= new CommandHandler(AddTalent2Index15, true);
        public ICommand Talent2Index16AddCommand => _talent2Index16AddCommand ??= new CommandHandler(AddTalent2Index16, true);
        public ICommand Talent2Index17AddCommand => _talent2Index17AddCommand ??= new CommandHandler(AddTalent2Index17, true);
        public ICommand Talent2Index18AddCommand => _talent2Index18AddCommand ??= new CommandHandler(AddTalent2Index18, true);
        public ICommand Talent2Index19AddCommand => _talent2Index19AddCommand ??= new CommandHandler(AddTalent2Index19, true);

        public ICommand Talent2Index1RemoveCommand => _talent2Index1RemoveCommand ??= new CommandHandler(RemoveTalent2Index1, true);
        public ICommand Talent2Index2RemoveCommand => _talent2Index2RemoveCommand ??= new CommandHandler(RemoveTalent2Index2, true);
        public ICommand Talent2Index3RemoveCommand => _talent2Index3RemoveCommand ??= new CommandHandler(RemoveTalent2Index3, true);
        public ICommand Talent2Index4RemoveCommand => _talent2Index4RemoveCommand ??= new CommandHandler(RemoveTalent2Index4, true);
        public ICommand Talent2Index5RemoveCommand => _talent2Index5RemoveCommand ??= new CommandHandler(RemoveTalent2Index5, true);
        public ICommand Talent2Index6RemoveCommand => _talent2Index6RemoveCommand ??= new CommandHandler(RemoveTalent2Index6, true);
        public ICommand Talent2Index7RemoveCommand => _talent2Index7RemoveCommand ??= new CommandHandler(RemoveTalent2Index7, true);
        public ICommand Talent2Index8RemoveCommand => _talent2Index8RemoveCommand ??= new CommandHandler(RemoveTalent2Index8, true);
        public ICommand Talent2Index9RemoveCommand => _talent2Index9RemoveCommand ??= new CommandHandler(RemoveTalent2Index9, true);
        public ICommand Talent2Index10RemoveCommand => _talent2Index10RemoveCommand ??= new CommandHandler(RemoveTalent2Index10, true);
        public ICommand Talent2Index11RemoveCommand => _talent2Index11RemoveCommand ??= new CommandHandler(RemoveTalent2Index11, true);
        public ICommand Talent2Index12RemoveCommand => _talent2Index12RemoveCommand ??= new CommandHandler(RemoveTalent2Index12, true);
        public ICommand Talent2Index13RemoveCommand => _talent2Index13RemoveCommand ??= new CommandHandler(RemoveTalent2Index13, true);
        public ICommand Talent2Index14RemoveCommand => _talent2Index14RemoveCommand ??= new CommandHandler(RemoveTalent2Index14, true);
        public ICommand Talent2Index15RemoveCommand => _talent2Index15RemoveCommand ??= new CommandHandler(RemoveTalent2Index15, true);
        public ICommand Talent2Index16RemoveCommand => _talent2Index16RemoveCommand ??= new CommandHandler(RemoveTalent2Index16, true);
        public ICommand Talent2Index17RemoveCommand => _talent2Index17RemoveCommand ??= new CommandHandler(RemoveTalent2Index17, true);
        public ICommand Talent2Index18RemoveCommand => _talent2Index18RemoveCommand ??= new CommandHandler(RemoveTalent2Index18, true);
        public ICommand Talent2Index19RemoveCommand => _talent2Index19RemoveCommand ??= new CommandHandler(RemoveTalent2Index19, true);

        public List<int> Talent3Row1Spells = new List<int>();
        public List<int> Talent3Row2Spells = new List<int>();
        public List<int> Talent3Row3Spells = new List<int>();
        public List<int> Talent3Row4Spells = new List<int>();
        public List<int> Talent3Row5Spells = new List<int>();
        public List<int> Talent3Row6Spells = new List<int>();
        public List<int> Talent3Row7Spells = new List<int>();

        public List<int> Talent3Index1Spells = new List<int>();
        public List<int> Talent3Index2Spells = new List<int>();
        public List<int> Talent3Index3Spells = new List<int>();
        public List<int> Talent3Index4Spells = new List<int>();
        public List<int> Talent3Index5Spells = new List<int>();
        public List<int> Talent3Index6Spells = new List<int>();
        public List<int> Talent3Index7Spells = new List<int>();
        public List<int> Talent3Index8Spells = new List<int>();
        public List<int> Talent3Index9Spells = new List<int>();
        public List<int> Talent3Index10Spells = new List<int>();
        public List<int> Talent3Index11Spells = new List<int>();
        public List<int> Talent3Index12Spells = new List<int>();
        public List<int> Talent3Index13Spells = new List<int>();
        public List<int> Talent3Index14Spells = new List<int>();
        public List<int> Talent3Index15Spells = new List<int>();
        public List<int> Talent3Index16Spells = new List<int>();
        public List<int> Talent3Index17Spells = new List<int>();
        public List<int> Talent3Index18Spells = new List<int>();
        public List<int> Talent3Index19Spells = new List<int>();
        public void AddTalent3Index1()
        {
            for (int i = 0; i < Talent3Index1Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index1Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index1Spells[i]);
                    Talent3Index1Content = $"{i + 1} / {Talent3Index1Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }

        public void RemoveTalent3Index1()
        {
            if (Talent3Index1CanRemove)
            {
                for (int i = Talent3Index1Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index1Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index1Spells[i]);
                        Talent3Index1Content = $"{i} / {Talent3Index1Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index2()
        {
            for (int i = 0; i < Talent3Index2Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index2Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index2Spells[i]);
                    Talent3Index2Content = $"{i + 1} / {Talent3Index2Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index2()
        {
            if (Talent3Index2CanRemove)
            {
                for (int i = Talent3Index2Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index2Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index2Spells[i]);
                        Talent3Index2Content = $"{i} / {Talent3Index2Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }

        public void AddTalent3Index3()
        {
            for (int i = 0; i < Talent3Index3Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index3Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index3Spells[i]);
                    Talent3Index3Content = $"{i + 1} / {Talent3Index3Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index3()
        {
            if (Talent3Index3CanRemove)
            {
                for (int i = Talent3Index3Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index3Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index3Spells[i]);
                        Talent3Index3Content = $"{i} / {Talent3Index3Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index4()
        {
            for (int i = 0; i < Talent3Index4Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index4Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index4Spells[i]);
                    Talent3Index4Content = $"{i + 1} / {Talent3Index4Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index4()
        {
            if (Talent3Index4CanRemove)
            {
                for (int i = Talent3Index4Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index4Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index4Spells[i]);
                        Talent3Index4Content = $"{i} / {Talent3Index4Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index5()
        {
            for (int i = 0; i < Talent3Index5Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index5Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index5Spells[i]);
                    Talent3Index5Content = $"{i + 1} / {Talent3Index5Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index5()
        {
            if (Talent3Index5CanRemove)
            {
                for (int i = Talent3Index5Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index5Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index5Spells[i]);
                        Talent3Index5Content = $"{i} / {Talent3Index6Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index6()
        {
            for (int i = 0; i < Talent3Index6Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index6Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index6Spells[i]);
                    Talent3Index6Content = $"{i + 1} / {Talent3Index6Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index6()
        {
            if (Talent3Index6CanRemove)
            {
                for (int i = Talent3Index6Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index6Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index6Spells[i]);
                        Talent3Index7Content = $"{i} / {Talent3Index7Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index7()
        {
            for (int i = 0; i < Talent3Index7Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index7Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index7Spells[i]);
                    Talent3Index7Content = $"{i + 1} / {Talent3Index7Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index7()
        {
            if (Talent3Index7CanRemove)
            {
                for (int i = Talent3Index7Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index7Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index7Spells[i]);
                        Talent3Index7Content = $"{i} / {Talent3Index7Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index8()
        {
            for (int i = 0; i < Talent3Index8Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index8Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index8Spells[i]);
                    Talent3Index8Content = $"{i + 1} / {Talent3Index8Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index8()
        {
            if (Talent3Index8CanRemove)
            {
                for (int i = Talent3Index8Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index8Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index8Spells[i]);
                        Talent3Index8Content = $"{i} / {Talent3Index8Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index9()
        {
            for (int i = 0; i < Talent3Index9Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index9Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index9Spells[i]);
                    Talent3Index9Content = $"{i + 1} / {Talent3Index9Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index9()
        {
            if (Talent3Index9CanRemove)
            {
                for (int i = Talent3Index9Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index9Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index9Spells[i]);
                        Talent3Index9Content = $"{i} / {Talent3Index9Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index10()
        {
            for (int i = 0; i < Talent3Index10Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index10Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index10Spells[i]);
                    Talent3Index10Content = $"{i + 1} / {Talent3Index10Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index10()
        {
            if (Talent3Index10CanRemove)
            {
                for (int i = Talent3Index10Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index10Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index10Spells[i]);
                        Talent3Index10Content = $"{i} / {Talent3Index10Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index11()
        {
            for (int i = 0; i < Talent3Index11Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index11Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index11Spells[i]);
                    Talent3Index11Content = $"{i + 1} / {Talent3Index11Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index11()
        {
            if (Talent3Index11CanRemove)
            {
                for (int i = Talent3Index11Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index11Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index11Spells[i]);
                        Talent3Index11Content = $"{i} / {Talent3Index11Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index12()
        {
            for (int i = 0; i < Talent3Index12Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index12Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index12Spells[i]);
                    Talent3Index12Content = $"{i + 1} / {Talent3Index12Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index12()
        {
            if (Talent3Index12CanRemove)
            {
                for (int i = Talent3Index12Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index12Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index12Spells[i]);
                        Talent3Index12Content = $"{i} / {Talent3Index12Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index13()
        {
            for (int i = 0; i < Talent3Index13Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index13Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index13Spells[i]);
                    Talent3Index13Content = $"{i + 1} / {Talent3Index13Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index13()
        {
            if (Talent3Index13CanRemove)
            {
                for (int i = Talent3Index13Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index13Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index13Spells[i]);
                        Talent3Index13Content = $"{i} / {Talent3Index13Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index14()
        {
            for (int i = 0; i < Talent3Index14Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index14Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index14Spells[i]);
                    Talent3Index14Content = $"{i + 1} / {Talent3Index14Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index14()
        {
            if (Talent3Index14CanRemove)
            {
                for (int i = Talent3Index14Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index14Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index14Spells[i]);
                        Talent3Index14Content = $"{i} / {Talent3Index14Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index15()
        {
            for (int i = 0; i < Talent3Index15Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index15Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index15Spells[i]);
                    Talent3Index15Content = $"{i + 1} / {Talent3Index15Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index15()
        {
            if (Talent3Index15CanRemove)
            {
                for (int i = Talent3Index15Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index15Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index15Spells[i]);
                        Talent3Index15Content = $"{i} / {Talent3Index15Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index16()
        {
            for (int i = 0; i < Talent3Index16Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index16Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index16Spells[i]);
                    Talent3Index16Content = $"{i + 1} / {Talent3Index16Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index16()
        {
            if (Talent3Index16CanRemove)
            {
                for (int i = Talent3Index16Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index16Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index16Spells[i]);
                        Talent3Index16Content = $"{i} / {Talent3Index16Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index17()
        {
            for (int i = 0; i < Talent3Index17Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index17Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index17Spells[i]);
                    Talent3Index17Content = $"{i + 1} / {Talent3Index17Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index17()
        {
            if (Talent3Index17CanRemove)
            {
                for (int i = Talent3Index17Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index17Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index17Spells[i]);
                        Talent3Index17Content = $"{i} / {Talent3Index17Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public void AddTalent3Index18()
        {
            for (int i = 0; i < Talent3Index18Spells.Count; i++)
            {
                if (!Talent3Spells.Contains(Talent3Index18Spells[i]))
                {
                    Talent3Spells.Add(Talent3Index18Spells[i]);
                    Talent3Index18Content = $"{i + 1} / {Talent3Index18Spells.Count}";
                    break;
                }
            }
            RefreshButtonEnables();
        }
        public void RemoveTalent3Index18()
        {
            if (Talent3Index18CanRemove)
            {
                for (int i = Talent3Index18Spells.Count - 1; i >= 0; i--)
                {
                    if (Talent3Spells.Contains(Talent3Index18Spells[i]))
                    {
                        Talent3Spells.Remove(Talent3Index18Spells[i]);
                        Talent3Index18Content = $"{i} / {Talent3Index18Spells.Count}";
                        break;
                    }
                }
            }
            RefreshButtonEnables();
        }
        public ICommand Talent3Index1AddCommand => _talent2Index1AddCommand ??= new CommandHandler(AddTalent3Index1, true);
        public ICommand Talent3Index2AddCommand => _talent2Index2AddCommand ??= new CommandHandler(AddTalent3Index2, true);
        public ICommand Talent3Index3AddCommand => _talent2Index3AddCommand ??= new CommandHandler(AddTalent3Index3, true);
        public ICommand Talent3Index4AddCommand => _talent2Index4AddCommand ??= new CommandHandler(AddTalent3Index4, true);
        public ICommand Talent3Index5AddCommand => _talent2Index5AddCommand ??= new CommandHandler(AddTalent3Index5, true);
        public ICommand Talent3Index6AddCommand => _talent2Index6AddCommand ??= new CommandHandler(AddTalent3Index6, true);
        public ICommand Talent3Index7AddCommand => _talent2Index7AddCommand ??= new CommandHandler(AddTalent3Index7, true);
        public ICommand Talent3Index8AddCommand => _talent2Index8AddCommand ??= new CommandHandler(AddTalent3Index8, true);
        public ICommand Talent3Index9AddCommand => _talent2Index9AddCommand ??= new CommandHandler(AddTalent3Index9, true);
        public ICommand Talent3Index10AddCommand => _talent2Index10AddCommand ??= new CommandHandler(AddTalent3Index10, true);
        public ICommand Talent3Index11AddCommand => _talent2Index11AddCommand ??= new CommandHandler(AddTalent3Index11, true);
        public ICommand Talent3Index12AddCommand => _talent2Index12AddCommand ??= new CommandHandler(AddTalent3Index12, true);
        public ICommand Talent3Index13AddCommand => _talent2Index13AddCommand ??= new CommandHandler(AddTalent3Index13, true);
        public ICommand Talent3Index14AddCommand => _talent2Index14AddCommand ??= new CommandHandler(AddTalent3Index14, true);
        public ICommand Talent3Index15AddCommand => _talent2Index15AddCommand ??= new CommandHandler(AddTalent3Index15, true);
        public ICommand Talent3Index16AddCommand => _talent2Index16AddCommand ??= new CommandHandler(AddTalent3Index16, true);
        public ICommand Talent3Index17AddCommand => _talent2Index17AddCommand ??= new CommandHandler(AddTalent3Index17, true);
        public ICommand Talent3Index18AddCommand => _talent2Index18AddCommand ??= new CommandHandler(AddTalent3Index18, true);

        public ICommand Talent3Index1RemoveCommand => _talent2Index1RemoveCommand ??= new CommandHandler(RemoveTalent3Index1, true);
        public ICommand Talent3Index2RemoveCommand => _talent2Index2RemoveCommand ??= new CommandHandler(RemoveTalent3Index2, true);
        public ICommand Talent3Index3RemoveCommand => _talent2Index3RemoveCommand ??= new CommandHandler(RemoveTalent3Index3, true);
        public ICommand Talent3Index4RemoveCommand => _talent2Index4RemoveCommand ??= new CommandHandler(RemoveTalent3Index4, true);
        public ICommand Talent3Index5RemoveCommand => _talent2Index5RemoveCommand ??= new CommandHandler(RemoveTalent3Index5, true);
        public ICommand Talent3Index6RemoveCommand => _talent2Index6RemoveCommand ??= new CommandHandler(RemoveTalent3Index6, true);
        public ICommand Talent3Index7RemoveCommand => _talent2Index7RemoveCommand ??= new CommandHandler(RemoveTalent3Index7, true);
        public ICommand Talent3Index8RemoveCommand => _talent2Index8RemoveCommand ??= new CommandHandler(RemoveTalent3Index8, true);
        public ICommand Talent3Index9RemoveCommand => _talent2Index9RemoveCommand ??= new CommandHandler(RemoveTalent3Index9, true);
        public ICommand Talent3Index10RemoveCommand => _talent2Index10RemoveCommand ??= new CommandHandler(RemoveTalent3Index10, true);
        public ICommand Talent3Index11RemoveCommand => _talent2Index11RemoveCommand ??= new CommandHandler(RemoveTalent3Index11, true);
        public ICommand Talent3Index12RemoveCommand => _talent2Index12RemoveCommand ??= new CommandHandler(RemoveTalent3Index12, true);
        public ICommand Talent3Index13RemoveCommand => _talent2Index13RemoveCommand ??= new CommandHandler(RemoveTalent3Index13, true);
        public ICommand Talent3Index14RemoveCommand => _talent2Index14RemoveCommand ??= new CommandHandler(RemoveTalent3Index14, true);
        public ICommand Talent3Index15RemoveCommand => _talent2Index15RemoveCommand ??= new CommandHandler(RemoveTalent3Index15, true);
        public ICommand Talent3Index16RemoveCommand => _talent2Index16RemoveCommand ??= new CommandHandler(RemoveTalent3Index16, true);
        public ICommand Talent3Index17RemoveCommand => _talent2Index17RemoveCommand ??= new CommandHandler(RemoveTalent3Index17, true);
        public ICommand Talent3Index18RemoveCommand => _talent2Index18RemoveCommand ??= new CommandHandler(RemoveTalent3Index18, true);
       
        private string _talent1Index1Content;
        private string _talent1Index2Content;
        private string _talent1Index3Content;
        private string _talent1Index4Content;
        private string _talent1Index5Content;
        private string _talent1Index6Content;
        private string _talent1Index7Content;
        private string _talent1Index8Content;
        private string _talent1Index9Content;
        private string _talent1Index10Content;
        private string _talent1Index11Content;
        private string _talent1Index12Content;
        private string _talent1Index13Content;
        private string _talent1Index14Content;
        private string _talent1Index15Content;
        private string _talent1Index16Content;
        private string _talent1Index17Content;
        private string _talent1Index18Content;
        public string Talent1Index1Content { get => _talent1Index1Content; set { _talent1Index1Content = value; OnPropertyChanged(nameof(Talent1Index1Content)); } }
        public string Talent1Index2Content{ get => _talent1Index2Content; set { _talent1Index2Content = value; OnPropertyChanged(nameof(Talent1Index2Content)); } }
        public string Talent1Index3Content{ get => _talent1Index3Content; set { _talent1Index3Content = value; OnPropertyChanged(nameof(Talent1Index3Content)); } }
        public string Talent1Index4Content{ get => _talent1Index4Content; set { _talent1Index4Content = value; OnPropertyChanged(nameof(Talent1Index4Content)); } }
        public string Talent1Index5Content{ get => _talent1Index5Content; set { _talent1Index5Content = value; OnPropertyChanged(nameof(Talent1Index5Content)); } }
        public string Talent1Index6Content{ get => _talent1Index6Content; set { _talent1Index6Content = value; OnPropertyChanged(nameof(Talent1Index6Content)); } }
        public string Talent1Index7Content{ get => _talent1Index7Content; set { _talent1Index7Content = value; OnPropertyChanged(nameof(Talent1Index7Content)); } }
        public string Talent1Index8Content{ get => _talent1Index8Content; set { _talent1Index8Content = value; OnPropertyChanged(nameof(Talent1Index8Content)); } }
        public string Talent1Index9Content{ get => _talent1Index9Content; set { _talent1Index9Content = value; OnPropertyChanged(nameof(Talent1Index9Content)); } }
        public string Talent1Index10Content{ get => _talent1Index10Content; set { _talent1Index10Content = value; OnPropertyChanged(nameof(Talent1Index10Content)); } }
        public string Talent1Index11Content{ get => _talent1Index11Content; set { _talent1Index11Content = value; OnPropertyChanged(nameof(Talent1Index11Content)); } }
        public string Talent1Index12Content{ get => _talent1Index12Content; set { _talent1Index12Content = value; OnPropertyChanged(nameof(Talent1Index12Content)); } }
        public string Talent1Index13Content{ get => _talent1Index13Content; set { _talent1Index13Content = value; OnPropertyChanged(nameof(Talent1Index13Content)); } }
        public string Talent1Index14Content{ get => _talent1Index14Content; set { _talent1Index14Content = value; OnPropertyChanged(nameof(Talent1Index14Content)); } }
        public string Talent1Index15Content{ get => _talent1Index15Content; set { _talent1Index15Content = value; OnPropertyChanged(nameof(Talent1Index15Content)); } }
        public string Talent1Index16Content{ get => _talent1Index16Content; set { _talent1Index16Content = value; OnPropertyChanged(nameof(Talent1Index16Content)); } }
        public string Talent1Index17Content{ get => _talent1Index17Content; set { _talent1Index17Content = value; OnPropertyChanged(nameof(Talent1Index17Content)); } }
        public string Talent1Index18Content{ get => _talent1Index18Content; set { _talent1Index18Content = value; OnPropertyChanged(nameof(Talent1Index18Content)); } }
        public virtual bool Talent1Index1Enabled => true;
        public virtual bool Talent1Index2Enabled => true;
        public virtual bool Talent1Index3Enabled => true;
        public virtual bool Talent1Index4Enabled => true;
        public virtual bool Talent1Index5Enabled => true;
        public virtual bool Talent1Index6Enabled => true;
        public virtual bool Talent1Index7Enabled => true;
        public virtual bool Talent1Index8Enabled => true;
        public virtual bool Talent1Index9Enabled => true;
        public virtual bool Talent1Index10Enabled => true;
        public virtual bool Talent1Index11Enabled => true;
        public virtual bool Talent1Index12Enabled => true;
        public virtual bool Talent1Index13Enabled => true;
        public virtual bool Talent1Index14Enabled => true;
        public virtual bool Talent1Index15Enabled => true;
        public virtual bool Talent1Index16Enabled => true;
        public virtual bool Talent1Index17Enabled => true;
        public virtual bool Talent1Index18Enabled => true;
        public virtual bool Talent1Index1CanRemove => true;
        public virtual bool Talent1Index2CanRemove => true;
        public virtual bool Talent1Index3CanRemove => true;
        public virtual bool Talent1Index4CanRemove => true;
        public virtual bool Talent1Index5CanRemove => true;
        public virtual bool Talent1Index6CanRemove => true;
        public virtual bool Talent1Index7CanRemove => true;
        public virtual bool Talent1Index8CanRemove => true;
        public virtual bool Talent1Index9CanRemove => true;
        public virtual bool Talent1Index10CanRemove => true;
        public virtual bool Talent1Index11CanRemove => true;
        public virtual bool Talent1Index12CanRemove => true;
        public virtual bool Talent1Index13CanRemove => true;
        public virtual bool Talent1Index14CanRemove => true;
        public virtual bool Talent1Index15CanRemove => true;
        public virtual bool Talent1Index16CanRemove => true;
        public virtual bool Talent1Index17CanRemove => true;
        public virtual bool Talent1Index18CanRemove => true;
        public virtual bool Talent2Index1Enabled => true;
        public virtual bool Talent2Index2Enabled => true;
        public virtual bool Talent2Index3Enabled => true;
        public virtual bool Talent2Index4Enabled => true;
        public virtual bool Talent2Index5Enabled => true;
        public virtual bool Talent2Index6Enabled => true;
        public virtual bool Talent2Index7Enabled => true;
        public virtual bool Talent2Index8Enabled => true;
        public virtual bool Talent2Index9Enabled => true;
        public virtual bool Talent2Index10Enabled => true;
        public virtual bool Talent2Index11Enabled => true;
        public virtual bool Talent2Index12Enabled => true;
        public virtual bool Talent2Index13Enabled => true;
        public virtual bool Talent2Index14Enabled => true;
        public virtual bool Talent2Index15Enabled => true;
        public virtual bool Talent2Index16Enabled => true;
        public virtual bool Talent2Index17Enabled => true;
        public virtual bool Talent2Index18Enabled => true;
        public virtual bool Talent2Index19Enabled => true;
        public virtual bool Talent2Index1CanRemove => true;
        public virtual bool Talent2Index2CanRemove => true;
        public virtual bool Talent2Index3CanRemove => true;
        public virtual bool Talent2Index4CanRemove => true;
        public virtual bool Talent2Index5CanRemove => true;
        public virtual bool Talent2Index6CanRemove => true;
        public virtual bool Talent2Index7CanRemove => true;
        public virtual bool Talent2Index8CanRemove => true;
        public virtual bool Talent2Index9CanRemove => true;
        public virtual bool Talent2Index10CanRemove => true;
        public virtual bool Talent2Index11CanRemove => true;
        public virtual bool Talent2Index12CanRemove => true;
        public virtual bool Talent2Index13CanRemove => true;
        public virtual bool Talent2Index14CanRemove => true;
        public virtual bool Talent2Index15CanRemove => true;
        public virtual bool Talent2Index16CanRemove => true;
        public virtual bool Talent2Index17CanRemove => true;
        public virtual bool Talent2Index18CanRemove => true;
        public virtual bool Talent2Index19CanRemove => true;
        public virtual bool Talent3Index1Enabled => true;
        public virtual bool Talent3Index2Enabled => true;
        public virtual bool Talent3Index3Enabled => true;
        public virtual bool Talent3Index4Enabled => true;
        public virtual bool Talent3Index5Enabled => true;
        public virtual bool Talent3Index6Enabled => true;
        public virtual bool Talent3Index7Enabled => true;
        public virtual bool Talent3Index8Enabled => true;
        public virtual bool Talent3Index9Enabled => true;
        public virtual bool Talent3Index10Enabled => true;
        public virtual bool Talent3Index11Enabled => true;
        public virtual bool Talent3Index12Enabled => true;
        public virtual bool Talent3Index13Enabled => true;
        public virtual bool Talent3Index14Enabled => true;
        public virtual bool Talent3Index15Enabled => true;
        public virtual bool Talent3Index16Enabled => true;
        public virtual bool Talent3Index17Enabled => true;
        public virtual bool Talent3Index18Enabled => true;
        public virtual bool Talent3Index1CanRemove => true;
        public virtual bool Talent3Index2CanRemove => true;
        public virtual bool Talent3Index3CanRemove => true;
        public virtual bool Talent3Index4CanRemove => true;
        public virtual bool Talent3Index5CanRemove => true;
        public virtual bool Talent3Index6CanRemove => true;
        public virtual bool Talent3Index7CanRemove => true;
        public virtual bool Talent3Index8CanRemove => true;
        public virtual bool Talent3Index9CanRemove => true;
        public virtual bool Talent3Index10CanRemove => true;
        public virtual bool Talent3Index11CanRemove => true;
        public virtual bool Talent3Index12CanRemove => true;
        public virtual bool Talent3Index13CanRemove => true;
        public virtual bool Talent3Index14CanRemove => true;
        public virtual bool Talent3Index15CanRemove => true;
        public virtual bool Talent3Index16CanRemove => true;
        public virtual bool Talent3Index17CanRemove => true;
        public virtual bool Talent3Index18CanRemove => true; 
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

        private string _talent2Index1Content;
        private string _talent2Index2Content;
        private string _talent2Index3Content;
        private string _talent2Index4Content;
        private string _talent2Index5Content;
        private string _talent2Index6Content;
        private string _talent2Index7Content;
        private string _talent2Index8Content;
        private string _talent2Index9Content;
        private string _talent2Index10Content;
        private string _talent2Index11Content;
        private string _talent2Index12Content;
        private string _talent2Index13Content;
        private string _talent2Index14Content;
        private string _talent2Index15Content;
        private string _talent2Index16Content;
        private string _talent2Index17Content;
        private string _talent2Index18Content;
        private string _talent2Index19Content;
        public string Talent2Index1Content { get => _talent2Index1Content; set { _talent2Index1Content = value; OnPropertyChanged(nameof(Talent2Index1Content)); } }
        public string Talent2Index2Content { get => _talent2Index2Content; set { _talent2Index2Content = value; OnPropertyChanged(nameof(Talent2Index2Content)); } }
        public string Talent2Index3Content { get => _talent2Index3Content; set { _talent2Index3Content = value; OnPropertyChanged(nameof(Talent2Index3Content)); } }
        public string Talent2Index4Content { get => _talent2Index4Content; set { _talent2Index4Content = value; OnPropertyChanged(nameof(Talent2Index4Content)); } }
        public string Talent2Index5Content { get => _talent2Index5Content; set { _talent2Index5Content = value; OnPropertyChanged(nameof(Talent2Index5Content)); } }
        public string Talent2Index6Content { get => _talent2Index6Content; set { _talent2Index6Content = value; OnPropertyChanged(nameof(Talent2Index6Content)); } }
        public string Talent2Index7Content { get => _talent2Index7Content; set { _talent2Index7Content = value; OnPropertyChanged(nameof(Talent2Index7Content)); } }
        public string Talent2Index8Content { get => _talent2Index8Content; set { _talent2Index8Content = value; OnPropertyChanged(nameof(Talent2Index8Content)); } }
        public string Talent2Index9Content { get => _talent2Index9Content; set { _talent2Index9Content = value; OnPropertyChanged(nameof(Talent2Index9Content)); } }
        public string Talent2Index10Content { get => _talent2Index10Content; set { _talent2Index10Content = value; OnPropertyChanged(nameof(Talent2Index10Content)); } }
        public string Talent2Index11Content { get => _talent2Index11Content; set { _talent2Index11Content = value; OnPropertyChanged(nameof(Talent2Index11Content)); } }
        public string Talent2Index12Content { get => _talent2Index12Content; set { _talent2Index12Content = value; OnPropertyChanged(nameof(Talent2Index12Content)); } }
        public string Talent2Index13Content { get => _talent2Index13Content; set { _talent2Index13Content = value; OnPropertyChanged(nameof(Talent2Index13Content)); } }
        public string Talent2Index14Content { get => _talent2Index14Content; set { _talent2Index14Content = value; OnPropertyChanged(nameof(Talent2Index14Content)); } }
        public string Talent2Index15Content { get => _talent2Index15Content; set { _talent2Index15Content = value; OnPropertyChanged(nameof(Talent2Index15Content)); } }
        public string Talent2Index16Content { get => _talent2Index16Content; set { _talent2Index16Content = value; OnPropertyChanged(nameof(Talent2Index16Content)); } }
        public string Talent2Index17Content { get => _talent2Index17Content; set { _talent2Index17Content = value; OnPropertyChanged(nameof(Talent2Index17Content)); } }
        public string Talent2Index18Content { get => _talent2Index18Content; set { _talent2Index18Content = value; OnPropertyChanged(nameof(Talent2Index18Content)); } }
        public string Talent2Index19Content { get => _talent2Index19Content; set { _talent2Index19Content = value; OnPropertyChanged(nameof(Talent2Index19Content)); } }
        public int Talent2Index1Column { get => _talent2Index1Column; set { _talent2Index1Column = value; OnPropertyChanged(nameof(Talent2Index1Column)); } }
        public int Talent2Index2Column { get => _talent2Index2Column; set { _talent2Index2Column = value; OnPropertyChanged(nameof(Talent2Index2Column)); } }
        public int Talent2Index3Column { get => _talent2Index3Column; set { _talent2Index3Column = value; OnPropertyChanged(nameof(Talent2Index3Column)); } }
        public int Talent2Index4Column { get => _talent2Index4Column; set { _talent2Index4Column = value; OnPropertyChanged(nameof(Talent2Index4Column)); } }
        public int Talent2Index5Column { get => _talent2Index5Column; set { _talent2Index5Column = value; OnPropertyChanged(nameof(Talent2Index5Column)); } }
        public int Talent2Index6Column { get => _talent2Index6Column; set { _talent2Index6Column = value; OnPropertyChanged(nameof(Talent2Index6Column)); } }
        public int Talent2Index7Column { get => _talent2Index7Column; set { _talent2Index7Column = value; OnPropertyChanged(nameof(Talent2Index7Column)); } }
        public int Talent2Index8Column { get => _talent2Index8Column; set { _talent2Index8Column = value; OnPropertyChanged(nameof(Talent2Index8Column)); } }
        public int Talent2Index9Column { get => _talent2Index9Column; set { _talent2Index9Column = value; OnPropertyChanged(nameof(Talent2Index9Column)); } }
        public int Talent2Index10Column { get => _talent2Index10Column; set { _talent2Index10Column = value; OnPropertyChanged(nameof(Talent2Index10Column)); } }
        public int Talent2Index11Column { get => _talent2Index11Column; set { _talent2Index11Column = value; OnPropertyChanged(nameof(Talent2Index11Column)); } }
        public int Talent2Index12Column { get => _talent2Index12Column; set { _talent2Index12Column = value; OnPropertyChanged(nameof(Talent2Index12Column)); } }
        public int Talent2Index13Column { get => _talent2Index13Column; set { _talent2Index13Column = value; OnPropertyChanged(nameof(Talent2Index13Column)); } }
        public int Talent2Index14Column { get => _talent2Index14Column; set { _talent2Index14Column = value; OnPropertyChanged(nameof(Talent2Index14Column)); } }
        public int Talent2Index15Column { get => _talent2Index15Column; set { _talent2Index15Column = value; OnPropertyChanged(nameof(Talent2Index15Column)); } }
        public int Talent2Index16Column { get => _talent2Index16Column; set { _talent2Index16Column = value; OnPropertyChanged(nameof(Talent2Index16Column)); } }
        public int Talent2Index17Column { get => _talent2Index17Column; set { _talent2Index17Column = value; OnPropertyChanged(nameof(Talent2Index17Column)); } }
        public int Talent2Index18Column { get => _talent2Index18Column; set { _talent2Index18Column = value; OnPropertyChanged(nameof(Talent2Index18Column)); } }
        public int Talent2Index19Column { get => _talent2Index19Column; set { _talent2Index19Column = value; OnPropertyChanged(nameof(Talent2Index19Column)); } }
        public int Talent2Index1Row { get => _talent2Index1Row; set { _talent2Index1Row = value; OnPropertyChanged(nameof(Talent2Index1Row)); } }
        public int Talent2Index2Row { get => _talent2Index2Row; set { _talent2Index2Row = value; OnPropertyChanged(nameof(Talent2Index2Row)); } }
        public int Talent2Index3Row { get => _talent2Index3Row; set { _talent2Index3Row = value; OnPropertyChanged(nameof(Talent2Index3Row)); } }
        public int Talent2Index4Row { get => _talent2Index4Row; set { _talent2Index4Row = value; OnPropertyChanged(nameof(Talent2Index4Row)); } }
        public int Talent2Index5Row { get => _talent2Index5Row; set { _talent2Index5Row = value; OnPropertyChanged(nameof(Talent2Index5Row)); } }
        public int Talent2Index6Row { get => _talent2Index6Row; set { _talent2Index6Row = value; OnPropertyChanged(nameof(Talent2Index6Row)); } }
        public int Talent2Index7Row { get => _talent2Index7Row; set { _talent2Index7Row = value; OnPropertyChanged(nameof(Talent2Index7Row)); } }
        public int Talent2Index8Row { get => _talent2Index8Row; set { _talent2Index8Row = value; OnPropertyChanged(nameof(Talent2Index8Row)); } }
        public int Talent2Index9Row { get => _talent2Index9Row; set { _talent2Index9Row = value; OnPropertyChanged(nameof(Talent2Index9Row)); } }
        public int Talent2Index10Row { get => _talent2Index10Row; set { _talent2Index10Row = value; OnPropertyChanged(nameof(Talent2Index10Row)); } }
        public int Talent2Index11Row { get => _talent2Index11Row; set { _talent2Index11Row = value; OnPropertyChanged(nameof(Talent2Index11Row)); } }
        public int Talent2Index12Row { get => _talent2Index12Row; set { _talent2Index12Row = value; OnPropertyChanged(nameof(Talent2Index12Row)); } }
        public int Talent2Index13Row { get => _talent2Index13Row; set { _talent2Index13Row = value; OnPropertyChanged(nameof(Talent2Index13Row)); } }
        public int Talent2Index14Row { get => _talent2Index14Row; set { _talent2Index14Row = value; OnPropertyChanged(nameof(Talent2Index14Row)); } }
        public int Talent2Index15Row { get => _talent2Index15Row; set { _talent2Index15Row = value; OnPropertyChanged(nameof(Talent2Index15Row)); } }
        public int Talent2Index16Row { get => _talent2Index16Row; set { _talent2Index16Row = value; OnPropertyChanged(nameof(Talent2Index16Row)); } }
        public int Talent2Index17Row { get => _talent2Index17Row; set { _talent2Index17Row = value; OnPropertyChanged(nameof(Talent2Index17Row)); } }
        public int Talent2Index18Row { get => _talent2Index18Row; set { _talent2Index18Row = value; OnPropertyChanged(nameof(Talent2Index18Row)); } }
        public int Talent2Index19Row { get => _talent2Index19Row; set { _talent2Index19Row = value; OnPropertyChanged(nameof(Talent2Index19Row)); } }
       
        private string _talent3Index1Content;
        private string _talent3Index2Content;
        private string _talent3Index3Content;
        private string _talent3Index4Content;
        private string _talent3Index5Content;
        private string _talent3Index6Content;
        private string _talent3Index7Content;
        private string _talent3Index8Content;
        private string _talent3Index9Content;
        private string _talent3Index10Content;
        private string _talent3Index11Content;
        private string _talent3Index12Content;
        private string _talent3Index13Content;
        private string _talent3Index14Content;
        private string _talent3Index15Content;
        private string _talent3Index16Content;
        private string _talent3Index17Content;
        private string _talent3Index18Content;
        public string Talent3Index1Content { get => _talent3Index1Content; set { _talent3Index1Content = value; OnPropertyChanged(nameof(Talent3Index1Content)); } }
        public string Talent3Index2Content { get => _talent3Index2Content; set { _talent3Index2Content = value; OnPropertyChanged(nameof(Talent3Index2Content)); } }
        public string Talent3Index3Content { get => _talent3Index3Content; set { _talent3Index3Content = value; OnPropertyChanged(nameof(Talent3Index3Content)); } }
        public string Talent3Index4Content { get => _talent3Index4Content; set { _talent3Index4Content = value; OnPropertyChanged(nameof(Talent3Index4Content)); } }
        public string Talent3Index5Content { get => _talent3Index5Content; set { _talent3Index5Content = value; OnPropertyChanged(nameof(Talent3Index5Content)); } }
        public string Talent3Index6Content { get => _talent3Index6Content; set { _talent3Index6Content = value; OnPropertyChanged(nameof(Talent3Index6Content)); } }
        public string Talent3Index7Content { get => _talent3Index7Content; set { _talent3Index7Content = value; OnPropertyChanged(nameof(Talent3Index7Content)); } }
        public string Talent3Index8Content { get => _talent3Index8Content; set { _talent3Index8Content = value; OnPropertyChanged(nameof(Talent3Index8Content)); } }
        public string Talent3Index9Content { get => _talent3Index9Content; set { _talent3Index9Content = value; OnPropertyChanged(nameof(Talent3Index9Content)); } }
        public string Talent3Index10Content { get => _talent3Index10Content; set { _talent3Index10Content = value; OnPropertyChanged(nameof(Talent3Index10Content)); } }
        public string Talent3Index11Content { get => _talent3Index11Content; set { _talent3Index11Content = value; OnPropertyChanged(nameof(Talent3Index11Content)); } }
        public string Talent3Index12Content { get => _talent3Index12Content; set { _talent3Index12Content = value; OnPropertyChanged(nameof(Talent3Index12Content)); } }
        public string Talent3Index13Content { get => _talent3Index13Content; set { _talent3Index13Content = value; OnPropertyChanged(nameof(Talent3Index13Content)); } }
        public string Talent3Index14Content { get => _talent3Index14Content; set { _talent3Index14Content = value; OnPropertyChanged(nameof(Talent3Index14Content)); } }
        public string Talent3Index15Content { get => _talent3Index15Content; set { _talent3Index15Content = value; OnPropertyChanged(nameof(Talent3Index15Content)); } }
        public string Talent3Index16Content { get => _talent3Index16Content; set { _talent3Index16Content = value; OnPropertyChanged(nameof(Talent3Index16Content)); } }
        public string Talent3Index17Content { get => _talent3Index17Content; set { _talent3Index17Content = value; OnPropertyChanged(nameof(Talent3Index17Content)); } }
        public string Talent3Index18Content { get => _talent3Index18Content; set { _talent3Index18Content = value; OnPropertyChanged(nameof(Talent3Index18Content)); } }
        public int Talent3Index1Column { get => _talent3Index1Column; set { _talent3Index1Column = value; OnPropertyChanged(nameof(Talent3Index1Column)); } }
        public int Talent3Index2Column { get => _talent3Index2Column; set { _talent3Index2Column = value; OnPropertyChanged(nameof(Talent3Index2Column)); } }
        public int Talent3Index3Column { get => _talent3Index3Column; set { _talent3Index3Column = value; OnPropertyChanged(nameof(Talent3Index3Column)); } }
        public int Talent3Index4Column { get => _talent3Index4Column; set { _talent3Index4Column = value; OnPropertyChanged(nameof(Talent3Index4Column)); } }
        public int Talent3Index5Column { get => _talent3Index5Column; set { _talent3Index5Column = value; OnPropertyChanged(nameof(Talent3Index5Column)); } }
        public int Talent3Index6Column { get => _talent3Index6Column; set { _talent3Index6Column = value; OnPropertyChanged(nameof(Talent3Index6Column)); } }
        public int Talent3Index7Column { get => _talent3Index7Column; set { _talent3Index7Column = value; OnPropertyChanged(nameof(Talent3Index7Column)); } }
        public int Talent3Index8Column { get => _talent3Index8Column; set { _talent3Index8Column = value; OnPropertyChanged(nameof(Talent3Index8Column)); } }
        public int Talent3Index9Column { get => _talent3Index9Column; set { _talent3Index9Column = value; OnPropertyChanged(nameof(Talent3Index9Column)); } }
        public int Talent3Index10Column { get => _talent3Index10Column; set { _talent3Index10Column = value; OnPropertyChanged(nameof(Talent3Index10Column)); } }
        public int Talent3Index11Column { get => _talent3Index11Column; set { _talent3Index11Column = value; OnPropertyChanged(nameof(Talent3Index11Column)); } }
        public int Talent3Index12Column { get => _talent3Index12Column; set { _talent3Index12Column = value; OnPropertyChanged(nameof(Talent3Index12Column)); } }
        public int Talent3Index13Column { get => _talent3Index13Column; set { _talent3Index13Column = value; OnPropertyChanged(nameof(Talent3Index13Column)); } }
        public int Talent3Index14Column { get => _talent3Index14Column; set { _talent3Index14Column = value; OnPropertyChanged(nameof(Talent3Index14Column)); } }
        public int Talent3Index15Column { get => _talent3Index15Column; set { _talent3Index15Column = value; OnPropertyChanged(nameof(Talent3Index15Column)); } }
        public int Talent3Index16Column { get => _talent3Index16Column; set { _talent3Index16Column = value; OnPropertyChanged(nameof(Talent3Index16Column)); } }
        public int Talent3Index17Column { get => _talent3Index17Column; set { _talent3Index17Column = value; OnPropertyChanged(nameof(Talent3Index17Column)); } }
        public int Talent3Index18Column { get => _talent3Index18Column; set { _talent3Index18Column = value; OnPropertyChanged(nameof(Talent3Index18Column)); } }
        public int Talent3Index1Row { get => _talent3Index1Row; set { _talent3Index1Row = value; OnPropertyChanged(nameof(Talent3Index1Row)); } }
        public int Talent3Index2Row { get => _talent3Index2Row; set { _talent3Index2Row = value; OnPropertyChanged(nameof(Talent3Index2Row)); } }
        public int Talent3Index3Row { get => _talent3Index3Row; set { _talent3Index3Row = value; OnPropertyChanged(nameof(Talent3Index3Row)); } }
        public int Talent3Index4Row { get => _talent3Index4Row; set { _talent3Index4Row = value; OnPropertyChanged(nameof(Talent3Index4Row)); } }
        public int Talent3Index5Row { get => _talent3Index5Row; set { _talent3Index5Row = value; OnPropertyChanged(nameof(Talent3Index5Row)); } }
        public int Talent3Index6Row { get => _talent3Index6Row; set { _talent3Index6Row = value; OnPropertyChanged(nameof(Talent3Index6Row)); } }
        public int Talent3Index7Row { get => _talent3Index7Row; set { _talent3Index7Row = value; OnPropertyChanged(nameof(Talent3Index7Row)); } }
        public int Talent3Index8Row { get => _talent3Index8Row; set { _talent3Index8Row = value; OnPropertyChanged(nameof(Talent3Index8Row)); } }
        public int Talent3Index9Row { get => _talent3Index9Row; set { _talent3Index9Row = value; OnPropertyChanged(nameof(Talent3Index9Row)); } }
        public int Talent3Index10Row { get => _talent3Index10Row; set { _talent3Index10Row = value; OnPropertyChanged(nameof(Talent3Index10Row)); } }
        public int Talent3Index11Row { get => _talent3Index11Row; set { _talent3Index11Row = value; OnPropertyChanged(nameof(Talent3Index11Row)); } }
        public int Talent3Index12Row { get => _talent3Index12Row; set { _talent3Index12Row = value; OnPropertyChanged(nameof(Talent3Index12Row)); } }
        public int Talent3Index13Row { get => _talent3Index13Row; set { _talent3Index13Row = value; OnPropertyChanged(nameof(Talent3Index13Row)); } }
        public int Talent3Index14Row { get => _talent3Index14Row; set { _talent3Index14Row = value; OnPropertyChanged(nameof(Talent3Index14Row)); } }
        public int Talent3Index15Row { get => _talent3Index15Row; set { _talent3Index15Row = value; OnPropertyChanged(nameof(Talent3Index15Row)); } }
        public int Talent3Index16Row { get => _talent3Index16Row; set { _talent3Index16Row = value; OnPropertyChanged(nameof(Talent3Index16Row)); } }
        public int Talent3Index17Row { get => _talent3Index17Row; set { _talent3Index17Row = value; OnPropertyChanged(nameof(Talent3Index17Row)); } }
        public int Talent3Index18Row { get => _talent3Index18Row; set { _talent3Index18Row = value; OnPropertyChanged(nameof(Talent3Index18Row)); } }


        public ICommand _talent1Index1AddCommand;
        public ICommand _talent1Index2AddCommand;
        public ICommand _talent1Index3AddCommand;
        public ICommand _talent1Index4AddCommand;
        public ICommand _talent1Index5AddCommand;
        public ICommand _talent1Index6AddCommand;
        public ICommand _talent1Index7AddCommand;
        public ICommand _talent1Index8AddCommand;
        public ICommand _talent1Index9AddCommand;
        public ICommand _talent1Index10AddCommand;
        public ICommand _talent1Index11AddCommand;
        public ICommand _talent1Index12AddCommand;
        public ICommand _talent1Index13AddCommand;
        public ICommand _talent1Index14AddCommand;
        public ICommand _talent1Index15AddCommand;
        public ICommand _talent1Index16AddCommand;
        public ICommand _talent1Index17AddCommand;
        public ICommand _talent1Index18AddCommand;

        public ICommand _talent2Index1AddCommand;
        public ICommand _talent2Index2AddCommand;
        public ICommand _talent2Index3AddCommand;
        public ICommand _talent2Index4AddCommand;
        public ICommand _talent2Index5AddCommand;
        public ICommand _talent2Index6AddCommand;
        public ICommand _talent2Index7AddCommand;
        public ICommand _talent2Index8AddCommand;
        public ICommand _talent2Index9AddCommand;
        public ICommand _talent2Index10AddCommand;
        public ICommand _talent2Index11AddCommand;
        public ICommand _talent2Index12AddCommand;
        public ICommand _talent2Index13AddCommand;
        public ICommand _talent2Index14AddCommand;
        public ICommand _talent2Index15AddCommand;
        public ICommand _talent2Index16AddCommand;
        public ICommand _talent2Index17AddCommand;
        public ICommand _talent2Index18AddCommand;
        public ICommand _talent2Index19AddCommand;

        public ICommand _talent3Index1AddCommand;
        public ICommand _talent3Index2AddCommand;
        public ICommand _talent3Index3AddCommand;
        public ICommand _talent3Index4AddCommand;
        public ICommand _talent3Index5AddCommand;
        public ICommand _talent3Index6AddCommand;
        public ICommand _talent3Index7AddCommand;
        public ICommand _talent3Index8AddCommand;
        public ICommand _talent3Index9AddCommand;
        public ICommand _talent3Index10AddCommand;
        public ICommand _talent3Index11AddCommand;
        public ICommand _talent3Index12AddCommand;
        public ICommand _talent3Index13AddCommand;
        public ICommand _talent3Index14AddCommand;
        public ICommand _talent3Index15AddCommand;
        public ICommand _talent3Index16AddCommand;
        public ICommand _talent3Index17AddCommand;
        public ICommand _talent3Index18AddCommand;


        public ICommand _talent1Index1RemoveCommand;
        public ICommand _talent1Index2RemoveCommand;
        public ICommand _talent1Index3RemoveCommand;
        public ICommand _talent1Index4RemoveCommand;
        public ICommand _talent1Index5RemoveCommand;
        public ICommand _talent1Index6RemoveCommand;
        public ICommand _talent1Index7RemoveCommand;
        public ICommand _talent1Index8RemoveCommand;
        public ICommand _talent1Index9RemoveCommand;
        public ICommand _talent1Index10RemoveCommand;
        public ICommand _talent1Index11RemoveCommand;
        public ICommand _talent1Index12RemoveCommand;
        public ICommand _talent1Index13RemoveCommand;
        public ICommand _talent1Index14RemoveCommand;
        public ICommand _talent1Index15RemoveCommand;
        public ICommand _talent1Index16RemoveCommand;
        public ICommand _talent1Index17RemoveCommand;
        public ICommand _talent1Index18RemoveCommand;

        public ICommand _talent2Index1RemoveCommand;
        public ICommand _talent2Index2RemoveCommand;
        public ICommand _talent2Index3RemoveCommand;
        public ICommand _talent2Index4RemoveCommand;
        public ICommand _talent2Index5RemoveCommand;
        public ICommand _talent2Index6RemoveCommand;
        public ICommand _talent2Index7RemoveCommand;
        public ICommand _talent2Index8RemoveCommand;
        public ICommand _talent2Index9RemoveCommand;
        public ICommand _talent2Index10RemoveCommand;
        public ICommand _talent2Index11RemoveCommand;
        public ICommand _talent2Index12RemoveCommand;
        public ICommand _talent2Index13RemoveCommand;
        public ICommand _talent2Index14RemoveCommand;
        public ICommand _talent2Index15RemoveCommand;
        public ICommand _talent2Index16RemoveCommand;
        public ICommand _talent2Index17RemoveCommand;
        public ICommand _talent2Index18RemoveCommand;
        public ICommand _talent2Index19RemoveCommand;

        public ICommand _talent3Index1RemoveCommand;
        public ICommand _talent3Index2RemoveCommand;
        public ICommand _talent3Index3RemoveCommand;
        public ICommand _talent3Index4RemoveCommand;
        public ICommand _talent3Index5RemoveCommand;
        public ICommand _talent3Index6RemoveCommand;
        public ICommand _talent3Index7RemoveCommand;
        public ICommand _talent3Index8RemoveCommand;
        public ICommand _talent3Index9RemoveCommand;
        public ICommand _talent3Index10RemoveCommand;
        public ICommand _talent3Index11RemoveCommand;
        public ICommand _talent3Index12RemoveCommand;
        public ICommand _talent3Index13RemoveCommand;
        public ICommand _talent3Index14RemoveCommand;
        public ICommand _talent3Index15RemoveCommand;
        public ICommand _talent3Index16RemoveCommand;
        public ICommand _talent3Index17RemoveCommand;
        public ICommand _talent3Index18RemoveCommand;

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

        private int _talent2Index1Column;
        private int _talent2Index2Column;
        private int _talent2Index3Column;
        private int _talent2Index4Column;
        private int _talent2Index5Column;
        private int _talent2Index6Column;
        private int _talent2Index7Column;
        private int _talent2Index8Column;
        private int _talent2Index9Column;
        private int _talent2Index10Column;
        private int _talent2Index11Column;
        private int _talent2Index12Column;
        private int _talent2Index13Column;
        private int _talent2Index14Column;
        private int _talent2Index15Column;
        private int _talent2Index16Column;
        private int _talent2Index17Column;
        private int _talent2Index18Column;
        private int _talent2Index19Column;
        private int _talent2Index1Row;
        private int _talent2Index2Row;
        private int _talent2Index3Row;
        private int _talent2Index4Row;
        private int _talent2Index5Row;
        private int _talent2Index6Row;
        private int _talent2Index7Row;
        private int _talent2Index8Row;
        private int _talent2Index9Row;
        private int _talent2Index10Row;
        private int _talent2Index11Row;
        private int _talent2Index12Row;
        private int _talent2Index13Row;
        private int _talent2Index14Row;
        private int _talent2Index15Row;
        private int _talent2Index16Row;
        private int _talent2Index17Row;
        private int _talent2Index18Row;
        private int _talent2Index19Row;

        private int _talent3Index1Column;
        private int _talent3Index2Column;
        private int _talent3Index3Column;
        private int _talent3Index4Column;
        private int _talent3Index5Column;
        private int _talent3Index6Column;
        private int _talent3Index7Column;
        private int _talent3Index8Column;
        private int _talent3Index9Column;
        private int _talent3Index10Column;
        private int _talent3Index11Column;
        private int _talent3Index12Column;
        private int _talent3Index13Column;
        private int _talent3Index14Column;
        private int _talent3Index15Column;
        private int _talent3Index16Column;
        private int _talent3Index17Column;
        private int _talent3Index18Column;
        private int _talent3Index1Row;
        private int _talent3Index2Row;
        private int _talent3Index3Row;
        private int _talent3Index4Row;
        private int _talent3Index5Row;
        private int _talent3Index6Row;
        private int _talent3Index7Row;
        private int _talent3Index8Row;
        private int _talent3Index9Row;
        private int _talent3Index10Row;
        private int _talent3Index11Row;
        private int _talent3Index12Row;
        private int _talent3Index13Row;
        private int _talent3Index14Row;
        private int _talent3Index15Row;
        private int _talent3Index16Row;
        private int _talent3Index17Row;
        private int _talent3Index18Row;
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
