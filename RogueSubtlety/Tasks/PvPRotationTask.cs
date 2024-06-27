using WoWActivityMember.Tasks;
using WoWActivityMember.Tasks.SharedStates;

namespace WarlockAffliction.Tasks
{
    class PvPRotationTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        const string Corruption = "Corruption";
        const string CurseOfAgony = "Curse of Agony";
        const string DeathCoil = "Death Coil";
        const string DrainSoul = "Drain Soul";
        const string Immolate = "Immolate";
        const string LifeTap = "Life Tap";
        const string ShadowBolt = "Shadow Bolt";
        const string SiphonLife = "Siphon Life";

        internal PvPRotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public override void PerformCombatRotation()
        {

        }

        public void Update()
        {
        }
    }
}
