using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ArcaneMageBot
{
    class RestTask : BotTask, IBotTask
    {
        const string Evocation = "Evocation";

        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
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

            if (Container.Player.IsChanneling)
                return;

            if (Container.Player.ManaPercent < 20 && Spellbook.Instance.IsSpellReady(Evocation))
            {
                Lua.Instance.Execute($"CastSpellByName('{Evocation}')");
                return;
            }

            if (Container.Player.Level > 3 && foodItem != null && !Container.Player.IsEating && Container.Player.HealthPercent < 80)
                foodItem.Use();

            if (Container.Player.Level > 3 && drinkItem != null && !Container.Player.IsDrinking && Container.Player.ManaPercent < 80)
                drinkItem.Use();
        }

        bool HealthOk => Container.Player.HealthPercent > 90;

        bool ManaOk => (Container.Player.Level < 6 && Container.Player.ManaPercent > 60) || Container.Player.ManaPercent >= 90 || (Container.Player.ManaPercent >= 75 && !Container.Player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);
    }
}
