// Friday owns this file!

using BeastMasterHunterBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BeastmasterHunterBot
{
    class PetManagerState : IBotState
    {
        const string CallPet = "Call Pet";
        const string RevivePet = "Revive Pet";
        const string FeedPet = "Feed Pet";


        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;
        readonly LocalPet pet;

        public PetManagerState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
            pet = ObjectManager.Pet;
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            if (!player.KnowsSpell(CallPet) || ObjectManager.Pet != null)
            {
                player.Stand();
                botStates.Pop();
                botStates.Push(new BuffSelfState(botStates, container));
                return;
            }

            player.LuaCall($"CastSpellByName('{CallPet}')");
        }

        public void Feed(string parFoodName)
        {
            if (true /*Inventory.Instance.GetItemCount(parFoodName) != 0*/)
            {
                const string checkFeedPet = "{0} = 0; if CursorHasSpell() then CanFeedMyPet = 1 end;";
                var result = player.LuaCallWithResults(checkFeedPet);
                if (result[0].Trim().Contains("0"))
                {
                    const string feedPet = "CastSpellByName('Feed Pet'); TargetUnit('Pet');";
                    player.LuaCall(feedPet);
                }
                const string usePetFood1 =
                    "for bag = 0,4 do for slot = 1,GetContainerNumSlots(bag) do local item = GetContainerItemLink(bag,slot) if item then if string.find(item, '";
                const string usePetFood2 = "') then PickupContainerItem(bag,slot) break end end end end";
                player.LuaCall(usePetFood1 + parFoodName.Replace("'", "\\'") + usePetFood2);
            }
            player.LuaCall("ClearCursor()");
        }
    }
}
