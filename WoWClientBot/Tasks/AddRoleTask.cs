namespace WoWClientBot.AI.SharedStates
{
    public class AddRoleTask(IClassContainer container, Stack<IBotTask> botTasks, int roleId) : BotTask(container, botTasks, TaskType.Ordinary), IBotTask
    {
        readonly int roleId = roleId;

        public void Update()
        {
            switch (roleId)
            {
                case 1:
                    Container.State.CharacterConfig.IsRole1 = true;
                    break;
                case 2:
                    Container.State.CharacterConfig.IsRole2 = true;
                    break;
                case 3:
                    Container.State.CharacterConfig.IsRole3 = true;
                    break;
                case 4:
                    Container.State.CharacterConfig.IsRole4 = true;
                    break;
                case 5:
                    Container.State.CharacterConfig.IsRole5 = true;
                    break;
                case 6:
                    Container.State.CharacterConfig.IsRole6 = true;
                    break;
                case 7:
                    Container.State.CharacterConfig.IsMainTank = true;
                    break;
                case 8:
                    Container.State.CharacterConfig.IsMainHealer = true;
                    break;
                case 9:
                    Container.State.CharacterConfig.IsOffTank = true;
                    break;
                case 10:
                    Container.State.CharacterConfig.IsOffHealer = true;
                    break;
                case 11:
                    Container.State.CharacterConfig.ShouldCleanse = true;
                    break;
                case 12:
                    Container.State.CharacterConfig.ShouldRebuff = true;
                    break;
            }

            BotTasks.Pop();
        }
    }
}
