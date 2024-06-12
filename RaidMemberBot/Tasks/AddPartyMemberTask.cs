using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Models.Dto;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RaidMemberBot.AI.SharedStates
{
    public class AddPartyMemberTask : BotTask, IBotTask
    {
        private readonly string characterName;
        private bool hasInvited;
        public AddPartyMemberTask(IClassContainer container, Stack<IBotTask> botTasks, string characterName) : base(container, botTasks, TaskType.Ordinary)
        {
            this.characterName = characterName;
        }

        public void Update()
        {
            if (hasInvited && Wait.For("GroupInviteDelay", 500))
            {
                BotTasks.Pop();
                return;
            }
            else if(!hasInvited)
            {
                if (ObjectManager.PartyMembers.Count() > 4 && !Container.State.InRaid)
                {
                    Functions.LuaCall("ConvertToRaid()");
                }
                Functions.LuaCall($"InviteByName('{characterName}')");
                hasInvited = true;
            } 
        }
    }
}
