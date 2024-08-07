using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class ActivityManagerSocketListener(string ipAddress, int port) : ProtobufAsyncSocketServer<ActivityMember>(ipAddress, port)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
