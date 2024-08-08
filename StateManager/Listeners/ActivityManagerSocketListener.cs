using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class ActivityManagerSocketListener(string ipAddress, int port, ILogger<ActivityManagerSocketListener> logger) : ProtobufAsyncSocketServer<ActivityMember>(ipAddress, port, logger)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
