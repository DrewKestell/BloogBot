using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace FrostMageBot
{
    class BuffTask : IBotTask
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string DampenMagic = "Dampen Magic";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string MageArmor = "Mage Armor";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> partyMembers)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if ((!player.KnowsSpell(ArcaneIntellect) || player.HasBuff(ArcaneIntellect)) && (player.HasBuff(FrostArmor) || player.HasBuff(IceArmor) || player.HasBuff(MageArmor)) && (!player.KnowsSpell(DampenMagic) || player.HasBuff(DampenMagic)))
            {
                botTasks.Pop();
                botTasks.Push(new ConjureItemsTask(botTasks, container));
                return;
            }

            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (player.KnowsSpell(MageArmor))
                TryCastSpell(MageArmor);
            else if (player.KnowsSpell(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!player.HasBuff(name) && player.KnowsSpell(name) && player.IsSpellReady(name))
            {
                if (castOnSelf)
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
                else
                    player.LuaCall($"CastSpellByName('{name}')");
            }
        }
    }
}
