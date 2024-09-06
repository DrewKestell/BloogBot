using BotCommLayer;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class ActivityMemberSocketListener(string ipAddress, int port, ILogger<ActivityMemberSocketListener> logger) : ProtobufAsyncSocketServer<ActivityMember>(ipAddress, port, logger)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
