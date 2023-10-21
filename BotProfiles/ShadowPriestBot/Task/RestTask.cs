using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ShadowPriestBot
{
    class RestTask : BotTask, IBotTask
    {
        const int stackCount = 5;

        const string AbolishDisease = "Abolish Disease";
        const string CureDisease = "Cure Disease";
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string ShadowForm = "Shadowform";
        readonly WoWItem drinkItem;

        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (Container.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                if (Spellbook.Instance.IsSpellReady(ShadowForm) && !Container.Player.GotAura(ShadowForm) && Container.Player.IsDiseased)
                {
                    if (Spellbook.Instance.IsSpellReady(AbolishDisease))
                        Lua.Instance.Execute($"CastSpellByName('{AbolishDisease}',1)");
                    else if (Spellbook.Instance.IsSpellReady(CureDisease))
                        Lua.Instance.Execute($"CastSpellByName('{CureDisease}',2)");

                    return;
                }

                if (Spellbook.Instance.IsSpellReady(ShadowForm) && !Container.Player.GotAura(ShadowForm))
                    Lua.Instance.Execute($"CastSpellByName('{ShadowForm}')");

                Wait.RemoveAll();
                Container.Player.Stand();
                BotTasks.Pop();

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
                    //    BotTasks.Push(new TravelState(botTasks, container, currentHotspot.TravelPath.Waypoints, 0));
                    //    BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.TravelPath.Waypoints[0]));
                    //}

                    //BotTasks.Push(new BuyItemsState(botTasks, currentHotspot.Innkeeper.Name, itemsToBuy));
                    //BotTasks.Push(new SellItemsState(botTasks, container, currentHotspot.Innkeeper.Name));
                    //BotTasks.Push(new MoveToPositionState(botTasks, container, currentHotspot.Innkeeper.Location));
                    //container.CheckForTravelPath(botTasks, true, false);
                }
                else
                    BotTasks.Push(new BuffTask(Container, BotTasks));

                return;
            }
            else
            {
                Container.Player.StopAllMovement();
            }

            if (!Container.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                Container.Player.Stand();

                if (Container.Player.HealthPercent < 70)
                {
                    if (Container.Player.GotAura(ShadowForm))
                        Lua.Instance.Execute($"CastSpellByName('{ShadowForm}')");
                }

                if (Container.Player.HealthPercent < 50)
                {
                    if (Spellbook.Instance.IsSpellReady(Heal))
                        Lua.Instance.Execute($"CastSpellByName('{Heal}',1)");
                    else
                        Lua.Instance.Execute($"CastSpellByName('{LesserHeal}',1)");
                }

                if (Container.Player.HealthPercent < 70)
                    Lua.Instance.Execute($"CastSpellByName('{LesserHeal}',1)");
            }

            if (Container.Player.Level >= 5 && drinkItem != null && !Container.Player.IsDrinking && Container.Player.ManaPercent < 60)
                drinkItem.Use();
        }

        bool HealthOk => Container.Player.HealthPercent > 90;

        bool ManaOk => (Container.Player.Level < 5 && Container.Player.ManaPercent > 50) || Container.Player.ManaPercent >= 90 || (Container.Player.ManaPercent >= 65 && !Container.Player.IsDrinking);

        bool InCombat => ObjectManager.Instance.Player.IsInCombat || ObjectManager.Instance.Units.Any(u => u.TargetGuid == ObjectManager.Instance.Player.Guid);
    }
}
