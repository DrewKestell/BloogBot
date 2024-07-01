using System.Net;
using WoWActivityManager.Clients;
using WoWActivityManager.Listeners;
using WoWActivityMember.Models;

namespace WoWActivityManager
{
    public class WoWActivityManager
    {
        private Task _updateCurrentStateTask;
        private WoWActivityMemberListener _woWActivityMemberListener;
        private WoWStateManagerClient _stateManagerClient;

        private ActivityState ActivityState { get; set; }
        protected ActivityState CurrentActivity { get; }

        public WoWActivityManager()
        {
            _woWActivityMemberListener = new WoWActivityMemberListener();
            _stateManagerClient = new WoWStateManagerClient(8089, IPAddress.Parse("127.0.0.1"));

            _updateCurrentStateTask = Task.Factory.StartNew(() => UpdateCurrentState());
        }

        public async Task UpdateCurrentState()
        {

            while (true)
            {
                ActivityState desiredActivityState = _stateManagerClient.SendCurrentActivityState(ActivityState);



                await Task.Delay(500);
            }
        }

        protected void OnInstanceUpdate(ActivityMemberState state)
        {

        }
    }
}
