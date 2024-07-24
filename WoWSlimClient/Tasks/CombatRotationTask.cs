using WoWSlimClient.Manager;
using WoWSlimClient.Models;


namespace WoWSlimClient.Tasks.SharedStates
{
    public abstract class CombatRotationTask(IClassContainer container, Stack<IBotTask> botTasks) : BotTask(container, botTasks, TaskType.Combat)
    {
        private Position hostileTargetLastPosition;
        private bool backpedaling;
        private readonly int backpedalStartTime;

        public WoWUnit raidLeader = ObjectManager.Instance.PartyMembers.First(x => x.Guid == ObjectManager.Instance.PartyLeaderGuid);

        public abstract void PerformCombatRotation();

        public bool Update(int desiredRange)
        {
            if (ObjectManager.Instance.Player.Target == null) return true;

            if (ObjectManager.Instance.Aggressors.Any(x => x.TargetGuid == ObjectManager.Instance.Player.Guid))
            {
                MoveTowardsTank();
                return true;
            }

            hostileTargetLastPosition = ObjectManager.Instance.Player.Target.Position;
            // melee classes occasionally end up in a weird state where they are too close to hit the mob,
            // so we backpedal a bit to correct the position
            if (backpedaling && Environment.TickCount - backpedalStartTime > 500)
            {
                ObjectManager.Instance.Player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return true;

            // the server-side los check is broken on Kronos, so we have to rely on an error message on the client.
            // when we see it, move toward the unit a bit to correct the position.
            if (!ObjectManager.Instance.Player.InLosWith(ObjectManager.Instance.Player.Target) || ObjectManager.Instance.Player.Position.DistanceTo(ObjectManager.Instance.Player.Target.Position) > desiredRange)
            {
                if (ObjectManager.Instance.Player.Position.DistanceTo(ObjectManager.Instance.Player.Target.Position) <= desiredRange)
                {
                    ObjectManager.Instance.Player.StopAllMovement();

                    ObjectManager.Instance.Player.Face(ObjectManager.Instance.Player.Target.Position);
                }
                else
                {
                    Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, ObjectManager.Instance.Player.Target.GetPointBehindObject(3), true);

                    ObjectManager.Instance.Player.MoveToward(locations[1]);
                    return true;
                }
            }
            else
            {
                ObjectManager.Instance.Player.StopAllMovement();

                // ensure we're facing the target
                if (!ObjectManager.Instance.Player.IsFacing(ObjectManager.Instance.Player.Target.Position))
                    ObjectManager.Instance.Player.Face(ObjectManager.Instance.Player.Target.Position);

                // make sure casters don't move or anything while they're casting by returning here
                if ((ObjectManager.Instance.Player.IsCasting || ObjectManager.Instance.Player.IsChanneling) && ObjectManager.Instance.Player.Class != Class.Warrior && ObjectManager.Instance.Player.Class != Class.Rogue)
                    return true;
            }

            return false;
        }

        public bool MoveTowardsTank()
        {
            if (raidLeader.Position.DistanceTo(ObjectManager.Instance.Player.Position) > 5)
            {
                Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, raidLeader.Position, true);

                ObjectManager.Instance.Player.MoveToward(locations[1]);
                return true;
            }
            else
            {
                ObjectManager.Instance.Player.StopAllMovement();
                return false;
            }
        }

        public bool MoveBehindTarget(float distance)
        {
            if (ObjectManager.Instance.Player.Target == null) return true;

            if (ObjectManager.Instance.Player.IsBehind(ObjectManager.Instance.Player.Target)
                && ObjectManager.Instance.Player.Position.DistanceTo(ObjectManager.Instance.Player.Target.Position) < distance + 1
                && ObjectManager.Instance.Player.Position.DistanceTo(ObjectManager.Instance.Player.Target.Position) > distance - 1)
            {
                return false;
            }

            Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, ObjectManager.Instance.Player.Target.GetPointBehindObject(distance), true);

