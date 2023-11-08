using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RaidLeaderBot
{
    public abstract class BaseSocketServer : IDisposable
    {
        protected readonly int _port;
        protected readonly IPAddress _ipAddress;
        protected readonly Dictionary<int, Socket> _processIds = new Dictionary<int, Socket>();
        protected Socket _listener;
        private Task _backgroundTask;
        private bool _listen;

        readonly int BufferSize = 1024;

        public BaseSocketServer(int port, IPAddress ipAddress)
        {
            _port = port;
            _ipAddress = ipAddress;
        }
        public List<int> ConnectedProcessIds { get { return _processIds.Keys.ToList(); } }
        public void Start()
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            _listener.Bind(new IPEndPoint(_ipAddress, _port));
            _listener.Listen(10);
            _listen = true;
            _processIds.Clear();
            _backgroundTask = Task.Run(StartAsync);
        }

        public void Stop()
        {
            _listen = false;
            if (_backgroundTask != null)
            {
                _backgroundTask.Wait(TimeSpan.FromSeconds(5));
            }
            _listener?.Close();
            _listener = null;
        }

        public abstract int HandleRequest(string payload, Socket clientSocket);

        private async Task StartAsync()
        {
            while (_listen)
            {
                Socket clientSocket = _listener.Accept();
                ThreadPool.QueueUserWorkItem(_ => HandleClient(clientSocket));
                await Task.Delay(100);
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            int processId = 0;

            Console.WriteLine($"New process connected");
            while (_listen)
            {
                try
                {
                    processId = HandleRequest(Encoding.UTF8.GetString(ReceiveMessage(clientSocket)), clientSocket);
                }
                catch (SocketException e)
                {
                    Console.WriteLine($"Process {processId} disconnected due to {e.SocketErrorCode}");
                    _processIds.Remove(processId);
                    clientSocket.Close();
                    return;
                }
            }
            _processIds.Remove(processId);
            clientSocket.Close();
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
