using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace CombatRogueBot
{
    class RestTask : BotTask, IBotTask
    {
        const string Cannibalize = "Cannibalize";
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
            bool readyToPop = ObjectManager.Player.HealthPercent >= 95
                || ObjectManager.Player.HealthPercent >= 80 && !ObjectManager.Player.IsEating
                || ObjectManager.Player.IsInCombat
                || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid);

            if (readyToPop)
            {
                Wait.RemoveAll();
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsChanneling)
                return;

            if (ObjectManager.Player.IsSpellReady(Cannibalize) && ObjectManager.Player.TastyCorpsesNearby)
            {
                Functions.LuaCall($"CastSpellByName('{Cannibalize}')");
                return;
            }


            ObjectManager.Player.SetTarget(ObjectManager.Player.Guid);

            if (ObjectManager.Player.TargetGuid == ObjectManager.Player.Guid)
            {
                if (Inventory.GetEquippedItems().Any(x => x.DurabilityPercentage > 0 && x.DurabilityPercentage < 100))
                {
                    Functions.LuaCall($"SendChatMessage('.repairitems')");
                }

                List<WoWItem> foodItems = ObjectManager.Items.Where(x => x.ItemId == 5479).ToList();
                int foodItemsCount = foodItems.Sum(x => x.StackCount);

                if (foodItemsCount < 20)
                {
                    Functions.LuaCall($"SendChatMessage('.additem 5479 {20 - foodItemsCount}')");
                }
            }

            WoWItem foodItem = ObjectManager.Items.First(x => x.ItemId == 5479);

            if (foodItem != null && !ObjectManager.Player.IsEating && Wait.For("EatDelay", 500, true))
                foodItem.Use();
        }
    }
}
