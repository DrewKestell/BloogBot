using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
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
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (Container.Player.IsChanneling)
                return;

            if (InCombat)
            {
                Container.Player.Stand();
                BotTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                Container.Player.Stand();
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(Container, BotTasks));
                return;
            }

            if (Container.Player.ManaPercent < 20 && Spellbook.Instance.IsSpellReady(Evocation))
            {
                Lua.Instance.Execute($"CastSpellByName('{Evocation}')");
                Thread.Sleep(200);
                return;
            }

            if (foodItem != null && !Container.Player.IsEating && Container.Player.HealthPercent < 80)
                foodItem.Use();

            if (drinkItem != null && !Container.Player.IsDrinking)
                drinkItem.Use();
        }

        bool HealthOk => foodItem == null || Container.Player.HealthPercent >= 90 || (Container.Player.HealthPercent >= 80 && !Container.Player.IsEating);

        bool ManaOk => drinkItem == null || Container.Player.ManaPercent >= 90 || (Container.Player.ManaPercent >= 80 && !Container.Player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);
    }
}
