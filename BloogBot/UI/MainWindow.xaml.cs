using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace BloogBot.UI
{
    public partial class MainWindow : Window
    {
        private static MainViewModel mainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            mainViewModel = (MainViewModel)DataContext;

            // make sure the output window stays scrolled to the bottom
            DispatcherTimer timer = new DispatcherTimer
            {
                Interval = new TimeSpan(0, 0, 2)
            };
            timer.Tick += (sender, e) =>
            {
                //if (Console.VerticalOffset == Console.ScrollableHeight)
                //    Console.ScrollToEnd();

                Process[] wowProcesses = Process.GetProcessesByName("WoW");

                foreach (Process process in wowProcesses)
                {
                    if (!mainViewModel.WoWProcessList.Contains(process.Id.ToString()))
                    {
                        mainViewModel.WoWProcessList.Add(process.Id.ToString());
                    }
                }
                for (int i = 0; i < mainViewModel.WoWProcessList.Count;)
                {
                    if (wowProcesses.All(x => x.Id.ToString() != mainViewModel.WoWProcessList[i]))
                    {
                        mainViewModel.WoWProcessList.RemoveAt(i);
                    }
                    else
                    {
                        i++;
                    }
                }

                //string[] results = Functions.LuaCallWithResult($"{{0}} = GetMouseFocus():GetName()");
                //if (results.Length > 0)
                //{
                //    Logger.Log(results[0]);
                //}
            };
            timer.Start();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Functions.LuaCall(
                "    local username = 'lrhodes404' " +
                "\r\nlocal password = 'Rockydog1.'" +
                "\r\nAccountLoginAccountEdit:SetText(username)" +
                "\r\nAccountLoginPasswordEdit:SetText(password)" +
                "\r\nAccountLogin_Login()");

            while(Wait.For("FadeFrameAnim", 2500))
            {
               
            }

            Functions.LuaCall("CharacterSelect_EnterWorld()");
        }
        public bool IsElementVisibile(string elementName)
        {
            var hasOption = Functions.LuaCallWithResult($"{{0}} = {elementName}:IsVisible()");

            return hasOption.Length > 0 && hasOption[0] == "1";
        }
    }
}
