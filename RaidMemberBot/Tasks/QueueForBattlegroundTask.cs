using RaidMemberBot.Game;
using RaidMemberBot.Game.Frames;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Models;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class QueueForBattlegroundTask : BotTask, IBotTask
    {
        WoWUnit npc;
        DialogFrame dialogFrame;
        bool hasInteracted;
        bool hasDialogFrame;
        bool hasSelectedGossipOption;
        bool hasQueued;
        bool readyToPop;

        public QueueForBattlegroundTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit woWUnit) : base(container, botTasks, TaskType.Ordinary)
        {
            npc = woWUnit;
        }

        public void Update()
        {
            if (npc.Position.DistanceTo(ObjectManager.Player.Position) > 5)
            {
                Container.State.Action = "Moving towards Battlemaster";
                ObjectManager.Player.MoveToward(npc.Position);
            }
            else if (!hasInteracted)
            {
                Container.State.Action = "Talking to Battlemaster";
                npc.Interact();
                hasInteracted = true;
            }
            else if (!hasDialogFrame && Wait.For("PrepDialogFrameDelay", 1000))
            {
                dialogFrame = new DialogFrame();
                hasDialogFrame = true;
            }
            else if (!hasSelectedGossipOption && Wait.For("PrepBattleMasterDialogFrameDelay", 1000))
            {
                Container.State.Action = "Selecting dialog option";
                dialogFrame.SelectFirstGossipOfType(Constants.Enums.DialogType.battlemaster);
                hasSelectedGossipOption = true;
            }
            else if (!hasQueued && ObjectManager.PartyMembers.All(x => x.Position.DistanceTo(ObjectManager.Player.Position) < 5) && Wait.For("DialogFrameDelay", 1000))
            {
                Container.State.Action = "Joining queue";
                string enabled = Functions.LuaCallWithResult("{0} = BattlefieldFrameGroupJoinButton:IsEnabled()")[0];
                Console.WriteLine($"[BG QUEUE TASK] group join enabled {enabled}");
                if (enabled == "1")
                    Functions.LuaCall("BattlefieldFrameGroupJoinButton:Click()");
                else
                    Functions.LuaCall("BattlefieldFrameJoinButton:Click()");
                hasQueued = true;
            }
            else if (hasQueued)
            {
                Container.State.Action = "Waiting in BG queue";
                Wait.RemoveAll();
                Container.State.InBGQueue = true;
                BotTasks.Pop();
            }
        }
    }
}
