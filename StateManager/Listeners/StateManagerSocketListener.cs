using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class StateManagerSocketListener(string ipAddress, int port, ILogger<StateManagerSocketListener> logger) : ProtobufAsyncSocketServer<WorldState>(ipAddress, port, logger)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
