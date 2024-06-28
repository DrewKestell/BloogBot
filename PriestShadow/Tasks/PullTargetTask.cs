using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;
using WoWActivityMember.Tasks;

namespace PriestShadow.Tasks
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string HolyFire = "Holy Fire";
        const string MindBlast = "Mind Blast";
        const string PowerWordShield = "Power Word: Shield";
        const string ShadowForm = "Shadowform";
        const string Smite = "Smite";
        const string WeakenedSoul = "WeakenedSoul";

        readonly string pullingSpell;

        Position currentWaypoint;
        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (ObjectManager.Player.HasBuff(ShadowForm))
                pullingSpell = MindBlast;
            else if (ObjectManager.Player.IsSpellReady(HolyFire))
                pullingSpell = HolyFire;
            else
                pullingSpell = Smite;
        }

        public void Update()
        {
            if (ObjectManager.Hostiles.Count() > 0)
            {
                WoWUnit potentialNewTarget = ObjectManager.Hostiles.First();

                if (potentialNewTarget != null && potentialNewTarget.Guid != ObjectManager.Player.TargetGuid)
                {
                    ObjectManager.Player.SetTarget(potentialNewTarget.Guid);
                }
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);
            if (distanceToTarget < 27)
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(pullingSpell))
                {
                    if (!ObjectManager.Player.IsSpellReady(PowerWordShield) || ObjectManager.Player.HasBuff(PowerWordShield) || ObjectManager.Player.IsInCombat)
                    {
                        if (Wait.For("ShadowPriestPullDelay", 250))
                        {
                            ObjectManager.Player.SetTarget(ObjectManager.Player.TargetGuid);
                            Wait.Remove("ShadowPriestPullDelay");

                            if (!ObjectManager.Player.IsInCombat)
                                Functions.LuaCall($"CastSpellByName('{pullingSpell}')");

                            ObjectManager.Player.StopAllMovement();
                            BotTasks.Pop();
                            BotTasks.Push(new PvERotationTask(Container, BotTasks));
                        }
                    }

                    if (ObjectManager.Player.IsSpellReady(PowerWordShield) && !ObjectManager.Player.HasDebuff(WeakenedSoul) && !ObjectManager.Player.HasBuff(PowerWordShield))
                        Functions.LuaCall($"CastSpellByName('{PowerWordShield}',1)");

                    return;
                }
            }
            else
            {
                Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                if (nextWaypoint.Length > 1)
                {
                    currentWaypoint = nextWaypoint[1];
                }
                else
                {
                    ObjectManager.Player.StopAllMovement();
                    BotTasks.Pop();
                    return;
                }

                ObjectManager.Player.MoveToward(currentWaypoint);
            }
        }
    }
}
