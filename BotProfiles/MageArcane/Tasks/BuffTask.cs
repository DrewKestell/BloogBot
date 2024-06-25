namespace ArcaneMageBot
{
    class BuffTask : BotTask, IBotTask
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string DampenMagic = "Dampen Magic";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;

        public BuffTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Buff) { }
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

        void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!ObjectManager.Player.HasBuff(name) && ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.IsSpellReady(name))
            {
                string castOnSelfString = castOnSelf ? ",1" : "";
                Functions.LuaCall($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
