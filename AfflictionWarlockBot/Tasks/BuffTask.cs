using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AfflictionWarlockBot
{
    class BuffTask : IBotTask
    {
        const string DemonArmor = "Demon Armor";
        const string DemonSkin = "Demon Skin";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;
            player.SetTarget(player.Guid);
        }

        public void Update()
        {
            if (player.HasBuff(DemonSkin) || player.HasBuff(DemonArmor))
            {
                if (HasEnoughSoulShards)
                {
                    botTasks.Pop();
                    return;
                }
                else
                    DeleteSoulShard();
            }

            if (player.KnowsSpell(DemonArmor))
                TryCastSpell(DemonArmor);
            else
                TryCastSpell(DemonSkin);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            
        }

        void DeleteSoulShard()
        {
            var ss = GetSoulShards.Last();
            player.LuaCall($"PickupContainerItem({Inventory.GetBagId(ss.Guid)},{Inventory.GetSlotId(ss.Guid)})");
            player.LuaCall("DeleteCursorItem()");
        }

        bool HasEnoughSoulShards => GetSoulShards.Count() <= 1;

        IEnumerable<WoWItem> GetSoulShards => Inventory.GetAllItems().Where(i => i.Info.Name == "Soul Shard");
    }
}
