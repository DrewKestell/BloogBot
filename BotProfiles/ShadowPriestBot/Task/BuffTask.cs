using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

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
            if ((!Spellbook.Instance.IsSpellReady(PowerWordFortitude) || ObjectManager.Instance.PartyMembers.All(x => x.GotAura(PowerWordFortitude))) && (!Spellbook.Instance.IsSpellReady(ShadowProtection) || ObjectManager.Instance.PartyMembers.All(x => x.GotAura(ShadowProtection))))
            {
                botTasks.Pop();
                return;
            }

            WoWUnit woWUnit = ObjectManager.Instance.PartyMembers.First(x => !x.GotAura(PowerWordFortitude));

            player.SetTarget(woWUnit);

            TryCastSpell(PowerWordFortitude);

            TryCastSpell(ShadowProtection);
        }

        void TryCastSpell(string name, int requiredLevel = 1)
        {
            if (!player.GotAura(name) && player.Level >= requiredLevel && Spellbook.Instance.IsSpellReady(name))
                Lua.Instance.Execute($"CastSpellByName('{name}')");
        }
    }
}
