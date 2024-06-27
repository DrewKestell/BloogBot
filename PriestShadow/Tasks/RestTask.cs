using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace ShadowPriestBot
{
    class RestTask : BotTask, IBotTask
    {
        const string AbolishDisease = "Abolish Disease";
        const string CureDisease = "Cure Disease";
        const string LesserHeal = "Lesser Heal";
        const string Heal = "Heal";
        const string ShadowForm = "Shadowform";
        public RestTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Rest) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (InCombat || (HealthOk && ManaOk))
            {
                if (ObjectManager.Player.IsSpellReady(ShadowForm) && !ObjectManager.Player.HasBuff(ShadowForm) && ObjectManager.Player.IsDiseased)
                {
                    if (ObjectManager.Player.IsSpellReady(AbolishDisease))
                        Functions.LuaCall($"CastSpellByName('{AbolishDisease}',1)");
                    else if (ObjectManager.Player.IsSpellReady(CureDisease))
                        Functions.LuaCall($"CastSpellByName('{CureDisease}',2)");

                    return;
                }

                if (ObjectManager.Player.IsSpellReady(ShadowForm) && !ObjectManager.Player.HasBuff(ShadowForm))
                    Functions.LuaCall($"CastSpellByName('{ShadowForm}')");

                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                    BotTasks.Push(new BuffTask(Container, BotTasks));

                return;
            }
            else
                ObjectManager.Player.StopAllMovement();

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.Target == null) return;

            if (ObjectManager.Player.Target.Guid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }

                List<WoWItem> drinkItems = ObjectManager.Items.Where(x => x.ItemId == 1179).ToList();
                int drinkItemsCount = drinkItems.Sum(x => x.StackCount);

                if (drinkItemsCount < 20)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 1179 {20 - drinkItemsCount}')");
                }

                WoWItem drinkItem = ObjectManager.Items.First(x => x.ItemId == 1179);

                if (ObjectManager.Player.Level >= 5 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 60)
                {
                    drinkItem.Use();
                }
            }

            if (!ObjectManager.Player.IsDrinking && Wait.For("HealSelfDelay", 3500, true))
            {
                ObjectManager.Player.Stand();

                if (ObjectManager.Player.HealthPercent < 70)
                {
                    if (ObjectManager.Player.HasBuff(ShadowForm))
                        Functions.LuaCall($"CastSpellByName('{ShadowForm}')");
                }

                if (ObjectManager.Player.HealthPercent < 50)
                {
                    if (ObjectManager.Player.IsSpellReady(Heal))
                        Functions.LuaCall($"CastSpellByName('{Heal}',1)");
                    else
                        Functions.LuaCall($"CastSpellByName('{LesserHeal}',1)");
                }

                if (ObjectManager.Player.HealthPercent < 70)
                    Functions.LuaCall($"CastSpellByName('{LesserHeal}',1)");
            }
        }

        bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        bool ManaOk => (ObjectManager.Player.Level < 5 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 65 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
