

using WoWActivityMember.Tasks;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using HunterBeastMastery.Tasks;

namespace HunterBeastMastery
{
    class PetManagerState : BotTask, IBotTask
    {
        const string CallPet = "Call Pet";
        const string RevivePet = "Revive Pet";
        const string FeedPet = "Feed Pet";

        public PetManagerState(Stack<IBotTask> botTasks, IClassContainer container) : base(container, botTasks, TaskType.Buff) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            if (!ObjectManager.Player.IsSpellReady(CallPet) || ObjectManager.Pet != null)
            {
                ObjectManager.Player.Stand();
                BotTasks.Pop();
                BotTasks.Push(new BuffTask(Container, BotTasks));
                return;
            }

            Functions.LuaCall($"CastSpellByName('{CallPet}')");
        }

        public void Feed(string parFoodName)
        {
            if (true /*Inventory.GetItemCount(parFoodName) != 0*/)
            {
                const string checkFeedPet = "{0} = 0; if CursorHasSpell() then CanFeedMyPet = 1 end;";
                string[] result = Functions.LuaCallWithResult(checkFeedPet);
                if (result[0].Trim().Contains("0"))
                {
                    const string feedPet = "CastSpellByName('Feed Pet'); TargetUnit('Pet');";
                    Functions.LuaCall(feedPet);
                }
                const string usePetFood1 =
                    "for bag = 0,4 do for slot = 1,GetContainerNumSlots(bag) do local item = GetContainerItemLink(bag,slot) if item then if string.find(item, '";
                const string usePetFood2 = "') then PickupContainerItem(bag,slot) break end end end end";
                Functions.LuaCall(usePetFood1 + parFoodName.Replace("'", "\\'") + usePetFood2);
            }
            Functions.LuaCall("ClearCursor()");
        }
    }
}
