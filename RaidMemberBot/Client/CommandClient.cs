using Newtonsoft.Json;
using RaidMemberBot.Models.Dto;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace RaidMemberBot.Client
{
    public class CommandClient
    {
        private Socket _commandSocket;

        bool isRaidLeader;
        readonly int BufferSize = 1024;

        public static CommandClient Instance { get; private set; } = new CommandClient();

        private CommandClient()
        {
        }

        public InstanceCommand GetCommandBasedOnState(CharacterState characterState)
        {
            isRaidLeader = string.IsNullOrEmpty(characterState.RaidLeader) && characterState.RaidLeader == characterState.CharacterName;

            string json = SendRequest(characterState);

            return JsonConvert.DeserializeObject<InstanceCommand>(json);
        }
        private string SendRequest(CharacterState characterState)
        {
            try
            {
                _commandSocket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_commandSocket.Connected)
                {
                    try
                    {
                        _commandSocket.Connect(IPAddress.Parse(RaidMemberSettings.Instance.ListenAddress), ConfigClient.Instance.ConfigurationResponse.RaidLeaderServerPort);
                    }
                    catch (SocketException e)
                    {
                        Console.WriteLine($"{e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                    }
                }
                if (_commandSocket.Connected)
                {
                    string json = JsonConvert.SerializeObject(characterState);

                    _commandSocket.Send(Encoding.ASCII.GetBytes(json));

                    byte[] buffer = ReceiveMessage();

                    return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine($"{e.Message} {e.ErrorCode} {e.NativeErrorCode} {e.SocketErrorCode}");
                try
                {
                    _commandSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _commandSocket.Close();
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
                bytesReceived = _commandSocket.Receive(buffer);

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
