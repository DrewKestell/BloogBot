using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace AfflictionWarlockBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string DemonArmor = "Demon Armor";
        const string DemonSkin = "Demon Skin";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((!Spellbook.Instance.IsSpellReady(DemonSkin) || Container.Player.HasBuff(DemonSkin)) && (!Spellbook.Instance.IsSpellReady(DemonArmor) || Container.Player.HasBuff(DemonArmor)))
            {
                if (HasEnoughSoulShards)
                {
                    BotTasks.Pop();
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
            if (!Container.Player.GotAura(name) && Container.Player.Level >= requiredLevel && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }

        void DeleteSoulShard()
        {
            WoWItem ss = GetSoulShards.Last();
            Lua.Instance.Execute($"PickupContainerItem({Inventory.Instance.GetBagId(ss.Guid)},{Inventory.Instance.GetSlotId(ss.Guid)})");
            Lua.Instance.Execute("DeleteCursorItem()");
        }

        bool HasEnoughSoulShards => GetSoulShards.Count() <= 1;

        IEnumerable<WoWItem> GetSoulShards => Inventory.Instance.GetAllItems().Where(i => i.Info.Name == "Soul Shard");
    }
}
