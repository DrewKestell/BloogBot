using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FrostMageBot
{
    class RestTask : BotTask, IBotTask
    {
        const string Evocation = "Evocation";

        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            Functions.LuaCall($"SendChatMessage('.repairitems')");
        }

        public void Update()
        {
            if (ObjectManager.Player.IsChanneling)
                return;

            if (InCombat)
            {
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Player.ManaPercent < 20 && ObjectManager.Player.IsSpellReady(Evocation))
            {
                Functions.LuaCall($"CastSpellByName('{Evocation}')");
                Thread.Sleep(200);
                return;
            }

            if (foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80)
                foodItem.Use();

            if (drinkItem != null && !ObjectManager.Player.IsDrinking)
                drinkItem.Use();
        }

        bool HealthOk => foodItem == null || ObjectManager.Player.HealthPercent >= 90 || (ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating);

        bool ManaOk => drinkItem == null || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 80 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
