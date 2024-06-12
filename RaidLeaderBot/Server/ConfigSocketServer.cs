using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaidLeaderBot.Server
{
    public class ConfigSocketServer : BaseSocketServer
    {
        private Dictionary<int, List<int>> CommandPortToProcessIds = new Dictionary<int, List<int>>();

        public static ConfigSocketServer Instance { get; private set; } = new ConfigSocketServer(RaidLeaderBotSettings.Instance.ConfigServerPort, IPAddress.Parse(RaidLeaderBotSettings.Instance.ListenAddress));

        private ConfigSocketServer(int port, IPAddress ipAddress) : base(port, ipAddress)
        {
            Console.WriteLine($"[CONFIG SERVER {port}] Started");
        }
        public void AddProcessToCommandPortMapping(int processId, int commandPortNumber)
        {
            if (!CommandPortToProcessIds.ContainsKey(commandPortNumber))
            {
                CommandPortToProcessIds.Add(commandPortNumber, new List<int>());
            }

            if (CommandPortToProcessIds.TryGetValue(commandPortNumber, out List<int> processIds))
            {
                processIds.Add(processId);
            }
        }
        public void RemoveProcessToCommandPortMapping(int processId, int commandPortNumber)
        {
            if (CommandPortToProcessIds.TryGetValue(commandPortNumber, out List<int> processIds))
            {
                processIds.Remove(processId);

                if (processIds.Count == 0)
                {
                    CommandPortToProcessIds.Remove(commandPortNumber);
                }
            }
        }
        public override int HandleRequest(string payload, Socket clientSocket)
        {
            if (string.IsNullOrEmpty(payload))
                return 0;

            ConfigurationRequest request = JsonConvert.DeserializeObject<ConfigurationRequest>(payload);
            int raidLeaderPort = 0;

            foreach(var keyValues in CommandPortToProcessIds)
            {
                if (keyValues.Value.Contains(request.ProcessId))
                {
                    raidLeaderPort = keyValues.Key;
                    break;
                }
            }

            ConfigurationResponse configurationResponse = new ConfigurationResponse() {
                ActivityManagerServerPort = raidLeaderPort,
                DatabaseServerPort = RaidLeaderBotSettings.Instance.DatabasePort,
                NavigationServerPort = RaidLeaderBotSettings.Instance.NavigationPort
            };

            string response = JsonConvert.SerializeObject(configurationResponse);
            Console.WriteLine($"[CONFIG SERVER {_port}]:{response}");

            byte[] bytes = Encoding.ASCII.GetBytes(response);
            int totalSent = 0;

            while (bytes.Length > 1024)
            {
                byte[] tempBytes = new byte[1024];
                Array.Copy(bytes, tempBytes, 1024);

                totalSent += clientSocket.Send(tempBytes);

                Array.Reverse(bytes);
                Array.Resize(ref bytes, bytes.Length - 1024);
                Array.Reverse(bytes);
            }
            if (bytes.Length > 0)
            {
                totalSent += clientSocket.Send(bytes);
            }
            return 0;
        }
    }
}
