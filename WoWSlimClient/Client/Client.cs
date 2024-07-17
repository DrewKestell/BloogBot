using System.Net.Sockets;

namespace WoWSlimClient
{
    public class Client : IDisposable
    {
        private TcpClient _tcpClient;
        private NetworkStream _networkStream;
        private Thread _receiveThread;
        private bool _disposed;

        public bool IsConnected => _tcpClient != null && _tcpClient.Connected;

        public void Connect(string serverAddress, int port)
        {
            _tcpClient = new TcpClient();
            _tcpClient.Connect(serverAddress, port);
            _networkStream = _tcpClient.GetStream();

            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.Start();
        }

        public void Disconnect()
        {
            _networkStream?.Close();
            _tcpClient?.Close();

            if (_receiveThread != null && _receiveThread.IsAlive)
            {
                _receiveThread.Abort();
            }
        }

        public void SendPacket(byte[] packet)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to the server.");

            _networkStream.Write(packet, 0, packet.Length);
        }

        public byte[] ReceivePacket()
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to the server.");

            byte[] lengthBuffer = new byte[2];
            _networkStream.Read(lengthBuffer, 0, 2);
            int packetLength = BitConverter.ToUInt16(lengthBuffer, 0);

            byte[] packetBuffer = new byte[packetLength];
            _networkStream.Read(packetBuffer, 0, packetLength);

            return packetBuffer;
        }

        private void ReceiveLoop()
        {
            while (IsConnected)
            {
                try
                {
                    byte[] packet = ReceivePacket();
                    OnPacketReceived(packet);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiving packet: {ex.Message}");
                    Disconnect();
                }
            }
        }

        protected virtual void OnPacketReceived(byte[] packet)
        {
            // Override in derived classes to handle received packets
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _networkStream?.Dispose();
                    _tcpClient?.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
