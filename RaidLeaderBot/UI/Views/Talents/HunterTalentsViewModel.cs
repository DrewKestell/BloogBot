using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RaidLeaderBot.UI.Views.Talents
{
    public class HunterTalentsViewModel : RaidMemberTalentsViewModel
    {
        public HunterTalentsViewModel()
        {
            Talent1Header = "Beast Mastery";
            Talent2Header = "Marksmanship";
            Talent3Header = "Survival";

            // Improved Heroic Strike
            Talent1Index1Row = 0;
            Talent1Index1Column = 1;
            // Deflection
            Talent1Index2Row = 0;
            Talent1Index2Column = 2;

            // Improved Rend
            Talent1Index3Row = 2;
            Talent1Index3Column = 0;
            // Improved Charge
            Talent1Index4Row = 2;
            Talent1Index4Column = 1;
            // Tactical Mastery
            Talent1Index5Row = 2;
            Talent1Index5Column = 2;
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
            Talent1Index14Column = 3;

            // Sword Specialization
            Talent1Index15Row = 10;
            Talent1Index15Column = 2;

            // Polearm Specialization
            Talent1Index16Visibility = Visibility.Visible;
            Talent1Index16Row = 12;
            Talent1Index16Column = 1;

            // Improved Hamstring
            Talent1Index17Visibility = Visibility.Hidden;
            Talent1Index18Visibility = Visibility.Hidden;

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
            Talent2Index7Column = 3;

            // Improved Battle Shout
            Talent2Index8Row = 6;
            Talent2Index8Column = 1;
            // Duel Wield Specialization
            Talent2Index9Row = 6;
            Talent2Index9Column = 2;

            // Improved Execute
            Talent2Index10Row = 8;
            Talent2Index10Column = 0;
            // Enrage
            Talent2Index11Row = 8;
            Talent2Index11Column = 1;
            // Improved Slam
            Talent2Index12Row = 8;
            Talent2Index12Column = 2;

            // Death Wish
            Talent2Index13Row = 10;
            Talent2Index13Column = 2;

            // Improved Intercept
            Talent2Index14Row = 12;
            Talent2Index14Column = 1;

            Talent2Index15Visibility = Visibility.Hidden;
            Talent2Index16Visibility = Visibility.Hidden;
            Talent2Index17Visibility = Visibility.Hidden;
            Talent2Index18Visibility = Visibility.Hidden;
            Talent2Index19Visibility = Visibility.Hidden;


            // Shield Specialization
            Talent3Index1Row = 0;
            Talent3Index1Column = 0;
            // Anticipation
            Talent3Index2Row = 0;
            Talent3Index2Column = 1;
            // Improved Bloodrage
            Talent3Index3Row = 0;
            Talent3Index3Column = 2;

            // Toughness
            Talent3Index4Row = 2;
            Talent3Index4Column = 0;
            // Iron Will
            Talent3Index5Row = 2;
            Talent3Index5Column = 1;
            // Last Stand
            Talent3Index6Row = 2;
            Talent3Index6Column = 2;

            // Improved Shield Block
            Talent3Index7Row = 4;
            Talent3Index7Column = 0;
            // Improved Revenge
            Talent3Index8Row = 4;
            Talent3Index8Column = 1;
            // Defiance
            Talent3Index9Row = 4;
            Talent3Index9Column = 2;

            // Improved Sunder Armor
            Talent3Index10Row = 6;
            Talent3Index10Column = 0;
            // Improved Disarm
            Talent3Index11Row = 6;
            Talent3Index11Column = 1;
            // Improved Taunt
            Talent3Index12Row = 6;
            Talent3Index12Column = 3;

            // Improved Shield Wall
            Talent3Index13Row = 8;
            Talent3Index13Column = 1;
            // Concussion Blow
            Talent3Index14Row = 8;
            Talent3Index14Column = 2;

            // Improved Shield Bash
            Talent3Index15Row = 10;
            Talent3Index15Column = 2;

            // One-Handed Specialization
            Talent3Index16Visibility = Visibility.Visible;
            Talent3Index16Row = 12;
            Talent3Index16Column = 1;

            Talent3Index17Visibility = Visibility.Hidden;
            Talent3Index18Visibility = Visibility.Hidden;
        }
    }
}
