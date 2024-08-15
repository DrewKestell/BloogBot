using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace DruidFeral.Tasks
{
    internal class RestTask : BotTask, IBotTask
    {
        private const int stackCount = 5;

        private readonly IWoWItem drinkItem;
        public RestTask(IBotContext botContext) : base(botContext)
        {
            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.GetTarget(ObjectManager.Player).Guid == ObjectManager.Player.Guid)
            {
                if (ObjectManager.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    ObjectManager.SendChatMessage("SendChatMessage('.repairitems')");
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
                ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                BotTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                if (ObjectManager.Player.HasBuff(BearForm) && Wait.For("BearFormDelay", 1000, true))
                    ObjectManager.Player.CastSpell(BearForm);
                else if (ObjectManager.Player.HasBuff(CatForm) && Wait.For("CatFormDelay", 1000, true))
                    ObjectManager.Player.CastSpell(CatForm);
                else
                {
                    Wait.RemoveAll();
                    ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);
                    BotTasks.Pop();

                    uint drinkCount = drinkItem == null ? 0 : ObjectManager.GetItemCount(drinkItem.ItemId);

                    if (!InCombat && drinkCount == 0)
                    {
                        uint drinkToBuy = 28 - (drinkCount / stackCount);
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
                        BotTasks.Push(new BuffTask(BotContext));
                }
            }

            if (ObjectManager.Player.CurrentShapeshiftForm == BearForm)
                ObjectManager.Player.CastSpell(BearForm);

            if (ObjectManager.Player.CurrentShapeshiftForm == CatForm)
                ObjectManager.Player.CastSpell(CatForm);

            if (ObjectManager.Player.HealthPercent < 60 && ObjectManager.Player.CurrentShapeshiftForm == HumanForm && !ObjectManager.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
                ObjectManager.Player.CastSpell(Regrowth);

            if (ObjectManager.Player.HealthPercent < 80 && ObjectManager.Player.CurrentShapeshiftForm == HumanForm && !ObjectManager.Player.HasBuff(Rejuvenation) && !ObjectManager.Player.HasBuff(Regrowth) && Wait.For("SelfHealDelay", 5000, true))
                ObjectManager.Player.CastSpell(Rejuvenation);

            if (ObjectManager.Player.Level > 8 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60 && ObjectManager.Player.CurrentShapeshiftForm == HumanForm)
                drinkItem.Use();
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent >= 81;

        private bool ManaOk => (ObjectManager.Player.Level <= 8 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Aggressors.Any();
    }
}
