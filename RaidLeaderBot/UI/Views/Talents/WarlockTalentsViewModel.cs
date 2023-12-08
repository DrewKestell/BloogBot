using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaidLeaderBot.UI.Views.Talents
{
    public class WarlockTalentsViewModel : RaidMemberTalentsViewModel
    {
        public WarlockTalentsViewModel(RaidMemberPreset raidMemberPreset)
        {
            RaidMemberPreset = raidMemberPreset;

            Talent1Header = "Affliction";
            Talent2Header = "Demonology";
            Talent3Header = "Destruction";

            // Suppression
            Talent1Index1Row = 0;
            Talent1Index1Column = 1;
            Talent1Index1Spells = new List<int>() { 18174, 18175, 18176, 18177, 18178 };
            Talent1Index1Content = $"0 / 5";
            // Improved Corruption
            Talent1Index2Row = 0;
            Talent1Index2Column = 2;
            Talent1Index2Spells = new List<int>() { 17810, 17811, 17812, 17813, 17814 };
            Talent1Index2Content = $"0 / 5";

            Talent1Row1Spells.AddRange(Talent1Index1Spells);
            Talent1Row1Spells.AddRange(Talent1Index2Spells);

            // Improved Curse of Weakness
            Talent1Index3Row = 2;
            Talent1Index3Column = 0;
            Talent1Index3Spells = new List<int>() { 18179, 18180, 18181 };
            Talent1Index3Content = $"0 / 3";
            // Improved Drain Soul
            Talent1Index4Row = 2;
            Talent1Index4Column = 1;
            Talent1Index4Spells = new List<int>() { 18213, 18372 };
            Talent1Index4Content = $"0 / 2";
            // Improved Life Tap
            Talent1Index5Row = 2;
            Talent1Index5Column = 2;
            Talent1Index5Spells = new List<int>() { 18182, 18183 };
            Talent1Index5Content = $"0 / 2";
            // Improved Drain Life
            Talent1Index6Row = 2;
            Talent1Index6Column = 3;
            Talent1Index6Spells = new List<int>() { 17804, 17805, 17806, 17807, 17808 };
            Talent1Index6Content = $"0 / 5";

            Talent1Row2Spells.AddRange(Talent1Index3Spells);
            Talent1Row2Spells.AddRange(Talent1Index4Spells);
            Talent1Row2Spells.AddRange(Talent1Index5Spells);
            Talent1Row2Spells.AddRange(Talent1Index6Spells);

            // Improved Curse of Agony
            Talent1Index7Row = 4;
            Talent1Index7Column = 0;
            Talent1Index7Spells = new List<int>() { 18827, 18829, 18830 };
            Talent1Index7Content = $"0 / 3";
            // Fel Concentration
            Talent1Index8Row = 4;
            Talent1Index8Column = 1;
            Talent1Index8Spells = new List<int>() { 17783, 17784, 17785, 17786, 17787 };
            Talent1Index8Content = $"0 / 5";
            // Amplify Curse
            Talent1Index9Row = 4;
            Talent1Index9Column = 2;
            Talent1Index9Spells = new List<int>() { 18288 };
            Talent1Index9Content = $"0 / 1";

            Talent1Row3Spells.AddRange(Talent1Index7Spells);
            Talent1Row3Spells.AddRange(Talent1Index8Spells);
            Talent1Row3Spells.AddRange(Talent1Index9Spells);

            // Grim Reach
            Talent1Index10Row = 6;
            Talent1Index10Column = 0;
            Talent1Index10Spells = new List<int>() { 18218, 18219 };
            Talent1Index10Content = $"0 / 2";
            // Nightfall
            Talent1Index11Row = 6;
            Talent1Index11Column = 1;
            Talent1Index11Spells = new List<int>() { 18094, 18095 };
            Talent1Index11Content = $"0 / 2";
            // Improved Drain Mana
            Talent1Index12Row = 6;
            Talent1Index12Column = 3;
            Talent1Index12Spells = new List<int>() { 17864, 18393 };
            Talent1Index12Content = $"0 / 2";

            Talent1Row4Spells.AddRange(Talent1Index10Spells);
            Talent1Row4Spells.AddRange(Talent1Index11Spells);
            Talent1Row4Spells.AddRange(Talent1Index12Spells);

            // Siphon Life
            Talent1Index13Row = 8;
            Talent1Index13Column = 1;
            Talent1Index13Spells = new List<int>() { 18265 };
            Talent1Index13Content = $"0 / 1";
            // Curse of Exhaustion
            Talent1Index14Row = 8;
            Talent1Index14Column = 2;
            Talent1Index14Spells = new List<int>() { 18223 };
            Talent1Index14Content = $"0 / 1";
            // Improved Curse of Exhaustion
            Talent1Index15Row = 8;
            Talent1Index15Column = 3;
            Talent1Index15Spells = new List<int>() { 18310, 18311, 18312, 18313 };
            Talent1Index15Content = $"0 / 4";

            Talent1Row5Spells.AddRange(Talent1Index13Spells);
            Talent1Row5Spells.AddRange(Talent1Index14Spells);
            Talent1Row5Spells.AddRange(Talent1Index15Spells);

            // Shadow Mastery
            Talent1Index16Visibility = Visibility.Visible;
            Talent1Index16Row = 10;
            Talent1Index16Column = 1;
            Talent1Index16Spells = new List<int>() { 18271, 18272, 18273, 18274, 18275 };
            Talent1Index16Content = $"0 / 5";

            Talent1Row6Spells.AddRange(Talent1Index16Spells);

            // Dark Pact
            Talent1Index17Visibility = Visibility.Visible;
            Talent1Index17Row = 12;
            Talent1Index17Column = 1;
            Talent1Index17Spells = new List<int>() { 18220 };
            Talent1Index17Content = $"0 / 1";

            Talent1Row7Spells.AddRange(Talent1Index17Spells);

            Talent1Index18Visibility = Visibility.Hidden;


            // Improved Healthstone
            Talent2Index1Row = 0;
            Talent2Index1Column = 0;
            Talent2Index1Spells = new List<int>() { 18692, 18693 };
            Talent2Index1Content = $"0 / 2";
            // Improved Imp
            Talent2Index2Row = 0;
            Talent2Index2Column = 1;
            Talent2Index2Spells = new List<int>() { 18694, 18695, 18696 };
            Talent2Index2Content = $"0 / 3";
            // Demonic Embrace
            Talent2Index3Row = 0;
            Talent2Index3Column = 2;
            Talent2Index3Spells = new List<int>() { 18697, 18698, 18699, 18700, 18701 };
            Talent2Index3Content = $"0 / 5";

            // Improved Health Funnel
            Talent2Index4Row = 2;
            Talent2Index4Column = 0;
            Talent2Index4Spells = new List<int>() { 18703, 18704 };
            Talent2Index4Content = $"0 / 2";
            // Improved Voidwalker
            Talent2Index5Row = 2;
            Talent2Index5Column = 1;
            Talent2Index5Spells = new List<int>() { 18705, 18706, 18707 };
            Talent2Index5Content = $"0 / 3";
            // Fel Intellect
            Talent2Index6Row = 2;
            Talent2Index6Column = 2;
            Talent2Index6Spells = new List<int>() { 18731, 18743, 18744, 18745, 18746 };
            Talent2Index6Content = $"0 / 5";

            // Improved Sayaad
            Talent2Index7Row = 4;
            Talent2Index7Column = 0;
            Talent2Index7Spells = new List<int>() { 18754, 18755, 18756 };
            Talent2Index7Content = $"0 / 3";
            // Fel Domination
            Talent2Index8Row = 4;
            Talent2Index8Column = 1;
            Talent2Index8Spells = new List<int>() { 18708 };
            Talent2Index8Content = $"0 / 1";
            // Fel Stamina
            Talent2Index9Row = 4;
            Talent2Index9Column = 2;
            Talent2Index9Spells = new List<int>() { 18748, 18749, 18750, 18751, 18752 };
            Talent2Index9Content = $"0 / 5";

            // Master Summoner
            Talent2Index10Row = 6;
            Talent2Index10Column = 1;
            Talent2Index10Spells = new List<int>() { 18709, 18710 };
            Talent2Index10Content = $"0 / 2";
            // Unholy Power
            Talent2Index11Row = 6;
            Talent2Index11Column = 2;
            Talent2Index11Spells = new List<int>() { 18769, 18770, 18771, 18772, 18773 };
            Talent2Index11Content = $"0 / 5";

            // Improved Subjugate Demon
            Talent2Index12Row = 8;
            Talent2Index12Column = 0;
            Talent2Index12Spells = new List<int>() { 18821, 18822, 18823, 18824, 18825 };
            Talent2Index12Content = $"0 / 5";
            // Demonic Sacrifice
            Talent2Index13Row = 8;
            Talent2Index13Column = 1;
            Talent2Index13Spells = new List<int>() { 18788 };
            Talent2Index13Content = $"0 / 1";
            // Improved Firestone
            Talent2Index14Row = 8;
            Talent2Index14Column = 3;
            Talent2Index14Spells = new List<int>() { 18767, 18768 };
            Talent2Index14Content = $"0 / 2";

            // Master Demonologist
            Talent2Index15Row = 10;
            Talent2Index15Column = 2;
            Talent2Index15Spells = new List<int>() { 23785, 23822, 23823, 23824, 23825 };
            Talent2Index15Content = $"0 / 5";

            // Soul Link
            Talent2Index16Visibility = Visibility.Visible;
            Talent2Index16Row = 12;
            Talent2Index16Column = 1;
            Talent2Index16Spells = new List<int>() { 19028 };
            Talent2Index16Content = $"0 / 1";
            // Improved Bloodstone
            Talent2Index17Visibility = Visibility.Visible;
            Talent2Index17Row = 12;
            Talent2Index17Column = 2;
            Talent2Index17Spells = new List<int>() { 18774, 18775 };
            Talent2Index17Content = $"0 / 2";


            Talent2Index18Visibility = Visibility.Hidden;
            Talent2Index19Visibility = Visibility.Hidden;


            // Improved Shadow Bolt
            Talent3Index1Row = 0;
            Talent3Index1Column = 1;
            Talent3Index1Spells = new List<int>() { 17793, 17796, 17801, 17802, 17803 };
            Talent3Index1Content = $"0 / 5";
            // Cataclysm
            Talent3Index2Row = 0;
            Talent3Index2Column = 2;
            Talent3Index2Spells = new List<int>() { 17778, 17779, 17780, 17781, 17782 };
            Talent3Index2Content = $"0 / 5";

            // Bane
            Talent3Index3Row = 2;
            Talent3Index3Column = 1;
            Talent3Index3Spells = new List<int>() { 17788, 18175, 18176, 18177, 18178 };
            Talent3Index3Content = $"0 / 5";
            // Aftermath
            Talent3Index4Row = 2;
            Talent3Index4Column = 2;
            Talent3Index4Spells = new List<int>() { 18119, 18120, 18121, 18122, 18123 };
            Talent3Index4Content = $"0 / 5";

            // Improved Firebolt
            Talent3Index5Row = 4;
            Talent3Index5Column = 0;
            Talent3Index5Spells = new List<int>() { 18126, 18127 };
            Talent3Index5Content = $"0 / 2";
            // Improved Lash of Pain
            Talent3Index6Row = 4;
            Talent3Index6Column = 1;
            Talent3Index6Spells = new List<int>() { 18128, 18129 };
            Talent3Index6Content = $"0 / 2";
            // Devastation
            Talent3Index7Row = 4;
            Talent3Index7Column = 2;
            Talent3Index7Spells = new List<int>() { 18130, 18131, 18132, 18133, 18134 };
            Talent3Index7Content = $"0 / 5";
            // Shadowburn
            Talent3Index8Row = 4;
            Talent3Index8Column = 3;
            Talent3Index8Spells = new List<int>() { 17877 };
            Talent3Index8Content = $"0 / 1";

            // Intensity
            Talent3Index9Row = 6;
            Talent3Index9Column = 0;
            Talent3Index9Spells = new List<int>() { 18135, 18136 };
            Talent3Index9Content = $"0 / 2";
            // Destructive Reach
            Talent3Index10Row = 6;
            Talent3Index10Column = 1;
            Talent3Index10Spells = new List<int>() { 17917, 17918 };
            Talent3Index10Content = $"0 / 2";
            // Improved Searing Pain
            Talent3Index11Row = 6;
            Talent3Index11Column = 3;
            Talent3Index11Spells = new List<int>() { 17927, 17929, 17930, 17931, 17932 };
            Talent3Index11Content = $"0 / 5";

            // Pyroclasm
            Talent3Index12Row = 8;
            Talent3Index12Column = 0;
            Talent3Index12Spells = new List<int>() { 18096, 18073 };
            Talent3Index12Content = $"0 / 2";
            // Improved Immolate
            Talent3Index13Row = 8;
            Talent3Index13Column = 1;
            Talent3Index13Spells = new List<int>() { 17815, 17833, 17834, 17835, 17836 };
            Talent3Index13Content = $"0 / 5";
            // Ruin
            Talent3Index14Row = 8;
            Talent3Index14Column = 2;
            Talent3Index14Spells = new List<int>() { 17959 };
            Talent3Index14Content = $"0 / 1";

            // Emberstone
            Talent3Index15Row = 10;
            Talent3Index15Column = 2;
            Talent3Index15Spells = new List<int>() { 17954, 17955, 17956, 17957, 17958 };
            Talent3Index15Content = $"0 / 5";

            // Conflagrate
            Talent3Index16Visibility = Visibility.Visible;
            Talent3Index16Row = 12;
            Talent3Index16Column = 1;
            Talent3Index16Spells = new List<int>() { 17962 };
            Talent3Index16Content = $"0 / 1";


            Talent3Index17Visibility = Visibility.Hidden;
            Talent3Index18Visibility = Visibility.Hidden;
        }
        #region Talent Tree 1
        public override bool Talent1Index1CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index2CanRemove => !Talent1Row2Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index3CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index4CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index5CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index6CanRemove => !Talent1Row3Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index7CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index8CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index9CanRemove => !Talent1Row4Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index10CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index11CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index12CanRemove => !Talent1Row5Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index13CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index14CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index15CanRemove => !Talent1Row6Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index16CanRemove => !Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index17CanRemove => Talent1Row7Spells.Any(x => Talent1Spells.Contains(x));
        public override bool Talent1Index3Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index4Enabled => Talent1Spells.Count > 4;
        public override bool Talent1Index5Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index6Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index7Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index8Enabled => Talent1Spells.Count > 9;
        public override bool Talent1Index9Enabled => Talent1Spells.Count > 14 && Talent1Index3Spells.All(x => Talent1Spells.Contains(x));
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
        public override bool Talent2Index3CanRemove => !Talent2Row2Spells.Any(x => Talent2Spells.Contains(x));
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
        public override bool Talent2Index17CanRemove => Talent2Row7Spells.Any(x => Talent2Spells.Contains(x));
        public override bool Talent2Index4Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index5Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index6Enabled => Talent2Spells.Count > 4;
        public override bool Talent2Index7Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index8Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index9Enabled => Talent2Spells.Count > 9;
        public override bool Talent2Index10Enabled => Talent2Spells.Count > 14 && Talent2Index8Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index11Enabled => Talent2Spells.Count > 14;
        public override bool Talent2Index12Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index13Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index14Enabled => Talent2Spells.Count > 19;
        public override bool Talent2Index15Enabled => Talent2Spells.Count > 24 && Talent2Index11Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index16Enabled => Talent2Spells.Count > 29 && Talent2Index13Spells.All(x => Talent2Spells.Contains(x));
        public override bool Talent2Index17Enabled => Talent2Spells.Count > 29;
        #endregion

        #region Talent Tree 3
        public override bool Talent3Index1CanRemove => !Talent3Row2Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index2CanRemove => !Talent3Row2Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index3CanRemove => !Talent3Row3Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index4CanRemove => !Talent3Row3Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index5CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index6CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index7CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index8CanRemove => !Talent3Row4Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index9CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index10CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index11CanRemove => !Talent3Row5Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index12CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index13CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index14CanRemove => !Talent3Row6Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index15CanRemove => !Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index16CanRemove => Talent3Row7Spells.Any(x => Talent3Spells.Contains(x));
        public override bool Talent3Index3Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index4Enabled => Talent3Spells.Count > 4;
        public override bool Talent3Index5Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index6Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index7Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index8Enabled => Talent3Spells.Count > 9;
        public override bool Talent3Index9Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index10Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index11Enabled => Talent3Spells.Count > 14;
        public override bool Talent3Index12Enabled => Talent3Spells.Count > 19 && Talent3Index9Spells.All(x => Talent3Spells.Contains(x));
        public override bool Talent3Index13Enabled => Talent3Spells.Count > 19;
        public override bool Talent3Index14Enabled => Talent3Spells.Count > 19 && Talent3Index7Spells.All(x => Talent3Spells.Contains(x));
        public override bool Talent3Index15Enabled => Talent3Spells.Count > 24;
        public override bool Talent3Index16Enabled => Talent3Spells.Count > 29 && Talent3Index13Spells.All(x => Talent3Spells.Contains(x));
        #endregion
    }
}
