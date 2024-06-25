namespace FrostMageBot
{
    class PullTargetTask : BotTask, IBotTask
    {
        const string waitKey = "FrostMagePull";

        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly string pullingSpell;
        readonly int range;

        internal PullTargetTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Pull)
        {
            if (ObjectManager.Player.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;

            range = 28 + (ObjectManager.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (ObjectManager.Player.IsCasting)
                return;

            if (ObjectManager.Player.Target.TappedByOther)
            {
                ObjectManager.Player.StopAllMovement();
                BotTasks.Pop();
                return;
            }

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.Player.Target.Position);
            if (distanceToTarget <= range && ObjectManager.Player.InLosWith(ObjectManager.Player.Target))
            {
                if (ObjectManager.Player.IsMoving)
                    ObjectManager.Player.StopAllMovement();

                if (Wait.For(waitKey, 250))
                {
                    ObjectManager.Player.StopAllMovement();
                    Wait.Remove(waitKey);
                    
                    if (!ObjectManager.Player.IsInCombat)
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
