using BotRunner.Interfaces;
using BotRunner.Tasks;

namespace DruidFeral.Tasks
{
    internal class HealTask(IBotContext botContext) : BotTask(botContext), IBotTask
    {
        private const string BearForm = "Bear Form";
        private const string CatForm = "Cat Form";
        private const string WarStomp = "War Stomp";
        private const string HealingTouch = "Healing Touch";

        public void Update()
        {
            if (ObjectManager.Player.IsCasting) return;

            if (ObjectManager.Player.CurrentShapeshiftForm == BearForm && Wait.For("BearFormDelay", 1000, true))
                CastSpell(BearForm);

            if (ObjectManager.Player.CurrentShapeshiftForm == CatForm && Wait.For("CatFormDelay", 1000, true))
                CastSpell(CatForm);

            if (ObjectManager.Player.HealthPercent > 70 || ObjectManager.Player.Mana < ObjectManager.Player.GetManaCost(HealingTouch))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsSpellReady(WarStomp) && ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) <= 8)
                ObjectManager.Player.CastSpell(WarStomp);

            CastSpell(HealingTouch, castOnSelf: true);
        }

        private void CastSpell(string name, bool castOnSelf = false)
        {
            if (ObjectManager.Player.IsSpellReady(name))
            {
                ObjectManager.Player.CastSpell(name, castOnSelf: castOnSelf);
            }
        }
    }
}
