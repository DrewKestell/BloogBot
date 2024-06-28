using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace WarlockDestruction.Tasks
{
    class BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        const string DemonArmor = "Demon Armor";
        const string DemonSkin = "Demon Skin";

        public void Update()
        {
            ObjectManager.Player.Stand();

            if ((!ObjectManager.Player.IsSpellReady(DemonSkin) || ObjectManager.Player.HasBuff(DemonSkin)) && (!ObjectManager.Player.IsSpellReady(DemonArmor) || ObjectManager.Player.HasBuff(DemonArmor)))
            {
                if (HasEnoughSoulShards)
                {
                    BotTasks.Pop();
                    return;
                }
                else
                    DeleteSoulShard();
            }

            if (ObjectManager.Player.IsSpellReady(DemonArmor))
                TryCastSpell(DemonArmor);
            else
                TryCastSpell(DemonSkin);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.Level >= requiredLevel && ObjectManager.Player.IsSpellReady(name))
                Functions.LuaCall($"CastSpellByName('{name}')");
        }

        void DeleteSoulShard()
        {
            var ss = GetSoulShards.Last();
            Functions.LuaCall($"PickupContainerItem({Inventory.GetBagId(ss.Guid)},{Inventory.GetSlotId(ss.Guid)})");
            Functions.LuaCall("DeleteCursorItem()");
        }

        bool HasEnoughSoulShards => GetSoulShards.Count() <= 1;

        IEnumerable<WoWItem> GetSoulShards => Inventory.GetAllItems().Where(i => i.Info.Name == "Soul Shard");
    }
}
