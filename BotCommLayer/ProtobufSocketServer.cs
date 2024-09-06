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
            _server = new TcpListener(IPAddress.Parse(ipAddress), port);
            _server.Start();
            _isRunning = true;

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
                    Console.WriteLine($"Error: {ex}");
                }
            }
        }

        protected virtual TResponse HandleRequest(TRequest request)
        {
            Console.WriteLine(request.Descriptor.Name); // Get the name of the message type
            // Implement request handling logic and generate response
            // Example:
            return new TResponse();
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
                    TRequest request = new();
                    request.MergeFrom(buffer);

                    // Process
                    TResponse response = HandleRequest(request);

                    // Serialize the response
                    byte[] responseBytes = response.ToByteArray();
                    byte[] responseLength = BitConverter.GetBytes(responseBytes.Length);

                    // Send the length of the response message
                    stream.Write(responseLength, 0, responseLength.Length);

                    // Send the response message
                    stream.Write(responseBytes, 0, responseBytes.Length);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Client Error: {ex}");
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