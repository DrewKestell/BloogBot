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
    public class ExecuteBlockingLuaTask : BotTask, IBotTask
    {
        private readonly string luaScriptString;
        private bool hasExecuted;
        public ExecuteBlockingLuaTask(IClassContainer container, Stack<IBotTask> botTasks, string luaScriptString) : base(container, botTasks, TaskType.Ordinary)
        {
            this.luaScriptString = luaScriptString;
        }

        public void Update()
        {
            if (!hasExecuted)
            {
                Functions.LuaCall(luaScriptString);
                hasExecuted = true;
            }
            else if (Wait.For("LuaScriptDelay", 500))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }
        }
    }
}
