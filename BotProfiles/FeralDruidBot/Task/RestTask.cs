using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FeralDruidBot
{
    class RestTask : IBotTask
    {
        const int stackCount = 5;

        const string HumanForm = "Human Form";
        const string BearForm = "Bear Form";
        const string CatForm = "Cat Form";
        const string Regrowth = "Regrowth";
        const string Rejuvenation = "Rejuvenation";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly WoWItem drinkItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;

            //drinkItem = Inventory.GetAllItems()
            //    .FirstOrDefault(i => i.Info.Name == container.BotSettings.Drink);
        }

        public void Update()
        {
            if (player.IsCasting)
                return;
            
            if (InCombat)
            {
                Wait.RemoveAll();
                player.Stand();
                botTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                if (player.HasBuff(BearForm) && Wait.For("BearFormDelay", 1000, true))
                    CastSpell(BearForm);
                else if (player.HasBuff(CatForm) && Wait.For("CatFormDelay", 1000, true))
                    CastSpell(CatForm);
                else
                {
                    Wait.RemoveAll();
                    player.Stand();
                    botTasks.Pop();

                    var drinkCount = drinkItem == null ? 0 : Inventory.Instance.GetItemCount(drinkItem.Id);

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
                        //botTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                        //container.CheckForTravelPath(botTasks, true, false);
                    }
                    else
                        botTasks.Push(new BuffTask(container, botTasks));
                }
            }

            if (player.CurrentShapeshiftForm == BearForm)
                CastSpell(BearForm);

            if (player.CurrentShapeshiftForm == CatForm)
                CastSpell(CatForm);

            if (player.HealthPercent < 60 && player.CurrentShapeshiftForm == HumanForm && !player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
                TryCastSpell(Regrowth);

            if (player.HealthPercent < 80 && player.CurrentShapeshiftForm == HumanForm && !player.HasBuff(Rejuvenation) && !player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
                TryCastSpell(Rejuvenation);

            if (player.Level > 8 && drinkItem != null && !player.IsDrinking && player.ManaPercent < 60 && player.CurrentShapeshiftForm == HumanForm)
                drinkItem.Use();
        }

        bool HealthOk => player.HealthPercent >= 81;

        bool ManaOk => (player.Level <= 8 && player.ManaPercent > 50) || player.ManaPercent >= 90 || (player.ManaPercent >= 65 && !player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Aggressors.Count() > 0;

        void CastSpell(string name)
        {
            if (Spellbook.Instance.IsSpellReady(name) && !player.IsDrinking)
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }

        void TryCastSpell(string name)
        {
            if (Spellbook.Instance.IsSpellReady(name) && player.IsCasting && player.Mana > player.GetManaCost(name) && !player.IsDrinking)
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
