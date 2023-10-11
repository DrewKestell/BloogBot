using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ArcaneMageBot
{
    class RestTask : IBotTask
    {
        const string Evocation = "Evocation";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;
        
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;

            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (InCombat)
            {
                player.Stand();
                botTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                player.Stand();
                botTasks.Pop();
                botTasks.Push(new BuffTask(container, botTasks));
                return;
            }

            if (player.IsChanneling)
                return;

            if (player.ManaPercent < 20 && Spellbook.Instance.IsSpellReady(Evocation))
            {
                Lua.Instance.Execute($"CastSpellByName('{Evocation}')");
                return;
            }

            if (player.Level > 3 && foodItem != null && !player.IsEating && player.HealthPercent < 80)
                foodItem.Use();

            if (player.Level > 3 && drinkItem != null && !player.IsDrinking && player.ManaPercent < 80)
                drinkItem.Use();
        }

        bool HealthOk => player.HealthPercent > 90;

        bool ManaOk => (player.Level < 6 && player.ManaPercent > 60) || player.ManaPercent >= 90 || (player.ManaPercent >= 75 && !player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);
    }
}
