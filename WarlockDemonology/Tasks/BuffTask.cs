using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Tasks;
using static BotRunner.Constants.Spellbook;

namespace WarlockDemonology.Tasks
{
    public class BuffTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        public void Update()
        {
            ObjectManager.Player.DoEmote(Emote.EMOTE_STATE_STAND);

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

        private void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.Level >= requiredLevel && ObjectManager.Player.IsSpellReady(name))
                ObjectManager.Player.CastSpell(name);
        }

        private void DeleteSoulShard()
        {
            var ss = GetSoulShards.Last();
            ObjectManager.PickupContainerItem(ObjectManager.GetBagId(ss.Guid), ObjectManager.GetSlotId(ss.Guid));
            ObjectManager.DeleteCursorItem();
        }

        private bool HasEnoughSoulShards => GetSoulShards.Count() <= 1;

        private IEnumerable<IWoWItem> GetSoulShards => ObjectManager.Items.Where(i => i.Info.Name == "Soul Shard");
    }
}
