using RaidMemberBot.AI;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FeralDruidBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string HumanForm = "Human Form";
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";
        const string Regrowth = "Regrowth";
        const string Rejuvenation = "Rejuvenation";

        readonly WoWItem drinkItem;
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            Functions.LuaCall($"SendChatMessage('.repairitems')");
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
                if (ObjectManager.Player.HasBuff(BearForm) && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);
                else if (ObjectManager.Player.HasBuff(CatForm) && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);
                else
                {
                    Wait.RemoveAll();
                    ObjectManager.Player.Stand();
                    BotTasks.Pop();

                    int drinkCount = drinkItem == null ? 0 : Inventory.GetItemCount(drinkItem.ItemId);

                    if (!InCombat && drinkCount == 0)
                    {
                        int drinkToBuy = 28 - (drinkCount / stackCount);
                        //var itemsToBuy = new Dictionary<string, int>
                        //{
                        //    { container.BotSettings.Drink, drinkToBuy }
                        //};

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
            }

            if (ObjectManager.Player.CurrentShapeshiftForm == BearForm)
                CastSpell(BearForm);

            if (ObjectManager.Player.CurrentShapeshiftForm == CatForm)
                CastSpell(CatForm);

            if (ObjectManager.Player.HealthPercent < 60 && ObjectManager.Player.CurrentShapeshiftForm == HumanForm && !ObjectManager.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
                TryCastSpell(Regrowth);

            if (ObjectManager.Player.HealthPercent < 80 && ObjectManager.Player.CurrentShapeshiftForm == HumanForm && !ObjectManager.Player.HasBuff(Rejuvenation) && !ObjectManager.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
                TryCastSpell(Rejuvenation);

            if (ObjectManager.Player.Level > 8 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && ObjectManager.Player.CurrentShapeshiftForm == HumanForm)
                drinkItem.Use();
        }

        bool HealthOk => ObjectManager.Player.HealthPercent >= 81;

        bool ManaOk => (ObjectManager.Player.Level <= 8 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Aggressors.Count() > 0;

        void CastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && !ObjectManager.Player.IsDrinking)
                Functions.LuaCall($"CastSpellByName('{name}')");
        }

        void TryCastSpell(string name)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsCasting && ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(name) && !ObjectManager.Player.IsDrinking)
                Functions.LuaCall($"CastSpellByName('{name}',1)");
        }
    }
}
