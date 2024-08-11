using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace ActivityManager.Listeners
{
    public class WoWActivityMemberListener(string address, int port, ILogger<WoWActivityMemberListener> logger) : ProtobufAsyncSocketServer<ActivityMember>(address, port, logger)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
