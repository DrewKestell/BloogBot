namespace ArcaneMageBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly string pullingSpell;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (ObjectManager.Player.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;
        }

        public void Update()
        {
            if (ObjectManager.Player.Target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);
            if (distanceToTarget < 27)
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (ObjectManager.Player.IsCasting && ObjectManager.Player.IsSpellReady(pullingSpell) && Wait.For("ArcaneMagePull", 500))
                {
                    ObjectManager.Player.StopAllMovement();
                    Wait.RemoveAll();
                    Functions.LuaCall($"CastSpellByName('{pullingSpell}')");
                    BotTasks.Pop();
                    BotTasks.Push(new PvERotationTask(Container, BotTasks));
                    return;
                }
            }
            else
            {
                Position[] nextWaypoint = Navigation.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.Player.Target.Position, true);
                ObjectManager.Player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
