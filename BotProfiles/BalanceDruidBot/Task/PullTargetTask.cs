using RaidMemberBot.AI;
using RaidMemberBot.Client;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace BalanceDruidBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Wrath = "Wrath";
        const string Starfire = "Starfire";
        const string MoonkinForm = "Moonkin Form";

        readonly int range;
        readonly string pullingSpell;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (ObjectManager.Player.Level <= 19)
                range = 28;
            else if (ObjectManager.Player.Level == 20)
                range = 31;
            else
                range = 34;

            if (ObjectManager.Player.IsSpellReady(Starfire))
                pullingSpell = Starfire;
            else
                pullingSpell = Wrath;
        }

        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther || (ObjectManager.Aggressors.Count() > 0 && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.Player.TargetGuid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsCasting)
                return;

            if (ObjectManager.Player.IsSpellReady(MoonkinForm) && !ObjectManager.Player.HasBuff(MoonkinForm))
            {
                Functions.LuaCall($"CastSpellByName('{MoonkinForm}')");
            }

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < range && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(pullingSpell) && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For("BalanceDruidPullDelay", 100))
                {
                    if (!ObjectManager.Player.IsInCombat)
                        Functions.LuaCall($"CastSpellByName('{pullingSpell}')");

                    if (ObjectManager.Player.IsCasting || ObjectManager.Player.IsInCombat)
                    {
                        ObjectManager.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    }
                }
                return;
            }

            Position[] nextWaypoint = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
