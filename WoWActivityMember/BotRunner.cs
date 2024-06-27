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
        readonly CharacterState currentCharacterState;
        readonly CharacterState desiredCharacterState;
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

            currentCharacterState = new CharacterState()
            {
                ProcessId = Environment.ProcessId
            };

            ObjectManager.Initialize(currentCharacterState);

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
                    CharacterState incomingCharacterState = activityCommandClient.GetCommandBasedOnState(currentCharacterState);
                    desiredCharacterState.AccountName = incomingCharacterState.AccountName;
                    desiredCharacterState.BehaviorProfileName = incomingCharacterState.BehaviorProfileName;

                    desiredCharacterState.Spells.RemoveAll(x => !incomingCharacterState.Spells.Contains(x));
                    desiredCharacterState.Skills.RemoveAll(x => !incomingCharacterState.Skills.Contains(x));
                    desiredCharacterState.Talents.RemoveAll(x => !incomingCharacterState.Talents.Contains(x));
                    desiredCharacterState.PetSpells.RemoveAll(x => !incomingCharacterState.PetSpells.Contains(x));
                    desiredCharacterState.ActivityMembers.RemoveAll(x => !incomingCharacterState.ActivityMembers.Contains(x));

                    foreach (var item in incomingCharacterState.Spells)
                        if (!desiredCharacterState.Spells.Contains(item))
                            desiredCharacterState.Spells.Add(item);
                    foreach (var item in incomingCharacterState.Skills)
                        if (!desiredCharacterState.Skills.Contains(item))
                            desiredCharacterState.Skills.Add(item);
                    foreach (var item in incomingCharacterState.Talents)
                        if (!desiredCharacterState.Talents.Contains(item))
                            desiredCharacterState.Talents.Add(item);
                    foreach (var item in incomingCharacterState.PetSpells)
                        if (!desiredCharacterState.PetSpells.Contains(item))
                            desiredCharacterState.PetSpells.Add(item);
                    foreach (var item in incomingCharacterState.ActivityMembers)
                        if (!desiredCharacterState.ActivityMembers.Contains(item))
                            desiredCharacterState.ActivityMembers.Add(item);
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
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin Battleground {desiredCharacterState.CommandParam1}");
                        //    switch (desiredCharacterState.CommandParam1)
                        //    {
                        //        case "WSG":
                        //            botTasks.Push(new WarsongGultchTask(classContainer, botTasks));
                        //            break;
                        //    }
                        //    break;
                        //case CharacterAction.QueuePvP:
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] Begin QueuePvP {desiredCharacterState.CommandParam1}");
                        //    botTasks.Push(new QueueForBattlegroundTask(classContainer, botTasks, ObjectManager.Units.First(x => x.Name.StartsWith(desiredCharacterState.CommandParam2))));
                        //    break;
                        //case CharacterAction.AddEquipment:
                        //    currentCharacterState.Action = "Adding equipment";
                        //    Console.WriteLine($"[BOT RUNNER : {Environment.ProcessId}] AddEquipment {desiredCharacterState.CommandParam1} {desiredCharacterState.CommandParam2}");
                        //    botTasks.Push(new AddEquipmentTask(classContainer, botTasks, int.Parse(desiredCharacterState.CommandParam1), int.Parse(desiredCharacterState.CommandParam2)));
                        //    break;

                        foreach (var activityMember in desiredCharacterState.ActivityMembers)
                            if (!currentCharacterState.ActivityMembers.Contains(activityMember))
                                botTasks.Push(new AddPartyMemberTask(classContainer, botTasks, activityMember));

                        foreach (var skill in desiredCharacterState.Skills)
                            if (!currentCharacterState.Skills.Contains(skill))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, skill));

                        foreach (var spell in desiredCharacterState.Spells)
                            if (!currentCharacterState.Spells.Contains(spell))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, spell));

                        foreach (var talent in desiredCharacterState.Talents)
                            if (!currentCharacterState.Talents.Contains(talent))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, talent));

                        foreach (var petSpell in desiredCharacterState.PetSpells)
                            if (!currentCharacterState.PetSpells.Contains(petSpell))
                                botTasks.Push(new AddSpellTask(classContainer, botTasks, petSpell));

                        //if (desiredCharacterState.Level != currentCharacterState.Level)
                        //    botTasks.Push(new ExecuteBlockingLuaTask(classContainer, botTasks, $"SendChatMessage(\".character level {ObjectManager.Player.Name} {desiredCharacterState.Level}\")"));

                        if (desiredCharacterState.BehaviorProfileName != currentCharacterState.BehaviorProfileName)
                        {
                            AssignClassContainer();

                            botTasks.Push(new ResetCharacterStateTask(classContainer, botTasks));
                        }

                        if (desiredCharacterState.AccountName != currentCharacterState.AccountName)
                        {
                            currentCharacterState.AccountName = desiredCharacterState.AccountName;
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
                currentBot = Bots.First(b => b.Name == currentCharacterState.BehaviorProfileName);

                classContainer = currentBot.GetClassContainer(currentCharacterState);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BOT RUNNER {Environment.ProcessId}]ReloadBots {ex.Message} {ex.StackTrace}");
            }
        }
    }
}
