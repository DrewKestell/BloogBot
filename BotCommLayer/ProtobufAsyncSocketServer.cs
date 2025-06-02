using Communication;
using Google.Protobuf;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;

namespace BotCommLayer
{
    public class ProtobufAsyncSocketServer<T> where T : IMessage<T>
    {
        private readonly TcpListener _server;
        private bool _isRunning;
        private readonly ILogger _logger;
        private readonly Dictionary<ulong, TcpClient> _clients;
        protected Subject<AsyncRequest> _instanceObservable;

        public ProtobufAsyncSocketServer(string ipAddress, int port, ILogger logger)
        {
            _logger = logger;
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _server.Start();

            _isRunning = true;

            _instanceObservable = new();
            _clients = [];

            Thread serverThread = new(Run);
            serverThread.Start();
        }

        private void Run()
        {
            while (_isRunning)
            {
                try
                {
                    TcpClient client = _server.AcceptTcpClient();
                    Thread clientThread = new(() => HandleClient(client));
                    clientThread.Start();
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error: {ex.Message}");
                }
            }
        }

        public void SendMessageToClient(ulong id, T ouboundMessage)
        {
            if (_clients.TryGetValue(id, out TcpClient client))
            {
                NetworkStream stream = client.GetStream();
                // Serialize the response
                byte[] responseBytes = ouboundMessage.ToByteArray();
                byte[] responseLength = BitConverter.GetBytes(responseBytes.Length);

                // Send the length of the response message
                stream.Write(responseLength, 0, responseLength.Length);

                // Send the response message
                stream.Write(responseBytes, 0, responseBytes.Length);
            }
        }

        private void HandleClient(TcpClient client)
        {
            NetworkStream stream = client.GetStream();
            while (true)
            {
                try
                {
                    // Read the length of the incoming message
                    byte[] lengthBuffer = new byte[4];
                    int bytesRead = stream.Read(lengthBuffer, 0, lengthBuffer.Length);
                    if (bytesRead == 0) break;

                    int length = BitConverter.ToInt32(lengthBuffer, 0);

                    // Read the message itself
                    byte[] buffer = new byte[length];
                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    // Deserialize the request
                    AsyncRequest request = new();
                    request.MergeFrom(buffer);

                    _clients.TryAdd(request.Id, client);

                    // Process
                    _instanceObservable.OnNext(request);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Client Error: {ex}");
                    break;
                }
            }
            client.Close();
        }

        public void Stop()
        {
            _isRunning = false;
            _server.Stop();
        }
    }
}