// Friday owns this file!

using BeastMasterHunterBot;
using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace BeastmasterHunterBot
{
    class PetManagerState : IBotTask
    {
        const string CallPet = "Call Pet";
        const string RevivePet = "Revive Pet";
        const string FeedPet = "Feed Pet";


        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        readonly LocalPet pet;

        public PetManagerState(Stack<IBotTask> botTasks, IClassContainer container)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
            pet = ObjectManager.Instance.Pet;
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            if (!Spellbook.Instance.IsSpellReady(CallPet) || ObjectManager.Instance.Pet != null)
            {
                player.Stand();
                botTasks.Pop();
                botTasks.Push(new BuffTask(container, botTasks));
                return;
            }

            Lua.Instance.Execute($"CastSpellByName('{CallPet}')");
        }

        public void Feed(string parFoodName)
        {
            if (true /*Inventory.Instance.GetItemCount(parFoodName) != 0*/)
            {
                const string checkFeedPet = "{0} = 0; if CursorHasSpell() then CanFeedMyPet = 1 end;";
                var result = Lua.Instance.ExecuteWithResult(checkFeedPet);
                if (result[0].Trim().Contains("0"))
                {
                    const string feedPet = "CastSpellByName('Feed Pet'); TargetUnit('Pet');";
                    Lua.Instance.Execute(feedPet);
                }
                const string usePetFood1 =
                    "for bag = 0,4 do for slot = 1,GetContainerNumSlots(bag) do local item = GetContainerItemLink(bag,slot) if item then if string.find(item, '";
                const string usePetFood2 = "') then PickupContainerItem(bag,slot) break end end end end";
                Lua.Instance.Execute(usePetFood1 + parFoodName.Replace("'", "\\'") + usePetFood2);
            }
            Lua.Instance.Execute("ClearCursor()");
        }
    }
}
