using Google.Protobuf;

namespace BotCommLayer.Models
{
    public interface IProtobufMessage<T> where T : IMessage<T>
    {
        T ParseFrom(byte[] data);
        byte[] ToByteArray();
    }

}
