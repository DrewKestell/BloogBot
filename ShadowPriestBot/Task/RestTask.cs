using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string AbolishDisease = "Abolish Disease";
        const string CureDisease = "Cure Disease";
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string ShadowForm = "Shadowform";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWItem drinkItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                if (player.KnowsSpell(ShadowForm) && !player.HasBuff(ShadowForm) && player.IsDiseased)
                {
                    if (player.KnowsSpell(AbolishDisease))
                        player.LuaCall($"CastSpellByName('{AbolishDisease}',1)");
                    else if (player.KnowsSpell(CureDisease))
                        player.LuaCall($"CastSpellByName('{CureDisease}',2)");

                    return;
                }

                if (player.KnowsSpell(ShadowForm) && !player.HasBuff(ShadowForm))
                    player.LuaCall($"CastSpellByName('{ShadowForm}')");

                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();

                var drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                if (!InCombat && drinkCount == 0)
                {
                    var drinkToBuy = 28 - (drinkCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //{
                    //    { container.BotSettings.Drink, drinkToBuy }
                    //};

                    //var currentHotspot = container.GetCurrentHotspot();
                    //if (currentHotspot.TravelPath != null)
                    //{
                    //    botTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //botTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //botTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Position));
                    //container.CheckForTravelPath(botTasks, true, false);
                }
                else
                    botTasks.Push(new BuffTask(container, botTasks, new List<WoWUnit>() { ObjectManager.Player }));

                return;
            }

            if (!player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                player.Stand();

                if (player.HealthPercent < 70)
                {
                    if (player.HasBuff(ShadowForm))
                        player.LuaCall($"CastSpellByName('{ShadowForm}')");
                }
                
                if (player.HealthPercent < 50)
                {
                    if (player.KnowsSpell(Heal))
                        player.LuaCall($"CastSpellByName('{Heal}',1)");
                    else
                        player.LuaCall($"CastSpellByName('{LesserHeal}',1)");
                }

                if (player.HealthPercent < 70)
                    player.LuaCall($"CastSpellByName('{LesserHeal}',1)");
            }

            if (player.Level >= 5 && drinkItem != null && !player.IsDrinking && player.ManaPercent < 60)
                drinkItem.Use();
        }

        bool HealthOk => player.HealthPercent > 90;

        bool ManaOk => (player.Level < 5 && player.ManaPercent > 50) || player.ManaPercent >= 90 || (player.ManaPercent >= 65 && !player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
