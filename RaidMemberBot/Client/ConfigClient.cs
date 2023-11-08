using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaidMemberBot.Client
{
    public class ConfigClient
    {
        private Socket _configSocket;

        readonly int BufferSize = 1024;

        public static ConfigClient Instance { get; private set; } = new ConfigClient();
        public ConfigurationResponse ConfigurationResponse => JsonConvert.DeserializeObject<ConfigurationResponse>(SendRequest(new ConfigurationRequest() { ProcessId = Process.GetCurrentProcess().Id })); 
        private ConfigClient() {
        }

        private string SendRequest(ConfigurationRequest configRequest)
        {
            try
            {
                _configSocket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_configSocket.Connected)
                {
                    try
                    {
                        _configSocket.Connect(IPAddress.Parse(RaidMemberSettings.Instance.ListenAddress), RaidMemberSettings.Instance.ConfigServerPort);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                }
                if (_configSocket.Connected)
                {
                    string databaseRequestJson = JsonConvert.SerializeObject(configRequest);

                    _configSocket.Send(Encoding.ASCII.GetBytes(databaseRequestJson));

                    byte[] buffer = ReceiveMessage();

                    return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                try
                {
                    _configSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _configSocket.Close();
                }
            }

            return string.Empty;
        }

        public byte[] ReceiveMessage()
        {
            byte[] messageBuffer = new byte[BufferSize];
            int totalBytesReceived = 0;

            int bytesReceived;

            int arrayBeginTokens = 0;
            int arrayEndTokens = 0;

            int objectBeginTokens = 0;
            int objectEndTokens = 0;
            do
            {
                byte[] buffer = new byte[BufferSize];

                // Receive at most the requested number of bytes, or the amount the 
                // buffer can hold, whichever is smaller.
                bytesReceived = _configSocket.Receive(buffer);

                if (messageBuffer.Length < totalBytesReceived + bytesReceived)
                {
                    Array.Resize(ref messageBuffer, messageBuffer.Length * 2);
                }

                // Copy the receive buffer into the message buffer, appending after 
                // previously received data (totalBytesReceived).
                Buffer.BlockCopy(buffer, 0, messageBuffer, totalBytesReceived, bytesReceived);

                totalBytesReceived += bytesReceived;

                string s = Encoding.UTF8.GetString(buffer);

                arrayBeginTokens += s.Count(x => x == '[');
                arrayEndTokens += s.Count(x => x == ']');

                objectBeginTokens += s.Count(x => x == '{');
                objectEndTokens += s.Count(x => x == '}');

                if (arrayBeginTokens == arrayEndTokens && objectBeginTokens == objectEndTokens)
                {
                    break;
                }
            } while (bytesReceived == BufferSize);

            Array.Resize(ref messageBuffer, totalBytesReceived);

            return messageBuffer;
        }
    }
}
