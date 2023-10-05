using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
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
            player = ObjectManager.Instance.Player;
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

            if (Spellbook.Instance.IsSpellReady(DemonArmor))
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
            //Lua.Instance.Execute($"PickupContainerItem({Inventory.Instance.GetBagId(ss.Guid)},{Inventory.GetSlotId(ss.Guid)})");
            //Lua.Instance.Execute("DeleteCursorItem()");
        }

        bool HasEnoughSoulShards => GetSoulShards.Count() <= 1;

        IEnumerable<WoWItem> GetSoulShards => Inventory.Instance.GetAllItems().Where(i => i.Info.Name == "Soul Shard");
    }
}
