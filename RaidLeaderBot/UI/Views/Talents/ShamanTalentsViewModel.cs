using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaidLeaderBot.UI.Views.Talents
{
    public class ShamanTalentsViewModel : RaidMemberTalentsViewModel
    {
        public ShamanTalentsViewModel(RaidMemberPreset raidMemberPreset)
        {
            RaidMemberPreset = raidMemberPreset;

            Talent1Header = "Elemental";
            Talent2Header = "Enhancement";
            Talent3Header = "Restoration";

            // Convection
            Talent1Index1Row = 0;
            Talent1Index1Column = 1;
            Talent1Index1Spells = new List<int>() { 16039, 16109, 16110, 16111, 16112 };
            Talent1Index1Content = $"0 / 5";
            // Concussion
            Talent1Index2Row = 0;
            Talent1Index2Column = 2;
            Talent1Index2Spells = new List<int>() { 16035, 16105, 16106, 16107, 16108 };
            Talent1Index2Content = $"0 / 5";

            Talent1Row1Spells.AddRange(Talent1Index1Spells);
            Talent1Row1Spells.AddRange(Talent1Index2Spells);

            // Earth's Grasp
            Talent1Index3Row = 2;
            Talent1Index3Column = 0;
            Talent1Index3Spells = new List<int>() { 16043, 16130 };
            Talent1Index3Content = $"0 / 2";
            // Elemental Warding
            Talent1Index4Row = 2;
            Talent1Index4Column = 1;
            Talent1Index4Spells = new List<int>() { 28896, 28897, 28898 };
            Talent1Index4Content = $"0 / 3";
            // Call of Flame
            Talent1Index5Row = 2;
            Talent1Index5Column = 2;
            Talent1Index5Spells = new List<int>() { 16038, 16160, 16161 };
            Talent1Index5Content = $"0 / 3";

            Talent1Row2Spells.AddRange(Talent1Index3Spells);
            Talent1Row2Spells.AddRange(Talent1Index4Spells);
            Talent1Row2Spells.AddRange(Talent1Index5Spells);

            // Elemental Focus
            Talent1Index6Row = 4;
            Talent1Index6Column = 0;
            Talent1Index6Spells = new List<int>() { 16164 };
            Talent1Index6Content = $"0 / 1";
            // Reverberation
            Talent1Index7Row = 4;
            Talent1Index7Column = 1;
            Talent1Index7Spells = new List<int>() { 16040, 16113, 16114, 16115, 16116 };
            Talent1Index7Content = $"0 / 5";
            // Call of Thunder
            Talent1Index8Row = 4;
            Talent1Index8Column = 2;
            Talent1Index8Spells = new List<int>() { 16041, 16117, 16118, 16119, 16120 };
            Talent1Index8Content = $"0 / 5";

            Talent1Row3Spells.AddRange(Talent1Index6Spells);
            Talent1Row3Spells.AddRange(Talent1Index7Spells);
            Talent1Row3Spells.AddRange(Talent1Index8Spells);

            // Improved Fire Totems
            Talent1Index9Row = 6;
            Talent1Index9Column = 0;
            Talent1Index9Spells = new List<int>() { 16086, 16544 };
            Talent1Index9Content = $"0 / 2";
            // Eye of the Storm
            Talent1Index10Row = 6;
            Talent1Index10Column = 1;
            Talent1Index10Spells = new List<int>() { 29062, 29064, 29065 };
            Talent1Index10Content = $"0 / 3";
            // Elemental Devastation
            Talent1Index11Row = 6;
            Talent1Index11Column = 3;
            Talent1Index11Spells = new List<int>() { 30160, 29179, 29180 };
            Talent1Index11Content = $"0 / 3";

            Talent1Row4Spells.AddRange(Talent1Index9Spells);
            Talent1Row4Spells.AddRange(Talent1Index10Spells);
            Talent1Row4Spells.AddRange(Talent1Index11Spells);

            // Storm Reach
            Talent1Index12Row = 8;
            Talent1Index12Column = 0;
            Talent1Index12Spells = new List<int>() { 18174, 18175, 18176, 18177, 18178 };
            Talent1Index12Content = $"0 / 2";
            // Elemental Fury
            Talent1Index13Row = 8;
            Talent1Index13Column = 1;
            Talent1Index13Spells = new List<int>() { 18174, 18175, 18176, 18177, 18178 };
            Talent1Index13Content = $"0 / 1";

            Talent1Row5Spells.AddRange(Talent1Index12Spells);
            Talent1Row5Spells.AddRange(Talent1Index13Spells);

            // Lightning Mastery
            Talent1Index14Row = 10;
            Talent1Index14Column = 2;
            Talent1Index14Spells = new List<int>() { 16578, 16579, 16580, 16581, 16582 };
            Talent1Index14Content = $"0 / 5";

            Talent1Row6Spells.AddRange(Talent1Index14Spells);

            // Elemental Mastery
            Talent1Index15Row = 12;
            Talent1Index15Column = 1;
            Talent1Index15Spells = new List<int>() { 16166 };
            Talent1Index15Content = $"0 / 1";

            Talent1Row7Spells.AddRange(Talent1Index15Spells);

            Talent1Index16Visibility = Visibility.Hidden;
            Talent1Index17Visibility = Visibility.Hidden;
            Talent1Index18Visibility = Visibility.Hidden;


            // Ancestral Knowledge
            Talent2Index1Row = 0;
            Talent2Index1Column = 1;
            Talent2Index1Spells = new List<int>() { 17485, 17486, 17487, 17488, 17489 };
            Talent2Index1Content = $"0 / 5";
            // Shield Specialization
            Talent2Index2Row = 0;
            Talent2Index2Column = 2;
            Talent2Index2Spells = new List<int>() { 16253, 16298, 16299, 16300, 16301 };
            Talent2Index2Content = $"0 / 5";

            Talent2Row1Spells.AddRange(Talent2Index1Spells);
            Talent2Row1Spells.AddRange(Talent2Index2Spells);

            // Guardian Totems
            Talent2Index3Row = 2;
            Talent2Index3Column = 0;
            Talent2Index3Spells = new List<int>() { 16258, 16293 };
            Talent2Index3Content = $"0 / 2";
            // Thundering Strikes
            Talent2Index4Row = 2;
            Talent2Index4Column = 1;
            Talent2Index4Spells = new List<int>() { 16255, 16302, 16303, 16304, 16305 };
            Talent2Index4Content = $"0 / 5";
            // Improved Ghost Wolf
            Talent2Index5Row = 2;
            Talent2Index5Column = 2;
            Talent2Index5Spells = new List<int>() { 16262, 16287 };
            Talent2Index5Content = $"0 / 2";
            // Improved Lightning Shield
            Talent2Index6Row = 2;
            Talent2Index6Column = 3;
            Talent2Index6Spells = new List<int>() { 16261, 16290, 16291 };
            Talent2Index6Content = $"0 / 3";

            Talent2Row2Spells.AddRange(Talent2Index3Spells);
            Talent2Row2Spells.AddRange(Talent2Index4Spells);
            Talent2Row2Spells.AddRange(Talent2Index5Spells);
            Talent2Row2Spells.AddRange(Talent2Index6Spells);

            // Enhancing Totems
            Talent2Index7Row = 4;
            Talent2Index7Column = 0;
            Talent2Index7Spells = new List<int>() { 16259, 16295 };
            Talent2Index7Content = $"0 / 2";
            // Two-Handed Axes and Maces
            Talent2Index8Row = 4;
            Talent2Index8Column = 2;
            Talent2Index8Spells = new List<int>() { 16269 };
            Talent2Index8Content = $"0 / 1";
            // Anticipation
            Talent2Index9Row = 4;
            Talent2Index9Column = 3;
            Talent2Index9Spells = new List<int>() { 16254, 16271, 162712, 16273, 16274 };
            Talent2Index9Content = $"0 / 5";

            Talent2Row3Spells.AddRange(Talent2Index7Spells);
            Talent2Row3Spells.AddRange(Talent2Index8Spells);
            Talent2Row3Spells.AddRange(Talent2Index9Spells);

            // Flurry
            Talent2Index10Row = 6;
            Talent2Index10Column = 1;
            Talent2Index10Spells = new List<int>() { 16256, 16281, 16282, 16283, 16284 };
            Talent2Index10Content = $"0 / 5";
            // Toughness
            Talent2Index11Row = 6;
            Talent2Index11Column = 2;
            Talent2Index11Spells = new List<int>() { 16252, 16306, 16307, 16308, 16309 };
            Talent2Index11Content = $"0 / 5";

            Talent2Row4Spells.AddRange(Talent2Index10Spells);
            Talent2Row4Spells.AddRange(Talent2Index11Spells);

            // Improved Weapon Totems
            Talent2Index12Row = 8;
            Talent2Index12Column = 0;
            Talent2Index12Spells = new List<int>() { 29192, 29193 };
            Talent2Index12Content = $"0 / 2";
            // Elemental Weapons
            Talent2Index13Row = 8;
            Talent2Index13Column = 1;
            Talent2Index13Spells = new List<int>() { 16266, 29079, 29080 };
            Talent2Index13Content = $"0 / 3";
            // Parry
            Talent2Index14Row = 8;
            Talent2Index14Column = 2;
            Talent2Index14Spells = new List<int>() { 16268 };
            Talent2Index14Content = $"0 / 1";

            Talent2Row5Spells.AddRange(Talent2Index12Spells);
            Talent2Row5Spells.AddRange(Talent2Index13Spells);
            Talent2Row5Spells.AddRange(Talent2Index14Spells);

            // Weapon Mastery
            Talent2Index15Row = 10;
            Talent2Index15Column = 2;
            Talent2Index15Spells = new List<int>() { 29082, 29084, 29086, 29087, 29088 };
            Talent2Index15Content = $"0 / 5";

            Talent2Row6Spells.AddRange(Talent2Index15Spells);

            // Stormstrike
            Talent2Index16Visibility = Visibility.Visible;
            Talent2Index16Row = 12;
            Talent2Index16Column = 1;
            Talent2Index16Spells = new List<int>() { 17364 };
            Talent2Index16Content = $"0 / 1";

            Talent2Row7Spells.AddRange(Talent2Index16Spells);


            Talent2Index17Visibility = Visibility.Hidden;
            Talent2Index18Visibility = Visibility.Hidden;
            Talent2Index19Visibility = Visibility.Hidden;


            // Improved Healing Wave
            Talent3Index1Row = 0;
            Talent3Index1Column = 1;
            Talent3Index1Spells = new List<int>() { 16182, 16226, 16227, 16228, 16229 };
            Talent3Index1Content = $"0 / 5";
            // Tidal Focus
            Talent3Index2Row = 0;
            Talent3Index2Column = 2;
            Talent3Index2Spells = new List<int>() { 16179, 16214, 16215, 16216, 16217 };
            Talent3Index2Content = $"0 / 5";

            Talent3Row1Spells.AddRange(Talent3Index1Spells);
            Talent3Row1Spells.AddRange(Talent3Index2Spells);

            // Improved Reincarnation
            Talent3Index3Row = 2;
            Talent3Index3Column = 0;
            Talent3Index3Spells = new List<int>() { 16184, 16209 };
            Talent3Index3Content = $"0 / 2";
            // Ancestral Healing
            Talent3Index4Row = 2;
            Talent3Index4Column = 1;
            Talent3Index4Spells = new List<int>() { 16176, 16235, 16240 };
            Talent3Index4Content = $"0 / 3";
            // Totemic Focus
            Talent3Index5Row = 2;
            Talent3Index5Column = 2;
            Talent3Index5Spells = new List<int>() { 16173, 16222, 16223, 16224, 16225 };
            Talent3Index5Content = $"0 / 5";

            Talent3Row2Spells.AddRange(Talent3Index3Spells);
            Talent3Row2Spells.AddRange(Talent3Index4Spells);
            Talent3Row2Spells.AddRange(Talent3Index5Spells);

            // Nature's Guidance
            Talent3Index6Row = 4;
            Talent3Index6Column = 0;
            Talent3Index6Spells = new List<int>() { 16180, 16196, 16198 };
            Talent3Index6Content = $"0 / 3";
            // Healing Focus
            Talent3Index7Row = 4;
            Talent3Index7Column = 1;
            Talent3Index7Spells = new List<int>() { 16181, 16230, 16232, 16233, 16234 };
            Talent3Index7Content = $"0 / 5";
            // Totemic Mastery
            Talent3Index8Row = 4;
            Talent3Index8Column = 2;
            Talent3Index8Spells = new List<int>() { 16189 };
            Talent3Index8Content = $"0 / 1";
            // Healing Grace
            Talent3Index9Row = 4;
            Talent3Index9Column = 3;
            Talent3Index9Spells = new List<int>() { 29187, 29189, 29191 };
            Talent3Index9Content = $"0 / 3";

            Talent3Row3Spells.AddRange(Talent3Index6Spells);
            Talent3Row3Spells.AddRange(Talent3Index7Spells);
            Talent3Row3Spells.AddRange(Talent3Index8Spells);
            Talent3Row3Spells.AddRange(Talent3Index9Spells);

            // Restorative Totems
            Talent3Index10Row = 6;
            Talent3Index10Column = 1;
            Talent3Index10Spells = new List<int>() { 16187, 16205, 16206, 16207, 16208 };
            Talent3Index10Content = $"0 / 5";
            // Tidal Mastery
            Talent3Index11Row = 6;
            Talent3Index11Column = 2;
            Talent3Index11Spells = new List<int>() { 16194, 16218, 16219, 16220, 16221 };
            Talent3Index11Content = $"0 / 5";

            Talent3Row4Spells.AddRange(Talent3Index10Spells);
            Talent3Row4Spells.AddRange(Talent3Index11Spells);

            // Healing Way
            Talent3Index12Row = 8;
            Talent3Index12Column = 0;
            Talent3Index12Spells = new List<int>() { 29206, 29205, 29202 };
            Talent3Index12Content = $"0 / 3";
            // Nature's Swiftness
            Talent3Index13Row = 8;
            Talent3Index13Column = 2;
            Talent3Index13Spells = new List<int>() { 16188, 16210, 16211, 16212, 16213 };
            Talent3Index13Content = $"0 / 1";

            Talent3Row5Spells.AddRange(Talent3Index12Spells);
            Talent3Row5Spells.AddRange(Talent3Index13Spells);

            // Purification
            Talent3Index14Row = 10;
            Talent3Index14Column = 2;
            Talent3Index14Spells = new List<int>() { 18174, 18175, 18176, 18177, 18178 };
            Talent3Index14Content = $"0 / 5";

            Talent3Row6Spells.AddRange(Talent3Index14Spells);

            // Mana Tide Totem
            Talent3Index15Row = 12;
            Talent3Index15Column = 1;
            Talent3Index15Spells = new List<int>() { 16190 };
            Talent3Index15Content = $"0 / 1";

            Talent3Row7Spells.AddRange(Talent3Index15Spells);


            Talent3Index16Visibility = Visibility.Hidden;
            Talent3Index17Visibility = Visibility.Hidden;
            Talent3Index18Visibility = Visibility.Hidden;
        }
        #region Talent Tree 1
        public override bool Talent1Index1CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index2CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index3CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index4CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index5CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index6CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index7CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index8CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index9CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index10CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index11CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index12CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index13CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index14CanRemove => !Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index15CanRemove => Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index3Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index4Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index5Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index6Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index7Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index8Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index9Enabled => Talent1Spells.Count > 14;
        public override bool Talent1Index10Enabled => Talent1Spells.Count > 14;
        public override bool Talent1Index11Enabled => Talent1Spells.Count > 14;
        public override bool Talent1Index12Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index13Enabled => Talent1Spells.Count > 19;
        public override bool Talent1Index14Enabled => Talent1Spells.Count > 24 && Talent1Index8Spells.All(x => Talent1Spells.Contains(x));
        public override bool Talent1Index15Enabled => Talent1Spells.Count > 29 && Talent1Index13Spells.All(x => Talent1Spells.Contains(x));
        #endregion

        #region Talent Tree 2
        public override bool Talent2Index1CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index2CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index3CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index4CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index5CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index6CanRemove => !Talent2Row3Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index7CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index8CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index9CanRemove => !Talent2Row4Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index10CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index11CanRemove => !Talent2Row5Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index12CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index13CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index14CanRemove => !Talent2Row6Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index15CanRemove => !Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index16CanRemove => Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index3Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index4Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index5Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index6Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index7Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index8Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index9Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index10Enabled => Talent2Spells.Count > 14 && Talent2Index4Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index11Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index12Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index13Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index14Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index15Enabled => Talent2Spells.Count > 24;
        public override bool Talent2Index16Enabled => Talent2Spells.Count > 29 && Talent2Index13Spells.All(x => Talent2Spells.Contains(x));
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
        public override bool Talent3Index12CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index13CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index14CanRemove => !Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index15CanRemove => Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index3Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index4Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index5Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index6Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index7Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index8Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index9Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index10Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index11Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index12Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index13Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index14Enabled => Talent3Spells.Count > 24;
        public override bool Talent3Index15Enabled => Talent3Spells.Count > 29 && Talent3Index10Spells.All(x => Talent3Spells.Contains(x));
        #endregion
    }
}
