using WoWSlimClient.Models;

namespace WoWSlimClient.Tasks.SharedStates
{
    public class ExecuteBlockingLuaTask(IClassContainer container, Stack<IBotTask> botTasks, string luaScriptString) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        private readonly string luaScriptString = luaScriptString;
        private bool hasExecuted;

        public void Update()
        {
            if (!hasExecuted)
            {
                //Functions.LuaCall(luaScriptString);
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
