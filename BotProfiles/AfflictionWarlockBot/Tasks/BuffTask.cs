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

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
            player.SetTarget(player.Guid);
        }

        public void Update()
        {
            if ((!Spellbook.Instance.IsSpellReady(DemonSkin) || player.HasBuff(DemonSkin)) && (!Spellbook.Instance.IsSpellReady(DemonArmor) || player.HasBuff(DemonArmor)))
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
            if (!player.GotAura(name) && player.Level >= requiredLevel && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }

        void DeleteSoulShard()
        {
            var ss = GetSoulShards.Last();
            Lua.Instance.Execute($"PickupContainerItem({Inventory.Instance.GetBagId(ss.Guid)},{Inventory.Instance.GetSlotId(ss.Guid)})");
            Lua.Instance.Execute("DeleteCursorItem()");
        }

        bool HasEnoughSoulShards => GetSoulShards.Count() <= 1;

        IEnumerable<WoWItem> GetSoulShards => Inventory.Instance.GetAllItems().Where(i => i.Info.Name == "Soul Shard");
    }
}
