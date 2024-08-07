using System.Collections.ObjectModel;
using ActivityManager.Clients;
using BotRunner.Interfaces;
using Communication;
using DatabaseDomain.Client;

namespace BotRunner
{
    public class BotRunner
    {
        private readonly BotLoader botLoader = new();

        private readonly Stack<IBotTask> botTasks = new();
        private readonly ObservableCollection<IBot> Bots = [];

        private readonly ActivityMemberState _currentActivityMemberState;
        private readonly ActivityMember _desiredActivityMemberState;

        private readonly ActivityManagerClient _activityCommandClient;
        private readonly WoWDatabaseClient _wowDatabaseClient;

        private readonly IObjectManager _objectManager;
        private IClassContainer _classContainer;

        private readonly Task _asyncBotTaskRunnerTask;
        private readonly Task _asyncServerFeedbackTask;

        // General
        private IBot currentBot;

        public BotRunner(IObjectManager objectManager, IWoWEventHandler wowEventHandler)
        {
            _objectManager = objectManager;
            Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

            _currentActivityMemberState = new ActivityMemberState()
            {

            };

            _objectManager.Initialize(_currentActivityMemberState, wowEventHandler);

            wowEventHandler.OnPartyInvite += (sender, args) =>
            {
                _objectManager.AcceptGroupInvite();
            };

            _asyncBotTaskRunnerTask = StartBotTaskRunnerAsync();
            _asyncServerFeedbackTask = StartServerFeedbackAsync();
        }
        private async Task StartServerFeedbackAsync()
        {
            Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Start server feedback task started.");
            while (true)
            {
                try
                {
                    ActivityMember incomingActivityMemberState = _activityCommandClient.SendCurrentStateToActivityManager(_currentActivityMemberState);
                    _desiredActivityMemberState.AccountName = incomingActivityMemberState.AccountName;
                    _desiredActivityMemberState.BehaviorProfile = incomingActivityMemberState.BehaviorProfile;
                    _desiredActivityMemberState.ProgressionProfile = incomingActivityMemberState.ProgressionProfile;
                    _desiredActivityMemberState.InitialProfile = incomingActivityMemberState.InitialProfile;
                    _desiredActivityMemberState.EndStateProfile = incomingActivityMemberState.EndStateProfile;
                }
                catch (Exception e)
                {

                }

                await Task.Delay(500);
            }
        }

        private async Task StartBotTaskRunnerAsync()
        {
            Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Bot Task Runner started.");
            while (true)
            {
                try
                {
                    //ThreadSynchronizer.RunOnMainThread(() =>
                    //{
                    //    if (Wait.For("AntiAFK", 5000, true))
                    //    {
                    //        ObjectManager.AntiAfk();
                    //    }

                    //    foreach (var activityMember in _desiredActivityMemberState.ActivityMembers)
                    //        if (!_currentActivityMemberState.ActivityMembers.Contains(activityMember))
                    //            botTasks.Push(new AddPartyMemberTask(_classContainer, botTasks, activityMember));

                    //    foreach (var skill in _desiredActivityMemberState.Skills)
                    //        if (!_currentActivityMemberState.Skills.Contains(skill))
                    //            botTasks.Push(new AddSpellTask(_classContainer, botTasks, skill));

                    //    foreach (var spell in _desiredActivityMemberState.Spells)
                    //        if (!_currentActivityMemberState.Spells.Contains(spell))
                    //            botTasks.Push(new AddSpellTask(_classContainer, botTasks, spell));

                    //    foreach (var talent in _desiredActivityMemberState.Talents)
                    //        if (!_currentActivityMemberState.Talents.Contains(talent))
                    //            botTasks.Push(new AddSpellTask(_classContainer, botTasks, talent));

                    //    foreach (var petSpell in _desiredActivityMemberState.PetSpells)
                    //        if (!_currentActivityMemberState.PetSpells.Contains(petSpell))
                    //            botTasks.Push(new AddSpellTask(_classContainer, botTasks, petSpell));

                    //    //if (desiredActivityMemberState.Level != currentActivityMemberState.Level)
                    //    //    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".character level {ObjectManager.Player.Name} {desiredActivityMemberState.Level}\")"));

                    //    if (_desiredActivityMemberState.BehaviorProfileName != _currentActivityMemberState.BehaviorProfileName)
                    //    {
                    //        AssignClassContainer();

                    //        botTasks.Push(new ResetActivityMemberStateTask(_classContainer, botTasks));
                    //    }

                    //    if (_desiredActivityMemberState.AccountName != _currentActivityMemberState.AccountName)
                    //    {
                    //        _currentActivityMemberState.AccountName = _desiredActivityMemberState.AccountName;
                    //        botTasks.Push(new LoginTask(_classContainer, botTasks));
                    //    }

                    //    if (botTasks.Count > 0)
                    //        botTasks.Peek()?.Update();
                    //});

                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER]{ex}");
                }
            }
        }

        private void AssignClassContainer()
        {
            try
            {
                currentBot = Bots.First(b => b.Name == _currentActivityMemberState.Member.BehaviorProfile);

                _classContainer = currentBot.GetClassContainer(_currentActivityMemberState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BOT RUNNER {Environment.ProcessId}]ReloadBots {ex}");
            }
        }
    }
}
