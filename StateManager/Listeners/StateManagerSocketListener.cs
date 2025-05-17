using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class StateManagerSocketListener(string ipAddress, int port, ILogger<StateManagerSocketListener> logger) : ProtobufAsyncSocketServer<StateChangeResponse>(ipAddress, port, logger)
    {
        public Subject<AsyncRequest> DataMessageSubject => _instanceObservable;
    }
}
