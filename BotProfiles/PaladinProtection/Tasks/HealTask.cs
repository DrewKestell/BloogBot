namespace ProtectionPaladinBot
{
    class HealTask : BotTask, IBotTask
    {
        const string DivineProtection = "Divine Protection";
        const string HolyLight = "Holy Light";
        public HealTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Heal) { }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HolyLight))
            {
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(DivineProtection) && ObjectManager.Player.IsSpellReady(DivineProtection))
                Functions.LuaCall($"CastSpellByName('{DivineProtection}')");

            if (ObjectManager.Player.Mana > ObjectManager.Player.GetManaCost(HolyLight) && ObjectManager.Player.IsSpellReady(HolyLight))
            {
                Functions.LuaCall($"CastSpellByName(\"HolyLight\",1)");
            }
        }
    }
}
