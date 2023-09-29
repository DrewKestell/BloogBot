using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FrostMageBot
{
    class RestTask : IBotTask
    {
        const string Evocation = "Evocation";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        readonly WoWItem foodItem;
        readonly WoWItem drinkItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;

            //foodItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Food);

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (player.IsChanneling)
                return;

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
                botTasks.Push(new BuffTask(container, botTasks, new List<WoWUnit>() { ObjectManager.Player }));
                return;
            }

            if (player.ManaPercent < 20 && player.IsSpellReady(Evocation))
            {
                player.LuaCall($"CastSpellByName('{Evocation}')");
                Thread.Sleep(200);
                return;
            }

            if (foodItem != null && !player.IsEating && player.HealthPercent < 80)
                foodItem.Use();

            if (drinkItem != null && !player.IsDrinking)
                drinkItem.Use();
        }

        bool HealthOk => foodItem == null || player.HealthPercent >= 90 || (player.HealthPercent >= 80 && !player.IsEating);

        bool ManaOk => drinkItem == null || player.ManaPercent >= 90 || (player.ManaPercent >= 80 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
