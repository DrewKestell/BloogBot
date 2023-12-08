using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RaidLeaderBot.UI.Views.Talents
{
    public class WarriorTalentsViewModel : RaidMemberTalentsViewModel
    {
        public WarriorTalentsViewModel(RaidMemberPreset raidMemberPreset)
        {
            RaidMemberPreset = raidMemberPreset;

            Talent1Header = "Arms";
            Talent2Header = "Fury";
            Talent3Header = "Protection";

            // Improved Heroic Strike
            Talent1Index1Row = 0;
            Talent1Index1Column = 0;
            Talent1Index1Spells = new List<int>() { 12282, 12663, 12664 };
            Talent1Index1Content = $"0 / 3";
            // Deflection
            Talent1Index2Row = 0;
            Talent1Index2Column = 1;
            Talent1Index2Spells = new List<int>() { 16462, 16463, 16464, 16465, 16466 };
            Talent1Index2Content = $"0 / 5";
            // Improved Rend
            Talent1Index3Row = 0;
            Talent1Index3Column = 2;
            Talent1Index3Spells = new List<int>() { 12286, 12658, 12659 };
            Talent1Index3Content = $"0 / 3";

            Talent1Row1Spells.AddRange(Talent1Index1Spells);
            Talent1Row1Spells.AddRange(Talent1Index2Spells);
            Talent1Row1Spells.AddRange(Talent1Index3Spells);

            // Improved Charge
            Talent1Index4Row = 2;
            Talent1Index4Column = 0;
            Talent1Index4Spells = new List<int>() { 12285, 12697 };
            Talent1Index4Content = $"0 / 2";
            // Tactical Mastery
            Talent1Index5Row = 2;
            Talent1Index5Column = 1;
            Talent1Index5Spells = new List<int>() { 12295, 12676, 12677, 12678, 12679 };
            Talent1Index5Content = $"0 / 5";
            // Improved Thunder Clap
            Talent1Index6Row = 2;
            Talent1Index6Column = 3;
            Talent1Index6Spells = new List<int>() { 12287, 12665, 12666 };
            Talent1Index6Content = $"0 / 3";

            Talent1Row2Spells.AddRange(Talent1Index4Spells);
            Talent1Row2Spells.AddRange(Talent1Index5Spells);
            Talent1Row2Spells.AddRange(Talent1Index6Spells);

            // Improved Overpower
            Talent1Index7Row = 4;
            Talent1Index7Column = 0;
            Talent1Index7Spells = new List<int>() { 12290, 12963 };
            Talent1Index7Content = $"0 / 2";
            // Anger Management
            Talent1Index8Row = 4;
            Talent1Index8Column = 1;
            Talent1Index8Spells = new List<int>() { 12296 };
            Talent1Index8Content = $"0 / 1";
            // Deep Woods
            Talent1Index9Row = 4;
            Talent1Index9Column = 2;
            Talent1Index9Spells = new List<int>() { 12834, 12849, 12867 };
            Talent1Index9Content = $"0 / 3";

            Talent1Row3Spells.AddRange(Talent1Index7Spells);
            Talent1Row3Spells.AddRange(Talent1Index8Spells);
            Talent1Row3Spells.AddRange(Talent1Index9Spells);

            // Two-Handed Weapon Specialization
            Talent1Index10Row = 6;
            Talent1Index10Column = 1;
            Talent1Index10Spells = new List<int>() { 12163, 12711, 12712, 12713, 12714 };
            Talent1Index10Content = $"0 / 5";
            // Impale
            Talent1Index11Row = 6;
            Talent1Index11Column = 2;
            Talent1Index11Spells = new List<int>() { 16493, 16494 };
            Talent1Index11Content = $"0 / 2";

            Talent1Row4Spells.AddRange(Talent1Index10Spells);
            Talent1Row4Spells.AddRange(Talent1Index11Spells);

            // Axe Specialization
            Talent1Index12Row = 8;
            Talent1Index12Column = 0;
            Talent1Index12Spells = new List<int>() { 12700, 12781, 12783, 12784, 12785 };
            Talent1Index12Content = $"0 / 5";
            // Sweeping Strikes
            Talent1Index13Row = 8;
            Talent1Index13Column = 1;
            Talent1Index13Spells = new List<int>() { 12292 };
            Talent1Index13Content = $"0 / 1";
            // Mace Specilization
            Talent1Index14Row = 8;
            Talent1Index14Column = 2;
            Talent1Index14Spells = new List<int>() { 12284, 12701, 12702, 12703, 12704 };
            Talent1Index14Content = $"0 / 5";
            // Sword Specialization
            Talent1Index15Row = 8;
            Talent1Index15Column = 3;
            Talent1Index15Spells = new List<int>() { 12281, 12812, 12813, 12814, 12815 };
            Talent1Index15Content = $"0 / 5";

            Talent1Row5Spells.AddRange(Talent1Index12Spells);
            Talent1Row5Spells.AddRange(Talent1Index13Spells);
            Talent1Row5Spells.AddRange(Talent1Index14Spells);
            Talent1Row5Spells.AddRange(Talent1Index15Spells);

            // Polearm Specialization
            Talent1Index16Visibility = Visibility.Visible;
            Talent1Index16Row = 10;
            Talent1Index16Column = 0;
            Talent1Index16Spells = new List<int>() { 12165, 12830, 12831, 12832, 12833 };
            Talent1Index16Content = $"0 / 5";
            // Improved Hamstring
            Talent1Index17Visibility = Visibility.Visible;
            Talent1Index17Row = 10;
            Talent1Index17Column = 2;
            Talent1Index17Spells = new List<int>() { 12289, 12668, 23695 };
            Talent1Index17Content = $"0 / 3";

            Talent1Row6Spells.AddRange(Talent1Index16Spells);
            Talent1Row6Spells.AddRange(Talent1Index17Spells);

            // Mortal Strike
            Talent1Index18Visibility = Visibility.Visible;
            Talent1Index18Row = 12;
            Talent1Index18Column = 1;
            Talent1Index18Spells = new List<int>() { 12294 };
            Talent1Index18Content = $"0 / 1";

            Talent1Row7Spells.AddRange(Talent1Index18Spells);


            // Booming Voice
            Talent2Index1Row = 0;
            Talent2Index1Column = 1;
            Talent2Index1Spells = new List<int>() { 12321, 12835, 12836, 12837, 12838 };
            Talent2Index1Content = $"0 / 5";
            // Cruelty
            Talent2Index2Row = 0;
            Talent2Index2Column = 2;
            Talent2Index2Spells = new List<int>() { 12320, 12852, 12853, 12855, 12856 };
            Talent2Index2Content = $"0 / 5";

            Talent2Row1Spells.AddRange(Talent2Index1Spells);
            Talent2Row1Spells.AddRange(Talent2Index2Spells);

            // Improved Demoralizing Shout
            Talent2Index3Row = 2;
            Talent2Index3Column = 1;
            Talent2Index3Spells = new List<int>() { 12324, 12876, 12877, 12878, 12879 };
            Talent2Index3Content = $"0 / 5";
            // Unbridled Wrath
            Talent2Index4Row = 2;
            Talent2Index4Column = 2;
            Talent2Index4Spells = new List<int>() { 12322, 12999, 13000, 13001, 13002 };
            Talent2Index4Content = $"0 / 5";

            Talent2Row2Spells.AddRange(Talent2Index3Spells);
            Talent2Row2Spells.AddRange(Talent2Index4Spells);

            // Improved Cleave
            Talent2Index5Row = 4;
            Talent2Index5Column = 0;
            Talent2Index5Spells = new List<int>() { 12329, 12950, 20496 };
            Talent2Index5Content = $"0 / 3";
            // Piercing Howl
            Talent2Index6Row = 4;
            Talent2Index6Column = 1;
            Talent2Index6Spells = new List<int>() { 12323 };
            Talent2Index6Content = $"0 / 1";
            // Blood Craze
            Talent2Index7Row = 4;
            Talent2Index7Column = 2;
            Talent2Index7Spells = new List<int>() { 16487, 16489, 16492 };
            Talent2Index7Content = $"0 / 3";
            // Improved Battle Shout
            Talent2Index8Row = 4;
            Talent2Index8Column = 3;
            Talent2Index8Spells = new List<int>() { 12318, 12857, 12858, 12860, 12861 };
            Talent2Index8Content = $"0 / 5";

            Talent2Row3Spells.AddRange(Talent2Index5Spells);
            Talent2Row3Spells.AddRange(Talent2Index6Spells);
            Talent2Row3Spells.AddRange(Talent2Index7Spells);
            Talent2Row3Spells.AddRange(Talent2Index8Spells);

            // Duel Wield Specialization
            Talent2Index9Row = 6;
            Talent2Index9Column = 0;
            Talent2Index9Spells = new List<int>() { 23584, 23585, 23586, 23587, 23588 };
            Talent2Index9Content = $"0 / 5";
            // Improved Execute
            Talent2Index10Row = 6;
            Talent2Index10Column = 1;
            Talent2Index10Spells = new List<int>() { 20502, 20503 };
            Talent2Index10Content = $"0 / 2";
            // Enrage
            Talent2Index11Row = 6;
            Talent2Index11Column = 2;
            Talent2Index11Spells = new List<int>() { 12317, 13045, 13046, 13047, 13048 };
            Talent2Index11Content = $"0 / 5";

            Talent2Row4Spells.AddRange(Talent2Index9Spells);
            Talent2Row4Spells.AddRange(Talent2Index10Spells);
            Talent2Row4Spells.AddRange(Talent2Index11Spells);

            // Improved Slam
            Talent2Index12Row = 8;
            Talent2Index12Column = 0;
            Talent2Index12Spells = new List<int>() { 12862, 12330, 20497, 20498, 20499 };
            Talent2Index12Content = $"0 / 5";
            // Death Wish
            Talent2Index13Row = 8;
            Talent2Index13Column = 1;
            Talent2Index13Spells = new List<int>() { 12328 };
            Talent2Index13Content = $"0 / 1";
            // Improved Intercept
            Talent2Index14Row = 8;
            Talent2Index14Column = 3;
            Talent2Index14Spells = new List<int>() { 20504, 20505 };
            Talent2Index14Content = $"0 / 2";

            Talent2Row5Spells.AddRange(Talent2Index12Spells);
            Talent2Row5Spells.AddRange(Talent2Index13Spells);
            Talent2Row5Spells.AddRange(Talent2Index14Spells);

            // Improved Berserker Rage
            Talent2Index15Row = 10;
            Talent2Index15Column = 0;
            Talent2Index15Spells = new List<int>() { 20500, 20501 };
            Talent2Index15Content = $"0 / 2";
            // Flurry
            Talent2Index16Visibility = Visibility.Visible;
            Talent2Index16Row = 10;
            Talent2Index16Column = 2;
            Talent2Index16Spells = new List<int>() { 12319, 12971, 12972, 12973, 12974 };
            Talent2Index16Content = $"0 / 5";

            Talent2Row6Spells.AddRange(Talent2Index15Spells);
            Talent2Row6Spells.AddRange(Talent2Index16Spells);

            // Bloodthirst
            Talent2Index17Visibility = Visibility.Visible;
            Talent2Index17Row = 12;
            Talent2Index17Column = 1;
            Talent2Index17Spells = new List<int>() { 23881 };
            Talent2Index17Content = $"0 / 1";

            Talent1Row7Spells.AddRange(Talent1Index17Spells);

            Talent2Index18Visibility = Visibility.Hidden;
            Talent2Index19Visibility = Visibility.Hidden;


            // Shield Specialization
            Talent3Index1Row = 0;
            Talent3Index1Column = 1;
            Talent3Index1Spells = new List<int>() { 12298, 12724, 12725, 12726, 12727 };
            Talent3Index1Content = $"0 / 5";
            // Anticipation
            Talent3Index2Row = 0;
            Talent3Index2Column = 2;
            Talent3Index2Spells = new List<int>() { 12297, 12750, 12751, 12752, 12753 };
            Talent3Index2Content = $"0 / 5";

            Talent3Row1Spells.AddRange(Talent3Index1Spells);
            Talent3Row1Spells.AddRange(Talent3Index2Spells);

            // Improved Bloodrage
            Talent3Index3Row = 2;
            Talent3Index3Column = 0;
            Talent3Index3Spells = new List<int>() { 12301, 12818 };
            Talent3Index3Content = $"0 / 2";
            // Toughness
            Talent3Index4Row = 2;
            Talent3Index4Column = 2;
            Talent3Index4Spells = new List<int>() { 12299, 12761, 12762, 12763, 12764 };
            Talent3Index4Content = $"0 / 5";
            // Iron Will
            Talent3Index5Row = 2;
            Talent3Index5Column = 3;
            Talent3Index5Spells = new List<int>() { 12300, 12959, 12960, 12961, 12962 };
            Talent3Index5Content = $"0 / 5";

            Talent3Row2Spells.AddRange(Talent3Index3Spells);
            Talent3Row2Spells.AddRange(Talent3Index4Spells);
            Talent3Row2Spells.AddRange(Talent3Index5Spells);

            // Last Stand
            Talent3Index6Row = 4;
            Talent3Index6Column = 0;
            Talent3Index6Spells = new List<int>() { 12975 };
            Talent3Index6Content = $"0 / 1";
            // Improved Shield Block
            Talent3Index7Row = 4;
            Talent3Index7Column = 1;
            Talent3Index7Spells = new List<int>() { 12945, 12307, 12944 };
            Talent3Index7Content = $"0 / 3";
            // Improved Revenge
            Talent3Index8Row = 4;
            Talent3Index8Column = 2;
            Talent3Index8Spells = new List<int>() { 12797, 12799, 12800 };
            Talent3Index8Content = $"0 / 3";
            // Defiance
            Talent3Index9Row = 4;
            Talent3Index9Column = 3;
            Talent3Index9Spells = new List<int>() { 12303, 12788, 12789, 12791, 12792 };
            Talent3Index9Content = $"0 / 5";

            Talent3Row3Spells.AddRange(Talent3Index6Spells);
            Talent3Row3Spells.AddRange(Talent3Index7Spells);
            Talent3Row3Spells.AddRange(Talent3Index8Spells);
            Talent3Row3Spells.AddRange(Talent3Index9Spells);

            // Improved Sunder Armor
            Talent3Index10Row = 6;
            Talent3Index10Column = 0;
            Talent3Index10Spells = new List<int>() { 12308, 12810, 12811 };
            Talent3Index10Content = $"0 / 3";
            // Improved Disarm
            Talent3Index11Row = 6;
            Talent3Index11Column = 1;
            Talent3Index11Spells = new List<int>() { 12313, 12804, 12807 };
            Talent3Index11Content = $"0 / 3";
            // Improved Taunt
            Talent3Index12Row = 6;
            Talent3Index12Column = 2;
            Talent3Index12Spells = new List<int>() { 12302, 12765 };
            Talent3Index12Content = $"0 / 2";

            Talent3Row4Spells.AddRange(Talent3Index10Spells);
            Talent3Row4Spells.AddRange(Talent3Index11Spells);
            Talent3Row4Spells.AddRange(Talent3Index12Spells);

            // Improved Shield Wall
            Talent3Index13Row = 8;
            Talent3Index13Column = 0;
            Talent3Index13Spells = new List<int>() { 12312, 12803 };
            Talent3Index13Content = $"0 / 2";
            // Concussion Blow
            Talent3Index14Row = 8;
            Talent3Index14Column = 1;
            Talent3Index14Spells = new List<int>() { 12809 };
            Talent3Index14Content = $"0 / 1";
            // Improved Shield Bash
            Talent3Index15Row = 8;
            Talent3Index15Column = 2;
            Talent3Index15Spells = new List<int>() { 12311, 12958 };
            Talent3Index15Content = $"0 / 2";

            Talent3Row5Spells.AddRange(Talent3Index13Spells);
            Talent3Row5Spells.AddRange(Talent3Index14Spells);
            Talent3Row5Spells.AddRange(Talent3Index15Spells);

            // One-Handed Specialization
            Talent3Index16Visibility = Visibility.Visible;
            Talent3Index16Row = 10;
            Talent3Index16Column = 2;
            Talent3Index16Spells = new List<int>() { 16538, 16539, 16540, 16541, 16542 };
            Talent3Index16Content = $"0 / 5";

            Talent3Row6Spells.AddRange(Talent3Index16Spells);

            // Shield Slam
            Talent3Index17Visibility = Visibility.Visible;
            Talent3Index17Row = 12;
            Talent3Index17Column = 1;
            Talent3Index17Spells = new List<int>() { 23922 };
            Talent3Index17Content = $"0 / 1";

            Talent3Row7Spells.AddRange(Talent3Index17Spells);


            Talent3Index18Visibility = Visibility.Hidden;
        }
        #region Talent Tree 1
        public override bool Talent1Index1CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index2CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index3CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index4CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index5CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index6CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index7CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index8CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index9CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index10CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index11CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index12CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index13CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index14CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index15CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index16CanRemove => !Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index17CanRemove => !Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index18CanRemove => Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index4Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index5Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index6Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index7Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index8Enabled => Talent1Spells.Count > 9 && Talent1Index5Spells.All(x => Talent1Spells.Contains(x));
        public override bool Talent1Index9Enabled => Talent1Spells.Count > 9 && Talent1Index3Spells.All(x => Talent1Spells.Contains(x));
        public override bool Talent1Index10Enabled => Talent1Spells.Count > 14;
        public override bool Talent1Index11Enabled => Talent1Spells.Count > 14 && Talent1Index9Spells.All(x => Talent1Spells.Contains(x));
        public override bool Talent1Index12Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index13Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index14Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index15Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index16Enabled => Talent1Spells.Count > 24;
        public override bool Talent1Index17Enabled => Talent1Spells.Count > 24;
        public override bool Talent1Index18Enabled => Talent1Spells.Count > 29 && Talent1Index13Spells.All(x => Talent1Spells.Contains(x));
        #endregion

        #region Talent Tree 2
        public override bool Talent2Index1CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index2CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index3CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index4CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index5CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index6CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index7CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index8CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index9CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index10CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index11CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index12CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index13CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index14CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index15CanRemove => !Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index16CanRemove => !Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index17CanRemove => Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index3Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index4Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index5Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index6Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index7Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index8Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index9Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index10Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index11Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index12Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index13Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index14Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index15Enabled => Talent2Spells.Count > 24;
        public override bool Talent2Index16Enabled => Talent2Spells.Count > 24 && Talent2Index11Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index17Enabled => Talent2Spells.Count > 29 && Talent2Index13Spells.All(x => Talent2Spells.Contains(x));
        #endregion

        #region Talent Tree 3
        public override bool Talent3Index1CanRemove => !Talent3Row2Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index2CanRemove => !Talent3Row2Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index3CanRemove => !Talent3Row3Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index4CanRemove => !Talent3Row3Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index5CanRemove => !Talent3Row3Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index6CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index7CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index8CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index9CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index10CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index11CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index12CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index13CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index14CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index15CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index16CanRemove => !Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index17CanRemove => Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index3Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index4Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index5Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index6Enabled => Talent3Spells.Count > 9 && Talent3Index3Spells.All(x => Talent3Spells.Contains(x));
        public override bool Talent3Index7Enabled => Talent3Spells.Count > 9 && Talent3Index1Spells.All(x => Talent3Spells.Contains(x));
        public override bool Talent3Index8Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index9Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index10Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index11Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index12Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index13Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index14Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index15Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index16Enabled => Talent3Spells.Count > 24;
        public override bool Talent3Index17Enabled => Talent3Spells.Count > 29 && Talent3Index14Spells.All(x => Talent3Spells.Contains(x));
        #endregion
    }
}