            ObjectManager.Instance.Player.MoveToward(locations[1]);
            return true;
        }
        public bool MoveBehindTankSpot(float distance)
        {
            if (distance < 3)
            {
                ObjectManager.Instance.Player.StopAllMovement();
                return false;
            }

            //Position tankPosition = new(Container.State.TankPosition.X, Container.State.TankPosition.Y, Container.State.TankPosition.Z);
            //Position position = GetPointBehindPosition(tankPosition, Container.State.TankFacing, distance);
            //Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, position, true);

            //if (!tankPosition.InLosWith(position) || locations.Length < 2 || !locations[1].InLosWith(position))
            //{
            //    return MoveBehindTankSpot(distance - 1);
            //}

            //if (ObjectManager.Instance.Player.IsBehind(tankPosition, Container.State.TankFacing)
            //    && ObjectManager.Instance.Player.Position.DistanceTo(tankPosition) < distance + 1
            //    && ObjectManager.Instance.Player.Position.DistanceTo(tankPosition) > distance - 1)
            //{
            //    ObjectManager.Instance.Player.StopAllMovement();
            //    return false;
            //}

            //ObjectManager.Pet?.FollowPlayer();

            //ObjectManager.Instance.Player.MoveToward(locations[1]);
            return true;
        }

        private Position GetPointBehindPosition(Position position, float facing, float parDistanceToMove)
        {
            var newX = position.X + parDistanceToMove * (float)-Math.Cos(facing);
            var newY = position.Y + parDistanceToMove * (float)-Math.Sin(facing);
            var end = new Position(newX, newY, position.Z);

            return end;
        }

        public bool MoveTowardsTarget()
        {
            if (!ObjectManager.Instance.Player.InLosWith(ObjectManager.Instance.Player.Target))
            {
                Position[] locations = Navigation.Instance.CalculatePath(ObjectManager.Instance.MapId, ObjectManager.Instance.Player.Position, ObjectManager.Instance.Player.Target.Position, true);

                ObjectManager.Instance.Player.MoveToward(locations[1]);
                return true;
            }
            else
            {
                ObjectManager.Instance.Player.StopAllMovement();
                return false;
            }
        }

        public bool TargetMovingTowardPlayer =>
            hostileTargetLastPosition != null &&
            hostileTargetLastPosition.DistanceTo(ObjectManager.Instance.Player.Position) > ObjectManager.Instance.Player.Target.Position.DistanceTo(ObjectManager.Instance.Player.Position);

        public bool TargetIsFleeing =>
            hostileTargetLastPosition != null &&
            hostileTargetLastPosition.DistanceTo(ObjectManager.Instance.Player.Position) < ObjectManager.Instance.Player.Target.Position.DistanceTo(ObjectManager.Instance.Player.Position);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        private void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            if (ObjectManager.Instance.Player.Target == null) return;

            float distanceToTarget = ObjectManager.Instance.Player.Position.DistanceTo(ObjectManager.Instance.Player.Target.Position);

            if (ObjectManager.Instance.Player.IsSpellReady(name)
                && distanceToTarget >= minRange
                && distanceToTarget <= maxRange
                && condition
                && !ObjectManager.Instance.Player.IsStunned
                && ((!ObjectManager.Instance.Player.IsCasting && !ObjectManager.Instance.Player.IsChanneling) || ObjectManager.Instance.Player.Class == Class.Warrior)
                && Wait.For("GlobalCooldown", 1000, true))
            {
                //Functions.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }

        // shared by 
        public void TryUseAbility(string name, int requiredResource = 0, bool condition = true, Action callback = null)
        {
            int playerResource = 0;

            if (ObjectManager.Instance.Player.Class == Class.Warrior)
                playerResource = ObjectManager.Instance.Player.Rage;
            else if (ObjectManager.Instance.Player.Class == Class.Rogue)
                playerResource = ObjectManager.Instance.Player.Energy;
            // todo: feral druids (bear/cat form)

            if (ObjectManager.Instance.Player.IsSpellReady(name) && playerResource >= requiredResource && condition && !ObjectManager.Instance.Player.IsStunned && !ObjectManager.Instance.Player.IsCasting)
            {
                //Functions.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }

        // https://vanilla-wow.fandom.com/wiki/API_CastSpell
        // The id is counted from 1 through all spell types (tabs on the right side of SpellBookFrame).
        public void TryUseAbilityById(string name, int id, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Instance.Player.IsSpellReady(name) && ObjectManager.Instance.Player.Rage >= requiredRage && condition && !ObjectManager.Instance.Player.IsStunned && !ObjectManager.Instance.Player.IsCasting)
            {
                //Functions.LuaCall($"CastSpell({id}, 'spell')");
                callback?.Invoke();
            }
        }

        public bool TargetIsHostile()
        {
            if (ObjectManager.Instance.Player.TargetGuid == 0)
                return false;
            return ObjectManager.Instance.Aggressors.Any(x => x.Guid == ObjectManager.Instance.Player.TargetGuid);
        }

        public void AssignDPSTarget()
        {
            ObjectManager.Instance.Player.SetTarget(ObjectManager.Instance.SkullTargetGuid);
        }
    }
}
