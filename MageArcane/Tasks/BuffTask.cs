using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace MageArcane.Tasks
{
    internal class BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Buff), IBotTask
    {
        private const string ArcaneIntellect = "Arcane Intellect";
        private const string FrostArmor = "Frost Armor";
        private const string IceArmor = "Ice Armor";
        private const string DampenMagic = "Dampen Magic";
        private readonly Stack<IBotTask> botTasks;
        private readonly IClassContainer container;
        private readonly LocalPlayer player;

        public void Update()
        {
            if ((!ObjectManager.Player.IsSpellReady(ArcaneIntellect) || ObjectManager.Player.HasBuff(ArcaneIntellect)) && (ObjectManager.Player.HasBuff(FrostArmor) || ObjectManager.Player.HasBuff(IceArmor)) && (!ObjectManager.Player.IsSpellReady(DampenMagic) || ObjectManager.Player.HasBuff(DampenMagic)))
            {
                BotTasks.Pop();
                BotTasks.Push(new ConjureItemsTask(botTasks, container));
                return;
            }
            
            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (ObjectManager.Player.IsSpellReady(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        private void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsSpellReady(name))
            {
                string castOnSelfString = castOnSelf ? ",1" : "";
                Functions.LuaCall($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
