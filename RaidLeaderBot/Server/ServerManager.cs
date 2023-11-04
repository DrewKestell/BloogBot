using RaidMemberBot.Game.Statics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RaidLeaderBot.Server
{
    internal class ServerManager
    {
        private CommandSockerServer _commandSockerServer;
        private NavigationSocketServer _navigationSocketServer;
        private DatabaseSocketServer _databaseSocketServer;
        private static Lazy<ServerManager> _instance = new Lazy<ServerManager>(() => new ServerManager());
        public static ServerManager Instance => _instance.Value;
        private ServerManager() {
            _commandSockerServer = new CommandSockerServer(RaidLeaderBotSettings.Instance.CommandPort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
            _navigationSocketServer = new NavigationSocketServer(RaidLeaderBotSettings.Instance.PathfindingPort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));
            _databaseSocketServer = new DatabaseSocketServer(RaidLeaderBotSettings.Instance.DatabasePort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));

            _commandSockerServer.Start();
            _navigationSocketServer.Start();
            _databaseSocketServer.Start();
        }
    }
}
