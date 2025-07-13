using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace BotCommLayer
{
    public class ProtobufSocketClient<TRequest, TResponse>
        where TRequest : IMessage<TRequest>, new()
        where TResponse : IMessage<TResponse>, new()
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly ILogger _logger;
        private readonly object _lock = new(); 
        public ProtobufSocketClient() { }
        public ProtobufSocketClient(string ipAddress, int port, ILogger logger)
        {
            _logger = logger;
            _client = new TcpClient();
            _client.Connect(IPAddress.Parse(ipAddress), port);
            _stream = _client.GetStream();
            _stream.ReadTimeout = 5000;
            _stream.WriteTimeout = 5000;
        }

        public TResponse SendMessage(TRequest request)
        {
            lock (_lock)
            {
                try
                {
                    byte[] messageBytes = request.ToByteArray();
                    byte[] length = BitConverter.GetBytes(messageBytes.Length);

                    _stream.Write(length);
                    _stream.Write(messageBytes);

                    byte[] lengthBuffer = new byte[4];
                    ReadExact(_stream, lengthBuffer);
                    int responseLength = BitConverter.ToInt32(lengthBuffer, 0);

                    byte[] buffer = new byte[responseLength];
                    ReadExact(_stream, buffer);

                    TResponse response = new();
                    response.MergeFrom(buffer);

                    return response;
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error sending message: {ex}");
                    throw;
                }
            }
        }

        private static void ReadExact(NetworkStream stream, byte[] buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = stream.Read(buffer, totalRead, buffer.Length - totalRead);
                if (bytesRead == 0)
                    throw new IOException("Unexpected EOF while reading stream.");
                totalRead += bytesRead;
            }
        }

        public void Close()
        {
            _stream.Close();
            _client.Close();
        }
    }
}
