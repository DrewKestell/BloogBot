using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ProtectionPaladinBot
{
    class BuffTask : IBotTask
    {
        const string BlessingOfKings = "Blessing of Kings";
        const string BlessingOfMight = "Blessing of Might";
        const string BlessingOfSanctuary = "Blessing of Sanctuary";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if (!player.KnowsSpell(BlessingOfMight) || player.HasBuff(BlessingOfMight) || player.HasBuff(BlessingOfKings) || player.HasBuff(BlessingOfSanctuary))
            {
                botTasks.Pop();
                return;
            }
            
            if (player.KnowsSpell(BlessingOfMight) && !player.KnowsSpell(BlessingOfKings) && !player.KnowsSpell(BlessingOfSanctuary))
                TryCastSpell(BlessingOfMight);

            if (player.KnowsSpell(BlessingOfKings) && !player.KnowsSpell(BlessingOfSanctuary))
                TryCastSpell(BlessingOfKings);
            
            if (player.KnowsSpell(BlessingOfSanctuary))
                TryCastSpell(BlessingOfSanctuary);
        }

        void TryCastSpell(string name)
        {
            if (!player.HasBuff(name) && player.IsSpellReady(name) && player.Mana > player.GetManaCost(name))
            {
                if (ClientHelper.ClientVersion == ClientVersion.Vanilla)
                {
                    player.LuaCall($"CastSpellByName(\"{name}\",1)");
                }
                else
                {
                    player.CastSpell(name, player.Guid);
                }
            }
        }
    }
}
