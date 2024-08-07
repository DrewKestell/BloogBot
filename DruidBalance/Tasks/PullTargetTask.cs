using BotRunner.Interfaces;
using BotRunner.Tasks;
using PathfindingService.Models;

namespace DruidBalance.Tasks
{
    internal class PullTargetTask : BotTask, IBotTask
    {
        private const string Wrath = "Wrath";
        private const string Starfire = "Starfire";
        private const string MoonkinForm = "Moonkin Form";
        private readonly int range;
        private readonly string pullingSpell;

        internal PullTargetTask(IBotContext botContext) : base(botContext)
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
            if (ObjectManager.Player.Target.TappedByOther || (ObjectManager.Aggressors.Any() && !ObjectManager.Aggressors.Any(a => a.Guid == ObjectManager.Player.TargetGuid)))
            {
                Wait.RemoveAll();
                BotTasks.Pop();
                return;
            }

            if (ObjectManager.Player.IsCasting)
                return;

            if (ObjectManager.Player.IsSpellReady(MoonkinForm) && !ObjectManager.Player.HasBuff(MoonkinForm))
            {
                ObjectManager.Player.CastSpell(MoonkinForm);
            }

            if (ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position) < range && ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(pullingSpell) && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For("BalanceDruidPullDelay", 100))
                {
                    if (!ObjectManager.Player.IsInCombat)
                        ObjectManager.Player.CastSpell(pullingSpell);

                    if (ObjectManager.Player.IsCasting || ObjectManager.Player.IsInCombat)
                    {
                        ObjectManager.Player.StopAllMovement();
                        Wait.RemoveAll();
                        BotTasks.Pop();
                        BotTasks.Push(new PvERotationTask(BotContext));
                    }
                }
                return;
            }

            Position[] nextWaypoint = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
            ObjectManager.Player.MoveToward(nextWaypoint[0]);
        }
    }
}
