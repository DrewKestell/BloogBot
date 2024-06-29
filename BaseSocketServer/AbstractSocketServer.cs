using System.Net;
using System.Net.Sockets;
using System.Text;

namespace BaseSocketServer
{
    public abstract class AbstractSocketServer(int port, IPAddress ipAddress) : IDisposable
    {
        protected readonly int _port = port;
        protected readonly IPAddress _ipAddress = ipAddress;
        protected readonly Dictionary<int, Socket> _processIds = [];
        protected Socket _connectionSocket;
        private Task _backgroundTask;
        private bool _listen;

        readonly int BufferSize = 1024;

        public List<int> ConnectedProcessIds { get { return [.. _processIds.Keys]; } }
        public void Start()
        {
            _connectionSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _connectionSocket.Bind(new IPEndPoint(_ipAddress, _port));
            _connectionSocket.Listen(10);
            _listen = true;
            _processIds.Clear();
            _backgroundTask = Task.Run(StartAsync);
        }

        public void Stop()
        {
            _listen = false;
            _backgroundTask?.Wait(TimeSpan.FromSeconds(5));
            _connectionSocket?.Close();
        }

        public abstract int HandleRequest(string payload, Socket clientSocket);

        private async Task StartAsync()
        {
            Console.WriteLine($"{DateTime.Now}|[SOCKET SERVER : {_port}]Starting listener on port {_port}");
            while (_listen)
            {
                Socket clientSocket = _connectionSocket.Accept();
                ThreadPool.QueueUserWorkItem(_ => HandleClient(clientSocket));
                await Task.Delay(100);
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            int processId = 0;
            while (_listen)
            {
                try
                {
                    processId = HandleRequest(Encoding.UTF8.GetString(ReceiveMessage(clientSocket)), clientSocket);
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"{DateTime.Now}|[SOCKET SERVER : {_port}]Process {processId} disconnected due to {e.SocketErrorCode}");
                    _processIds.Remove(processId);
                    clientSocket.Close();
                    return;
                }
            }
            _processIds.Remove(processId);
            clientSocket.Close();
        }
        protected static int SendMessage(string message, Socket clientSocket)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(message);
            while (bytes.Length > 1024)
            {
                byte[] tempBytes = new byte[1024];
                Array.Copy(bytes, tempBytes, 1024);

                clientSocket.Send(tempBytes);

                Array.Reverse(bytes);
                Array.Resize(ref bytes, bytes.Length - 1024);
                Array.Reverse(bytes);
            }
            if (bytes.Length > 0)
            {
                clientSocket.Send(bytes);
            }
            return 0;
        }

        public byte[] ReceiveMessage(Socket clientSocket)
        {
            byte[] messageBuffer = new byte[BufferSize];
            int totalBytesReceived = 0;

            int arrayBeginTokens = 0;
            int arrayEndTokens = 0;

            int objectBeginTokens = 0;
            int objectEndTokens = 0;

            int bytesReceived;
            do
            {
                byte[] buffer = new byte[BufferSize];

                // Receive at most the requested number of bytes, or the amount the 
                // buffer can hold, whichever is smaller.
                bytesReceived = clientSocket.Receive(buffer);

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
            Stop();
        }
    }
}
