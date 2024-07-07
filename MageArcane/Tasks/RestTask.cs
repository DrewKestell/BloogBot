using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace MageArcane.Tasks
{
    internal class RestTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Rest), IBotTask
    {
        private const string Evocation = "Evocation";
        private readonly WoWItem foodItem;
        private readonly WoWItem drinkItem;

        public void Update()
        {
            if (InCombat)
            {
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                return;
            }

            if (HealthOk && ManaOk)
            {
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Player.IsChanneling)
                return;

            if (ObjectManager.Player.ManaPercent < 20 && ObjectManager.Player.IsSpellReady(Evocation))
            {
                Functions.LuaCall($"CastSpellByName('{Evocation}')");
                return;
            }

            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }
            }

            if (ObjectManager.Player.Level > 3 && foodItem != null && !ObjectManager.Player.IsEating && ObjectManager.Player.HealthPercent < 80)
                foodItem.Use();

            if (ObjectManager.Player.Level > 3 && drinkItem != null && !ObjectManager.Player.IsDrinking && ObjectManager.Player.ManaPercent < 80)
                drinkItem.Use();
        }

        private bool HealthOk => ObjectManager.Player.HealthPercent > 90;

        private bool ManaOk => (ObjectManager.Player.Level < 6 && ObjectManager.Player.ManaPercent > 60) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 75 && !ObjectManager.Player.IsDrinking);

        private bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);
    }
}
