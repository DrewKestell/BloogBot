using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;

namespace ShadowPriestBot
{
    class BuffTask : IBotTask
    {
        const string PowerWordFortitude = "Power Word: Fortitude";
        const string ShadowProtection = "Shadow Protection";

        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.botTasks = botTasks;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if ((!Spellbook.Instance.IsSpellReady(PowerWordFortitude) || player.GotAura(PowerWordFortitude)) && (!Spellbook.Instance.IsSpellReady(ShadowProtection) || player.GotAura(ShadowProtection)))
            {
                botTasks.Pop();
                return;
            }

            TryCastSpell(PowerWordFortitude);

            TryCastSpell(ShadowProtection);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!player.GotAura(name) && player.Level >= requiredLevel && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}',1)");
        }
    }
}
