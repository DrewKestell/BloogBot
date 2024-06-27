using WoWActivityMember.Tasks;
using WoWActivityMember.Game;
using WoWActivityMember.Game.Statics;
using WoWActivityMember.Mem;
using WoWActivityMember.Objects;

namespace FeralDruidBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Wrath = "Wrath";

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull) { }
        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther || (ObjectManager.Aggressors.Count() > 0 && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.Player.TargetGuid)))
            {
                ObjectManager.Player.StopAllMovement();
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }
            
            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < 27 && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(Wrath) && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For("PullWithWrathDelay", 100))
                {
                    if (!ObjectManager.Player.IsInCombat)
                        Functions.LuaCall($"CastSpellByName('{Wrath}')");

                    if (ObjectManager.Player.IsCasting || ObjectManager.Player.CurrentShapeshiftForm != "Human Form" || ObjectManager.Player.IsInCombat)
                    {
                        ObjectManager.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    }
                }
                return;
            }

            Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
