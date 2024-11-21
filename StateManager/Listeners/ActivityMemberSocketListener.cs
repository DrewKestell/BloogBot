using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class ActivityMemberSocketListener(string ipAddress, int port, ILogger<ActivityMemberSocketListener> logger) : ProtobufAsyncSocketServer<ActivitySnapshot>(ipAddress, port, logger)
    {
        public Subject<AsyncRequest> DataMessageSubject => _instanceObservable;
    }
}
