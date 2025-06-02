using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;

namespace BotCommLayer
{
    public class ProtobufSocketServer<TRequest, TResponse>
        where TRequest : IMessage<TRequest>, new()
        where TResponse : IMessage<TResponse>, new()
    {
        private readonly TcpListener _server;
        private bool _isRunning;
        private readonly ILogger _logger;

        public ProtobufSocketServer(string ipAddress, int port, ILogger logger)
        {
            _logger = logger;
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _server.Start();
            _isRunning = true;

            Thread serverThread = new(Run)
            {
                IsBackground = true
            };
            serverThread.Start();
        }

        private void Run()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _server.AcceptTcpClient();
                    ThreadPool.QueueUserWorkItem(_ => HandleClient(client));
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Server error: {ex}");
                }
            }
        }

        /// <summary>
        /// Override this method to provide logic for handling requests.
        /// Remember: if your message uses wrapper types, you must check for null.
        /// </summary>
        protected virtual TResponse HandleRequest(TRequest request)
        {
            _logger.LogWarning("Base HandleRequest called — override this method.");
            return new TResponse();
        }

        private void HandleClient(TcpClient client)
        {
            using NetworkStream stream = client.GetStream();
            try
            {
                while (_isRunning && client.Connected)
                {
                    // Read incoming message length
                    byte[] lengthBuffer = new byte[4];
                    ReadExact(stream, lengthBuffer);
                    int length = BitConverter.ToInt32(lengthBuffer, 0);

                    // Read message payload
                    byte[] buffer = new byte[length];
                    ReadExact(stream, buffer);

                    // Deserialize request
                    TRequest request = new();
                    request.MergeFrom(buffer);

                    // Handle request -> get response
                    TResponse response = HandleRequest(request);

                    // Serialize response
                    byte[] responseBytes = response.ToByteArray();
                    byte[] responseLength = BitConverter.GetBytes(responseBytes.Length);

                    stream.Write(responseLength);
                    stream.Write(responseBytes);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Client connection closed or error occurred: {ex.Message}");
            }
        }

        private static void ReadExact(NetworkStream stream, byte[] buffer)
        {
            int totalRead = 0;
            while (totalRead < buffer.Length)
            {
                int bytesRead = stream.Read(buffer, totalRead, buffer.Length - totalRead);
                if (bytesRead == 0)
                    throw new IOException("Unexpected EOF");
                totalRead += bytesRead;
            }
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
        }
    }
}
