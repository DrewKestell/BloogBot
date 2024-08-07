using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace StateManager.Listeners
{
    public class StateManagerSocketListener(string ipAddress, int port) : ProtobufAsyncSocketServer<WorldState>(ipAddress, port)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
