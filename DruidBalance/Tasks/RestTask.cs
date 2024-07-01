using WoWActivityMember.Tasks;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;

namespace DruidBalance.Tasks
{
    internal class RestTask : BotTask, IBotTask
    {
        private const int stackCount = 5;
        private const string Regrowth = "Regrowth";
        private const string Rejuvenation = "Rejuvenation";
        private const string MoonkinForm = "Moonkin Form";
        private WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }
            }
        }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;
            
            if (InCombat)
            {
                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                return;
            }
            if (HealthOk && ManaOk)
            {
                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();

                int drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                if (!InCombat && drinkCount == 0)
                {
                    int drinkToBuy = 28 - (drinkCount / stackCount);
                    //var itemsToBuy = new Dictionary<string, int>
                    //    {
                    //        { container.BotSettings.Drink, drinkToBuy }
                    //    };

                    //var currentHotspot = container.GetCurrentHotspot();

                    //if (currentHotspot.TravelPath != null)
                    //{
                    //    BotTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //BotTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //BotTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Position));
                    //container.CheckForTravelPath(botTasks, true, false);
                }
                else
                    BotTasks.Push(new BuffTask(Container, BotTasks));
            }

            if (ObjectManager.Player.HealthPercent < 60 && !ObjectManager.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
            {
                TryCastSpell(MoonkinForm, ObjectManager.Player.HasBuff(MoonkinForm));
                TryCastSpell(Regrowth);
            }
                
            if (ObjectManager.Player.HealthPercent < 80 && !ObjectManager.Player.HasBuff(Rejuvenation) && !ObjectManager.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
            {
                TryCastSpell(MoonkinForm, ObjectManager.Player.HasBuff(MoonkinForm));
                TryCastSpell(Rejuvenation);
            }

            if (ObjectManager.Player.Level >= 6 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60)
                drinkItem.Use();
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent >= 81;

        private bool ManaOk => (ObjectManager.Player.Level < 6 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Aggressors.Count() > 0;

        private void TryCastSpell(string name, bool condition = true)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsCasting && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(name) && !ObjectManager.Player.IsDrinking && condition)
            {
                ObjectManager.Player.Stand();
                Functions.LuaCall($"CastSpellByName('{name}',1)");
            }
        }
    }
}
