using System.Net.Sockets;
using Google.Protobuf;

namespace BotCommLayer
{
    public class ProtobufSocketClient<TRequest, TResponse>
        where TRequest : IMessage<TRequest>, new()
        where TResponse : IMessage<TResponse>, new()
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;

        public ProtobufSocketClient(string ipAddress, int port)
        {
            _client = new TcpClient(ipAddress, port);
            _stream = _client.GetStream();
        }

        protected TResponse SendMessage(TRequest request)
        {
            byte[] messageBytes = request.ToByteArray();
            byte[] length = BitConverter.GetBytes(messageBytes.Length);

            // Send the length of the message
            _stream.Write(length, 0, length.Length);

            // Send the message itself
            _stream.Write(messageBytes, 0, messageBytes.Length);

            // Receive the length of the response
            byte[] lengthBuffer = new byte[4];
            int bytesRead = _stream.Read(lengthBuffer, 0, lengthBuffer.Length);
            if (bytesRead == 0) throw new Exception("Connection closed");

            int responseLength = BitConverter.ToInt32(lengthBuffer, 0);

            // Receive the response
            byte[] buffer = new byte[responseLength];
            bytesRead = _stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) throw new Exception("Connection closed");

            // Deserialize the response
            TResponse response = new();
            response.MergeFrom(buffer);
            return response;
        }

        public void Close()
        {
            _stream.Close();
            _client.Close();
        }
    }
}