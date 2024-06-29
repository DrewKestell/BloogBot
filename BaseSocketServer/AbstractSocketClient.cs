using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BaseSocketServer
{
    public abstract class AbstractSocketClient : IDisposable
    {

        protected readonly int _port;
        protected readonly IPAddress _ipAddress;
        protected readonly int BufferSize = 1024;
        protected Socket _socket;
        protected AbstractSocketClient(int port, IPAddress iPAddress)
        {
            _port = port;
            _ipAddress = iPAddress;
        }

        protected string SendMessage(string message)
        {
            try
            {
                _socket ??= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                if (!_socket.Connected)
                {
                    try
                    {
                        _socket.Connect(_ipAddress, _port);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"SocketClient: {e.Message}");
                    }
                }
                if (_socket.Connected)
                {
                    _socket.Send(Encoding.ASCII.GetBytes(message));

                    byte[] buffer = ReceiveMessage();

                    return Encoding.UTF8.GetString(buffer, 0, buffer.Length);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"SocketClient: {e.Message}");
                try
                {
                    _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                }
                catch
                {
                    _socket.Close();
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
                bytesReceived = _socket.Receive(buffer);

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
        public void Dispose()
        {
            _socket?.Dispose();
        }
    }
}
