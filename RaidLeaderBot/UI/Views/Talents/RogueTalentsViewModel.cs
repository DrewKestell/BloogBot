using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            Talent1Index12Column = 1;
            // Sweeping Strikes
            Talent1Index13Row = 8;
            Talent1Index13Column = 2;

            // Mace Specilization
            Talent1Index14Row = 10;
            Talent1Index14Column = 1;

            // Sword Specialization
            Talent1Index15Row = 12;
            Talent1Index15Column = 1;


            Talent1Index16Visibility = Visibility.Hidden;
            Talent1Index17Visibility = Visibility.Hidden;
            Talent1Index18Visibility = Visibility.Hidden;


            // Booming Voice
            Talent2Index1Row = 0;
            Talent2Index1Column = 0;
            // Cruelty
            Talent2Index2Row = 0;
            Talent2Index2Column = 1;
            // Improved Demoralizing Shout
            Talent2Index3Row = 0;
            Talent2Index3Column = 2;

            // Unbridled Wrath
            Talent2Index4Row = 2;
            Talent2Index4Column = 0;
            // Improved Cleave
            Talent2Index5Row = 2;
            Talent2Index5Column = 1;
            // Piercing Howl
            Talent2Index6Row = 2;
            Talent2Index6Column = 2;

            // Blood Craze
            Talent2Index7Row = 4;
            Talent2Index7Column = 0;
            // Improved Battle Shout
            Talent2Index8Row = 4;
            Talent2Index8Column = 1;
            // Duel Wield Specialization
            Talent2Index9Row = 4;
            Talent2Index9Column = 3;

            // Improved Execute
            Talent2Index10Row = 6;
            Talent2Index10Column = 0;
            // Enrage
            Talent2Index11Row = 6;
            Talent2Index11Column = 1;
            // Improved Slam
            Talent2Index12Row = 6;
            Talent2Index12Column = 2;

            // Death Wish
            Talent2Index13Row = 8;
            Talent2Index13Column = 0;
            // Improved Intercept
            Talent2Index14Row = 8;
            Talent2Index14Column = 1;
            // Improved Berserker Rage
            Talent2Index15Row = 8;
            Talent2Index15Column = 2;
            // Flurry
            Talent2Index16Visibility = Visibility.Visible;
            Talent2Index16Row = 8;
            Talent2Index16Column = 3;

            // Bloodthirst
            Talent2Index17Visibility = Visibility.Visible;
            Talent2Index17Row = 10;
            Talent2Index17Column = 1;
            // 
            Talent2Index18Visibility = Visibility.Visible;
            Talent2Index18Row = 10;
            Talent2Index18Column = 2;

            Talent2Index19Visibility = Visibility.Visible;
            Talent2Index19Row = 12;
            Talent2Index19Column = 1;


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
            Talent3Index4Column = 1;
            // Iron Will
            Talent3Index5Row = 2;
            Talent3Index5Column = 2;

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
            Talent3Index9Row = 6;
            Talent3Index9Column = 0;
            // Improved Sunder Armor
            Talent3Index10Row = 6;
            Talent3Index10Column = 1;
            // Improved Disarm
            Talent3Index11Row = 6;
            Talent3Index11Column = 2;

            // Improved Taunt
            Talent3Index12Row = 8;
            Talent3Index12Column = 0;
            // Improved Shield Wall
            Talent3Index13Row = 8;
            Talent3Index13Column = 1;
            // Concussion Blow
            Talent3Index14Row = 8;
            Talent3Index14Column = 2;
            // Improved Shield Bash
            Talent3Index15Row = 8;
            Talent3Index15Column = 3;

            // One-Handed Specialization
            Talent3Index16Visibility = Visibility.Visible;
            Talent3Index16Row = 10;
            Talent3Index16Column = 2;

            // Shield Slam
            Talent3Index17Visibility = Visibility.Visible;
            Talent3Index17Row = 12;
            Talent3Index17Column = 1;


            Talent3Index18Visibility = Visibility.Hidden;
        }
    }
}
