using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RaidLeaderBot.UI.Views.Talents
{
    public class RogueTalentsViewModel : RaidMemberTalentsViewModel
    {
        public RogueTalentsViewModel(RaidMemberPreset raidMemberPreset)
        {
            RaidMemberPreset = raidMemberPreset;

            Talent1Header = "Assassination";
            Talent2Header = "Combat";
            Talent3Header = "Subtlety";

            // Improved Eviscerate
            Talent1Index1Row = 0;
            Talent1Index1Column = 0;
            Talent1Index1Spells = new List<int>() { 14162, 14163, 14164 };
            // Remorseless Attacks
            Talent1Index2Row = 0;
            Talent1Index2Column = 1;
            Talent1Index2Spells = new List<int>() { 14144, 14148 };
            // Malice
            Talent1Index3Row = 0;
            Talent1Index3Column = 2;
            Talent1Index3Spells = new List<int>() { 14138, 14139, 14140, 14141, 14142 };

            Talent1Row1Spells.AddRange(Talent1Index1Spells);
            Talent1Row1Spells.AddRange(Talent1Index2Spells);
            Talent1Row1Spells.AddRange(Talent1Index3Spells);

            // Ruthlessness
            Talent1Index4Row = 2;
            Talent1Index4Column = 0;
            Talent1Index4Spells = new List<int>() { 14156, 14160, 14161 };
            // Murder
            Talent1Index5Row = 2;
            Talent1Index5Column = 1;
            Talent1Index5Spells = new List<int>() { 14158, 14159 };
            // Improved Slice and Dice
            Talent1Index6Row = 2;
            Talent1Index6Column = 3;
            Talent1Index6Spells = new List<int>() { 14165, 14166, 14167 };

            Talent1Row2Spells.AddRange(Talent1Index4Spells);
            Talent1Row2Spells.AddRange(Talent1Index5Spells);
            Talent1Row2Spells.AddRange(Talent1Index6Spells);

            // Relentless Strikes
            Talent1Index7Row = 4;
            Talent1Index7Column = 0;
            Talent1Index7Spells = new List<int>() { 14179 };
            // Improved Expose Armor
            Talent1Index8Row = 4;
            Talent1Index8Column = 1;
            Talent1Index8Spells = new List<int>() { 14168, 14169 };
            // Lethality
            Talent1Index9Row = 4;
            Talent1Index9Column = 2;
            Talent1Index9Spells = new List<int>() { 14128, 14132, 14135, 14136, 14137 };

            Talent1Row3Spells.AddRange(Talent1Index7Spells);
            Talent1Row3Spells.AddRange(Talent1Index8Spells);
            Talent1Row3Spells.AddRange(Talent1Index9Spells);

            // Vile Poisons
            Talent1Index10Row = 6;
            Talent1Index10Column = 1;
            Talent1Index10Spells = new List<int>() { 16513, 16514, 16515, 16719, 16720 };
            // Improved Poisons
            Talent1Index11Row = 6;
            Talent1Index11Column = 2;
            Talent1Index11Spells = new List<int>() { 14113, 14114, 14115, 14116, 14117 };

            Talent1Row4Spells.AddRange(Talent1Index10Spells);
            Talent1Row4Spells.AddRange(Talent1Index11Spells);

            // Cold Blood
            Talent1Index12Row = 8;
            Talent1Index12Column = 1;
            Talent1Index12Spells = new List<int>() { 14177 };
            // Improved Kidney Shot
            Talent1Index13Row = 8;
            Talent1Index13Column = 2;
            Talent1Index13Spells = new List<int>() { 14174, 14175, 14176 };

            Talent1Row5Spells.AddRange(Talent1Index12Spells);
            Talent1Row5Spells.AddRange(Talent1Index13Spells);

            // Seal Fate
            Talent1Index14Row = 10;
            Talent1Index14Column = 1;
            Talent1Index14Spells = new List<int>() { 14186, 14190, 14193, 14194, 14195 };

            Talent1Row6Spells.AddRange(Talent1Index14Spells);

            // Vigor
            Talent1Index15Row = 12;
            Talent1Index15Column = 1;
            Talent1Index15Spells = new List<int>() { 14983 };

            Talent1Row6Spells.AddRange(Talent1Index15Spells);

            Talent1Index16Visibility = Visibility.Hidden;
            Talent1Index17Visibility = Visibility.Hidden;
            Talent1Index18Visibility = Visibility.Hidden;


            // Improved Gouge
            Talent2Index1Row = 0;
            Talent2Index1Column = 0;
            Talent2Index1Spells = new List<int>() { 13741, 13793, 13792 };
            // Improved Sinister Strike
            Talent2Index2Row = 0;
            Talent2Index2Column = 1;
            Talent2Index2Spells = new List<int>() { 13732, 13863 };
            // Lightning Reflexes
            Talent2Index3Row = 0;
            Talent2Index3Column = 2;
            Talent2Index3Spells = new List<int>() { 13712, 13788, 13789, 13790, 13791 };

            Talent2Row1Spells.AddRange(Talent2Index1Spells);
            Talent2Row1Spells.AddRange(Talent2Index2Spells);
            Talent2Row1Spells.AddRange(Talent2Index3Spells);

            // Improved Backstab
            Talent2Index4Row = 2;
            Talent2Index4Column = 0;
            Talent2Index4Spells = new List<int>() { 13733, 13865, 13866 };
            // Deflection
            Talent2Index5Row = 2;
            Talent2Index5Column = 1;
            Talent2Index5Spells = new List<int>() { 13713, 13853, 13854, 13855, 13856 };
            // Precision
            Talent2Index6Row = 2;
            Talent2Index6Column = 2;
            Talent2Index6Spells = new List<int>() { 13705, 13832, 13843, 13844, 13845 };

            Talent2Row2Spells.AddRange(Talent2Index4Spells);
            Talent2Row2Spells.AddRange(Talent2Index5Spells);
            Talent2Row2Spells.AddRange(Talent2Index6Spells);

            // Endurance
            Talent2Index7Row = 4;
            Talent2Index7Column = 0;
            Talent2Index7Spells = new List<int>() { 13742, 13872 };
            // Riposte
            Talent2Index8Row = 4;
            Talent2Index8Column = 1;
            Talent2Index8Spells = new List<int>() { 14251 };
            // Improved Sprint
            Talent2Index9Row = 4;
            Talent2Index9Column = 3;
            Talent2Index9Spells = new List<int>() { 13743, 13875 };

            Talent2Row3Spells.AddRange(Talent2Index7Spells);
            Talent2Row3Spells.AddRange(Talent2Index8Spells);
            Talent2Row3Spells.AddRange(Talent2Index9Spells);

            // Improved Kick
            Talent2Index10Row = 6;
            Talent2Index10Column = 0;
            Talent2Index10Spells = new List<int>() { 13754, 13867 };
            // Dagger Specialization
            Talent2Index11Row = 6;
            Talent2Index11Column = 1;
            Talent2Index11Spells = new List<int>() { 13706, 13804, 13805, 13806, 13807 };
            // Dual Wield Specialization
            Talent2Index12Row = 6;
            Talent2Index12Column = 2;
            Talent2Index12Spells = new List<int>() { 13715, 13848, 13849, 13851, 13852 };

            Talent2Row4Spells.AddRange(Talent2Index10Spells);
            Talent2Row4Spells.AddRange(Talent2Index11Spells);
            Talent2Row4Spells.AddRange(Talent2Index12Spells);

            // Mace Specialization
            Talent2Index13Row = 8;
            Talent2Index13Column = 0;
            Talent2Index13Spells = new List<int>() { 13709, 13800, 13801, 13802, 13803 };
            // Blade Flurry
            Talent2Index14Row = 8;
            Talent2Index14Column = 1;
            Talent2Index14Spells = new List<int>() { 13877 };
            // Sword Specialization
            Talent2Index15Row = 8;
            Talent2Index15Column = 2;
            Talent2Index15Spells = new List<int>() { 13960, 13961, 13962, 13963, 13964 };
            // Fist Weapon Specialization
            Talent2Index16Visibility = Visibility.Visible;
            Talent2Index16Row = 8;
            Talent2Index16Column = 3;
            Talent2Index16Spells = new List<int>() { 13707, 13966, 13967, 13968, 13969 };

            Talent2Row5Spells.AddRange(Talent2Index13Spells);
            Talent2Row5Spells.AddRange(Talent2Index14Spells);
            Talent2Row5Spells.AddRange(Talent2Index15Spells);
            Talent2Row5Spells.AddRange(Talent2Index16Spells);

            // Weapon Expertise
            Talent2Index17Visibility = Visibility.Visible;
            Talent2Index17Row = 10;
            Talent2Index17Column = 1;
            Talent2Index17Spells = new List<int>() { 30919, 30920 };
            // Aggression
            Talent2Index18Visibility = Visibility.Visible;
            Talent2Index18Row = 10;
            Talent2Index18Column = 2;
            Talent2Index18Spells = new List<int>() { 18427, 18428, 18429 };

            Talent2Row6Spells.AddRange(Talent2Index17Spells);
            Talent2Row6Spells.AddRange(Talent2Index18Spells);

            // Adrenaline Rush
            Talent2Index19Visibility = Visibility.Visible;
            Talent2Index19Row = 12;
            Talent2Index19Column = 1;
            Talent2Index19Spells = new List<int>() { 13750 };

            Talent2Row7Spells.AddRange(Talent2Index19Spells);


            // Master of Deception
            Talent3Index1Row = 0;
            Talent3Index1Column = 1;
            Talent3Index1Spells = new List<int>() { 13958, 13970, 13971, 13972, 13973 };
            // Opportunity
            Talent3Index2Row = 0;
            Talent3Index2Column = 2;
            Talent3Index2Spells = new List<int>() { 14057, 14072, 14073, 14074, 14075 };

            Talent3Row1Spells.AddRange(Talent3Index1Spells);
            Talent3Row1Spells.AddRange(Talent3Index2Spells);

            // Sleight of Hand
            Talent3Index3Row = 2;
            Talent3Index3Column = 0;
            Talent3Index3Spells = new List<int>() { 30892, 30893 };
            // Elusiveness
            Talent3Index4Row = 2;
            Talent3Index4Column = 1;
            Talent3Index4Spells = new List<int>() { 13981, 14066 };
            // Camouflage
            Talent3Index5Row = 2;
            Talent3Index5Column = 2;
            Talent3Index5Spells = new List<int>() { 13975, 14062, 14063, 14064, 14065 };

            Talent3Row2Spells.AddRange(Talent3Index3Spells);
            Talent3Row2Spells.AddRange(Talent3Index4Spells);
            Talent3Row2Spells.AddRange(Talent3Index5Spells);

            // Initiative
            Talent3Index6Row = 4;
            Talent3Index6Column = 0;
            Talent3Index6Spells = new List<int>() { 13976, 13979, 13980 };
            // Ghostly Strike
            Talent3Index7Row = 4;
            Talent3Index7Column = 1;
            Talent3Index7Spells = new List<int>() { 14278 };
            // Improved Ambush
            Talent3Index8Row = 4;
            Talent3Index8Column = 2;
            Talent3Index8Spells = new List<int>() { 14079, 14080, 14081 };

            Talent3Row3Spells.AddRange(Talent3Index6Spells);
            Talent3Row3Spells.AddRange(Talent3Index7Spells);
            Talent3Row3Spells.AddRange(Talent3Index8Spells);

            // Setup
            Talent3Index9Row = 6;
            Talent3Index9Column = 0;
            Talent3Index9Spells = new List<int>() { 13983, 14070, 14071 };
            // Improved Sap
            Talent3Index10Row = 6;
            Talent3Index10Column = 1;
            Talent3Index10Spells = new List<int>() { 14076, 14094, 14095 };
            // Serrated Blades
            Talent3Index11Row = 6;
            Talent3Index11Column = 2;
            Talent3Index11Spells = new List<int>() { 14171, 14172, 14173 };

            Talent3Row4Spells.AddRange(Talent3Index9Spells);
            Talent3Row4Spells.AddRange(Talent3Index10Spells);
            Talent3Row4Spells.AddRange(Talent3Index11Spells);

            // Heightened Senses
            Talent3Index12Row = 8;
            Talent3Index12Column = 0;
            Talent3Index12Spells = new List<int>() { 30894, 30895 };
            // Preparation
            Talent3Index13Row = 8;
            Talent3Index13Column = 1;
            Talent3Index13Spells = new List<int>() { 14185 };
            // Dirty Deeds
            Talent3Index14Row = 8;
            Talent3Index14Column = 2;
            Talent3Index14Spells = new List<int>() { 14082, 14083 };
            // Hemorrhage
            Talent3Index15Row = 8;
            Talent3Index15Column = 3;
            Talent3Index15Spells = new List<int>() { 16511 };

            Talent3Row5Spells.AddRange(Talent3Index12Spells);
            Talent3Row5Spells.AddRange(Talent3Index13Spells);
            Talent3Row5Spells.AddRange(Talent3Index14Spells);
            Talent3Row5Spells.AddRange(Talent3Index15Spells);

            // Deadliness
            Talent3Index16Visibility = Visibility.Visible;
            Talent3Index16Row = 10;
            Talent3Index16Column = 2;
            Talent3Index16Spells = new List<int>() { 30902, 30903, 30904, 30905, 30906 };

            Talent3Row6Spells.AddRange(Talent3Index16Spells);

            // Premeditation
            Talent3Index17Visibility = Visibility.Visible;
            Talent3Index17Row = 12;
            Talent3Index17Column = 1;
            Talent3Index17Spells = new List<int>() { 14183 };

            Talent3Row7Spells.AddRange(Talent3Index17Spells);

            Talent3Index18Visibility = Visibility.Hidden;

            LoadTalentList();
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
        public override bool Talent1Index14CanRemove => !Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index15CanRemove => Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index4Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index5Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index6Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index7Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index8Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index9Enabled => Talent1Spells.Count > 9 && Talent1Index3Spells.All(x => Talent1Spells.Contains(x));
        public override bool Talent1Index10Enabled => Talent1Spells.Count > 14;
        public override bool Talent1Index11Enabled => Talent1Spells.Count > 14;
        public override bool Talent1Index12Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index13Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index14Enabled => Talent1Spells.Count > 24 && Talent1Index12Spells.All(x => Talent1Spells.Contains(x));
        public override bool Talent1Index15Enabled => Talent1Spells.Count > 29;
        #endregion

        #region Talent Tree 2
        public override bool Talent2Index1CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index2CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index3CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index4CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index5CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index6CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index7CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index8CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index9CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index10CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index11CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index12CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index13CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index14CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index15CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index16CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index17CanRemove => !Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index18CanRemove => !Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index19CanRemove => Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index4Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index5Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index6Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index7Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index8Enabled => Talent2Spells.Count > 9 && Talent2Index5Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index9Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index10Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index11Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index12Enabled => Talent2Spells.Count > 14 && Talent2Index6Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index13Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index14Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index15Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index16Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index17Enabled => Talent2Spells.Count > 24 && Talent2Index14Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index18Enabled => Talent2Spells.Count > 24;
        public override bool Talent2Index19Enabled => Talent2Spells.Count > 29;
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
        public override bool Talent3Index9CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index10CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index11CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index12CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index13CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index14CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index15CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index16CanRemove => !Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index17CanRemove => Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index3Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index4Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index5Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index6Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index7Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index8Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index9Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index10Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index11Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index12Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index13Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index14Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index15Enabled => Talent3Spells.Count > 19 && Talent3Index11Spells.All(x => Talent3Spells.Contains(x));
        public override bool Talent3Index16Enabled => Talent3Spells.Count > 24;
        public override bool Talent3Index17Enabled => Talent3Spells.Count > 29 && Talent3Index10Spells.All(x => Talent3Spells.Contains(x));
        #endregion
    }
}
