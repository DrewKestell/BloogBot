using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Tasks;
using static WoWActivityMember.Constants.Enums;

namespace WarlockAffliction.Tasks
{
    class RestTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Rest), IBotTask
    {
        const string ConsumeShadows = "Consume Shadows";
        const string HealthFunnel = "Health Funnel";
        const string LifeTap = "Life Tap";

        public void Update()
        {
            
        }

        bool HealthOk => ObjectManager.Player.HealthPercent >= 90 || (ObjectManager.Player.HealthPercent >= 70 && !ObjectManager.Player.IsEating);

        bool PetHealthOk => ObjectManager.Pet == null || ObjectManager.Pet.HealthPercent >= 80;

        bool ManaOk => (ObjectManager.Player.Level < 6 && ObjectManager.Player.ManaPercent > 50) || ObjectManager.Player.ManaPercent >= 90 || (ObjectManager.Player.ManaPercent >= 55 && !ObjectManager.Player.IsDrinking);

        bool InCombat => ObjectManager.Player.IsInCombat || ObjectManager.Units.Any(u => u.TargetGuid == ObjectManager.Player.Guid || u.TargetGuid == ObjectManager.Pet?.Guid);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            if (ObjectManager.Player.IsSpellReady(name) && condition && !ObjectManager.Player.IsStunned && ((!ObjectManager.Player.IsCasting && ObjectManager.Player.ChannelingId == 0) || ObjectManager.Player.Class == Class.Warrior))
            {
                Functions.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }
    }
}
