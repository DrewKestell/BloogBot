using WoWActivityMember.Tasks.SharedStates;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using System.Collections.ObjectModel;
using WoWActivityMember.Models;
using WoWActivityMember.Client;
using ObjectManager = WoWActivityMember.Game.Statics.ObjectManager;
using Functions = WoWActivityMember.Mem.Functions;
using MaNGOSDBDomain.Client;

namespace WoWActivityMember.Tasks
{
    public class BotRunner
    {
        readonly Stack<IBotTask> botTasks = new();
        readonly BotLoader botLoader = new();
        readonly ActivityMemberState currentActivityMemberState;
        readonly ActivityMemberState desiredActivityMemberState;
        readonly ActivityCommandClient activityCommandClient;
        readonly MaNGOSDBClient maNGOSDBClient;

        IClassContainer classContainer;
        readonly ObservableCollection<IBot> Bots = [];

        readonly Task _asyncBotTaskRunnerTask;
        readonly Task _asyncServerFeedbackTask;

        // General
        IBot currentBot;

        public BotRunner()
        {
            Bots = new ObservableCollection<IBot>(botLoader.ReloadBots());

            currentActivityMemberState = new ActivityMemberState()
            {
                ProcessId = Environment.ProcessId
            };

            ObjectManager.Initialize(currentActivityMemberState);

            WoWEventHandler.Instance.OnPartyInvite += (sender, args) =>
            {
                ThreadSynchronizer.RunOnMainThread(() =>
                {
                    Functions.LuaCall($"StaticPopup1Button1:Click()");
                    Functions.LuaCall($"AcceptGroup()");
                });
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
                    ActivityMemberState incomingActivityMemberState = activityCommandClient.GetCommandBasedOnState(currentActivityMemberState);
                    desiredActivityMemberState.AccountName = incomingActivityMemberState.AccountName;
                    desiredActivityMemberState.BehaviorProfileName = incomingActivityMemberState.BehaviorProfileName;

                    desiredActivityMemberState.Spells.RemoveAll(x => !incomingActivityMemberState.Spells.Contains(x));
                    desiredActivityMemberState.Skills.RemoveAll(x => !incomingActivityMemberState.Skills.Contains(x));
                    desiredActivityMemberState.Talents.RemoveAll(x => !incomingActivityMemberState.Talents.Contains(x));
                    desiredActivityMemberState.PetSpells.RemoveAll(x => !incomingActivityMemberState.PetSpells.Contains(x));
                    desiredActivityMemberState.ActivityMembers.RemoveAll(x => !incomingActivityMemberState.ActivityMembers.Contains(x));

                    foreach (var item in incomingActivityMemberState.Spells)
                        if (!desiredActivityMemberState.Spells.Contains(item))
                            desiredActivityMemberState.Spells.Add(item);
                    foreach (var item in incomingActivityMemberState.Skills)
                        if (!desiredActivityMemberState.Skills.Contains(item))
                            desiredActivityMemberState.Skills.Add(item);
                    foreach (var item in incomingActivityMemberState.Talents)
                        if (!desiredActivityMemberState.Talents.Contains(item))
                            desiredActivityMemberState.Talents.Add(item);
                    foreach (var item in incomingActivityMemberState.PetSpells)
                        if (!desiredActivityMemberState.PetSpells.Contains(item))
                            desiredActivityMemberState.PetSpells.Add(item);
                    foreach (var item in incomingActivityMemberState.ActivityMembers)
                        if (!desiredActivityMemberState.ActivityMembers.Contains(item))
                            desiredActivityMemberState.ActivityMembers.Add(item);
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
                    ThreadSynchronizer.RunOnMainThread(() =>
                    {
                        if (Wait.For("AntiAFK", 5000, true))
                        {
                            ObjectManager.AntiAfk();
                        }
                        //case CharacterAction.BeginDungeon:
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Dungeon {ObjectManager.ZoneText}");
                        //    botTasks.Push(new DungeoneeringTask(classContainer, botTasks));
                        //    break;
                        //case CharacterAction.BeginBattleGrounds:
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Battleground {desiredActivityMemberState.CommandParam1}");
                        //    switch (desiredActivityMemberState.CommandParam1)
                        //    {
                        //        case "WSG":
                        //            botTasks.Push(new WarsongGultchTask(classContainer, botTasks));
                        //            break;
                        //    }
                        //    break;
                        //case CharacterAction.QueuePvP:
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin QueuePvP {desiredActivityMemberState.CommandParam1}");
                        //    botTasks.Push(new QueueForBattlegroundTask(classContainer, botTasks, ObjectManager.Units.First(x => x.Name.StartsWith(desiredActivityMemberState.CommandParam2))));
                        //    break;
                        //case CharacterAction.AddEquipment:
                        //    currentActivityMemberState.Action = "Adding equipment";
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddEquipment {desiredActivityMemberState.CommandParam1} {desiredActivityMemberState.CommandParam2}");
                        //    botTasks.Push(new AddEquipmentTask(classContainer, botTasks, int.Parse(desiredActivityMemberState.CommandParam1), int.Parse(desiredActivityMemberState.CommandParam2)));
                        //    break;

                        foreach (var activityMember in desiredActivityMemberState.ActivityMembers)
                            if (!currentActivityMemberState.ActivityMembers.Contains(activityMember))
                                botTasks.Push(new AddPartyMemberTask(classContainer, botTasks, activityMember));

                        foreach (var skill in desiredActivityMemberState.Skills)
                            if (!currentActivityMemberState.Skills.Contains(skill))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, skill));

                        foreach (var spell in desiredActivityMemberState.Spells)
                            if (!currentActivityMemberState.Spells.Contains(spell))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, spell));

                        foreach (var talent in desiredActivityMemberState.Talents)
                            if (!currentActivityMemberState.Talents.Contains(talent))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, talent));

                        foreach (var petSpell in desiredActivityMemberState.PetSpells)
                            if (!currentActivityMemberState.PetSpells.Contains(petSpell))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, petSpell));

                        //if (desiredActivityMemberState.Level != currentActivityMemberState.Level)
                        //    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".character level {ObjectManager.Player.Name} {desiredActivityMemberState.Level}\")"));

                        if (desiredActivityMemberState.BehaviorProfileName != currentActivityMemberState.BehaviorProfileName)
                        {
                            AssignClassContainer();

                            botTasks.Push(new ResetActivityMemberStateTask(classContainer, botTasks));
                        }

                        if (desiredActivityMemberState.AccountName != currentActivityMemberState.AccountName)
                        {
                            currentActivityMemberState.AccountName = desiredActivityMemberState.AccountName;
                            botTasks.Push(new LoginTask(classContainer, botTasks));
                        }

                        if (botTasks.Count > 0)
                            botTasks.Peek()?.Update();
                    });

                    await Task.Delay(50);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}]{ex.Message} {ex.StackTrace}");
                }
            }
        }

        private void AssignClassContainer()
        {
            try
            {
                currentBot = Bots.First(b => b.Name == currentActivityMemberState.BehaviorProfileName);

                classContainer = currentBot.GetClassContainer(currentActivityMemberState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BOT RUNNER {Environment.ProcessId}]ReloadBots {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
