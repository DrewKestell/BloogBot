using Communication;
using System.Net.Sockets;
using Google.Protobuf;

namespace WoWStateManager.Communication
{
    public class ActivityManagerService(string server, int port) : IDisposable
    {
        private readonly string _server = server;
        private readonly int _port = port;
        private TcpClient _client = new();
        private NetworkStream _stream;
        private bool _disposed;

        public async Task ConnectAsync()
        {
            await _client.ConnectAsync(_server, _port);
            _stream = _client.GetStream();
            Console.WriteLine("Connected to the server.");
        }

        public async Task DisconnectAsync()
        {
            Dispose();
        }

        public async Task SendMessageAsync(UniversalMessage message)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ActivityManagerService));

            var data = message.ToByteArray();
            await _stream.WriteAsync(BitConverter.GetBytes(data.Length), 0, 4); // Send the length first
            await _stream.WriteAsync(data, 0, data.Length); // Send the actual data
            Console.WriteLine("Message sent.");
        }

        public async Task<UniversalMessage> ReceiveMessageAsync()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ActivityManagerService));

            var lengthBytes = new byte[4];
            var readLengthLength = await _stream.ReadAsync(lengthBytes, 0, 4);

            if (readLengthLength == 0)
                throw new InvalidOperationException("The connection was closed by the server.");

            int length = BitConverter.ToInt32(lengthBytes, 0);

            var messageBytes = new byte[length];
            var readLen = await _stream.ReadAsync(messageBytes, 0, length);

            if (readLen != length)
                throw new InvalidOperationException("The connection was closed by the server.");

            var message = new UniversalMessage();
            message.MergeFrom(messageBytes);

            if (!message.IsInitialized())
                throw new InvalidOperationException("The received message is not valid.");

            return message;
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
                    _stream?.Close();
                    _client?.Close();
                }

                _disposed = true;
            }
        }

        ~ActivityManagerService()
        {
            Dispose(false);
        }
    }
}
