using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class MoveToTargetTask : IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        const string CurseOfAgony = "Curse of Agony";
        const string ShadowBolt = "Shadow Bolt";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (Spellbook.Instance.IsSpellReady(CurseOfAgony))
                pullingSpell = CurseOfAgony;
            else
                pullingSpell = ShadowBolt;
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            }

            if (ObjectManager.Instance.Pet == null && (Spellbook.Instance.IsSpellReady(SummonImp) || Spellbook.Instance.IsSpellReady(SummonVoidwalker)))
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Push(new SummonVoidwalkerTask(container, botTasks));
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget < 27 && player.Casting == 0 && Spellbook.Instance.IsSpellReady(pullingSpell))
            {
                if (player.MovementState != MovementFlags.None)
                    player.StopMovement(ControlBits.Nothing);

                if (Wait.For("AfflictionWarlockPullDelay", 250))
                {
                    player.StopMovement(ControlBits.Nothing);
                    Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");
                    botTasks.Pop();
                    botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                }

                return;
            }

            var nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, false);
            player.Face(nextWaypoint[0]);
            player.StartMovement(ControlBits.Front);
        }
    }
}
