using System.Net;
using WoWActivityManager.Clients;
using WoWActivityManager.Listeners;
using WoWActivityMember.Models;

namespace WoWActivityManager
{
    public class WoWActivityManager
    {
        private WoWActivityMemberListener _woWActivityMemberListener;
        private readonly WoWStateManagerClient _stateManagerClient;
        ActivityState ActivityState { get; set; }
        protected ActivityState CurrentActivity { get; }

        public WoWActivityManager(IPAddress listenAddress, int listenPort, IPAddress stateManagerAddress, int stateManagerPort)
        {
            _woWActivityMemberListener = new WoWActivityMemberListener(listenAddress, listenPort);
            _stateManagerClient = new WoWStateManagerClient(stateManagerPort, stateManagerAddress);
        }

        public async Task UpdateCurrentState(CancellationToken cancellationToken)
        {
            var disposable = _woWActivityMemberListener.InstanceUpdateObservable.Subscribe(x =>
            {
                foreach (var objActivityMemberState in x.ActivityMemberStates)
                {
                    //OnInstanceUpdate(objActivityMemberState);
                }
            });

            while (!cancellationToken.IsCancellationRequested)
            {
                ActivityState desiredActivityState = _stateManagerClient.SendCurrentActivityState(ActivityState);



                await Task.Delay(500, cancellationToken);
            }

            disposable.Dispose();
        }

        protected void OnInstanceUpdate(ActivityMemberState state)
        {
            //do stuff?
        }
    }
}
