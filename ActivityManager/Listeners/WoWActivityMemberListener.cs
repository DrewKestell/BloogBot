using BotCommLayer;
using Communication;
using System.Reactive.Subjects;

namespace ActivityManager.Listeners
{
    public class WoWActivityMemberListener(string address, int port) : ProtobufAsyncSocketServer<ActivityMember>(address, port)
    {
        public Subject<DataMessage> DataMessageSubject => _instanceObservable;
    }
}
