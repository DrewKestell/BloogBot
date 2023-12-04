using RaidLeaderBot.Pathfinding;
using RaidMemberBot.Objects;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RaidLeaderBot
{
    public partial class RaidActivityManagerWindow : Window
    {
        RaidActivityViewModel _raidActivityPresetViewModel;
        public RaidActivityManagerWindow()
        {
            InitializeComponent();

            _raidActivityPresetViewModel = new RaidActivityViewModel();
            DataContext = _raidActivityPresetViewModel;

            _raidActivityPresetViewModel.Initialize();

            Console.Write($"[RAIDLEADER]Loading navigation tiles...");
            Navigation.Instance.CalculatePath(1, new Position(0,0,0), new Position(0, 0, 0), true);
            Console.WriteLine($" Loaded.");
        }        

        private void RaidPresetGroupBox_MouseLeftClick(object sender, MouseEventArgs e)
        {
            GroupBox groupBox = sender as GroupBox;
            RaidLeaderViewModel raidPresetViewModel = groupBox.DataContext as RaidLeaderViewModel;

            for (int i = 0; i < _raidActivityPresetViewModel.RaidPresetViewModels.Count; i++)
            {
                _raidActivityPresetViewModel.SetRaidFocusState(i, _raidActivityPresetViewModel.RaidPresetViewModels[i].Index == raidPresetViewModel.Index);
            }
        }
        private void MemberPresetGroupBox_MouseLeftClick(object sender, MouseEventArgs e)
        {
            GroupBox groupBox = sender as GroupBox;
            RaidMemberViewModel raidMemberViewModel = groupBox.DataContext as RaidMemberViewModel;

            for (int i = 0; i < _raidActivityPresetViewModel.RaidPresetViewModels[_raidActivityPresetViewModel.SelectedRaidIndex].RaidMemberViewModels.Count; i++)
            {
                _raidActivityPresetViewModel.SetMemberFocusState(i, _raidActivityPresetViewModel.RaidPresetViewModels[_raidActivityPresetViewModel.SelectedRaidIndex].RaidMemberViewModels[i].Index == raidMemberViewModel.Index);
            }
        }
    }
    public enum ActivityType
    {
        [Description("Idle")]
        Idle,
        [Description("Warsong Gultch [10-19]")]
        WarsongGulch19,
        [Description("Warsong Gultch [20-29]")]
        WarsongGulch29,
        [Description("Warsong Gultch [30-39]")]
        WarsongGulch39,
        [Description("Warsong Gultch [40-49]")]
        WarsongGulch49,
        [Description("Warsong Gultch [50-59]")]
        WarsongGulch59,
        [Description("Warsong Gultch [60]")]
        WarsongGulch60,
        [Description("Arathi Basin [20-29]")]
        ArathiBasin29,
        [Description("Arathi Basin [30-39]")]
        ArathiBasin39,
        [Description("Arathi Basin [40-49]")]
        ArathiBasin49,
        [Description("Arathi Basin [50-59]")]
        ArathiBasin59,
        [Description("Arathi Basin [60]")]
        ArathiBasin60,
        [Description("Alterac Valley [51-60]")]
        AlteracValley,
        [Description("Ragefire Chasm [8]")]
        RagefireChasm,
        [Description("Wailing Caverns [10]")]
        WailingCaverns,
        [Description("The Deadmines [10]")]
        TheDeadmines,
        [Description("Shadowfang Keep [10]")]
        ShadowfangKeep,
        [Description("The Stockade [15]")]
        TheStockade,
        [Description("Razorfen Kraul [17]")]
        RazorfenKraul,
        [Description("Blackfathom Deeps [19]")]
        BlackfathomDeeps,
        [Description("Gnomeregan [20]")]
        Gnomeregan,
        [Description("The Scarlet Monastery - Graveyard [20]")]
        SMGraveyard,
        [Description("The Scarlet Monastery - Library [20]")]
        SMLibrary,
        [Description("The Scarlet Monastery - Armory [20]")]
        SMArmory,
        [Description("The Scarlet Monastery - Cathedral [20]")]
        SMCathedral,
        [Description("Razorfen Downs [25]")]
        RazorfenDowns,
        [Description("Uldaman [30]")]
        Uldaman,
        [Description("Zul'Farrak [35]")]
        ZulFarrak,
        [Description("Maraudon - Wicked Grotto [30]")]
        MaraudonWickedGrotto,
        [Description("Maraudon - Foulspore Cavern [30]")]
        MaraudonFoulsporeCavern,
        [Description("Maraudon - Earth Song Falls [30]")]
        MaraudonEarthSongFalls,
        [Description("Temple of Atal'Hakkar [35]")]
        TempleOfAtalHakkar,
        [Description("Blackrock Depths [45]")]
        BlackrockDepths,
        [Description("Lower Blackrock Spire [45]")]
        LowerBlackrockSpire,
        [Description("Upper Blackrock Spire [45]")]
        UpperBlackrockSpire,
        [Description("Dire Maul [45]")]
        DireMaul,
        [Description("Stratholme - Alive [45]")]
        StratholmeAlive,
        [Description("Stratholme - Undead [45]")]
        StratholmeUndead,
        [Description("Scholomance [45]")]
        Scholomance,
        [Description("Onyxia's Lair [50]")]
        OnyxiasLair,
        [Description("Zul'Gurub [50]")]
        ZulGurub,
        [Description("Molten Core [50]")]
        MoltenCore,
        [Description("Blackwing Lair [60]")]
        BlackwingLair,
        [Description("Ruins of Ahn'Qiraj [60]")]
        RuinsOfAhnQiraj,
        [Description("Temple of Ahn'Qiraj [60]")]
        TempleOfAhnQiraj,
        [Description("Naxxramas [8]")]
        Naxxramas,
    }
}
