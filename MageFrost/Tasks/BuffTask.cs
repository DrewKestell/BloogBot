using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;

namespace MageFrost.Tasks
{
    class BuffTask : BotTask, IBotTask
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string DampenMagic = "Dampen Magic";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string MageArmor = "Mage Armor";

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
        public void Update()
        {
            if ((!ObjectManager.Player.IsSpellReady(ArcaneIntellect) || ObjectManager.Player.HasBuff(ArcaneIntellect)) && (ObjectManager.Player.HasBuff(FrostArmor) || ObjectManager.Player.HasBuff(IceArmor) || ObjectManager.Player.HasBuff(MageArmor)) && (!ObjectManager.Player.IsSpellReady(DampenMagic) || ObjectManager.Player.HasBuff(DampenMagic)))
            {
                BotTasks.Pop();
                BotTasks.Push(new ConjureItemsTask(BotTasks, Container));
                return;
            }

            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (ObjectManager.Player.IsSpellReady(MageArmor))
                TryCastSpell(MageArmor);
            else if (ObjectManager.Player.IsSpellReady(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsSpellReady(name))
            {
                if (castOnSelf)
                {
                    Functions.LuaCall($"CastSpellByName(\"{name}\",1)");
                }
                else
                    Functions.LuaCall($"CastSpellByName('{name}')");
            }
        }
    }
}
