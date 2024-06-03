using System.Collections.Generic;

namespace RaidMemberBot.AI.SharedStates
{
    public class AddRoleTask : BotTask, IBotTask
    {
        int roleId;
        public AddRoleTask(IClassContainer container, Stack<IBotTask> botTasks, int roleId) : base(container, botTasks, TaskType.Ordinary)
        {
            this.roleId = roleId;
        }

        public void Update()
        {
            switch (roleId)
            {
                case 1:
                    Container.State.IsRole1 = true;
                    break;
                case 2:
                    Container.State.IsRole2 = true;
                    break;
                case 3:
                    Container.State.IsRole3 = true;
                    break;
                case 4:
                    Container.State.IsRole4 = true;
                    break;
                case 5:
                    Container.State.IsRole5 = true;
                    break;
                case 6:
                    Container.State.IsRole6 = true;
                    break;
                case 7:
                    Container.State.IsMainTank = true;
                    break;
                case 8:
                    Container.State.IsMainHealer = true;
                    break;
                case 9:
                    Container.State.IsOffTank = true;
                    break;
                case 10:
                    Container.State.IsOffHealer = true;
                    break;
                case 11:
                    Container.State.ShouldCleanse = true;
                    break;
                case 12:
                    Container.State.ShouldRebuff = true;
                    break;
            }

            BotTasks.Pop();
        }
    }
}
