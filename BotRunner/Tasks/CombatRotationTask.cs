using BotRunner.Constants;
using BotRunner.Interfaces;
using PathfindingService.Models;

namespace BotRunner.Tasks
{
    public abstract class CombatRotationTask(IBotContext botContext) : BotTask(botContext)
    {
        private Position hostileTargetLastPosition;
        private bool backpedaling;
        private readonly int backpedalStartTime;

        public abstract void PerformCombatRotation();

        public bool Update(int desiredRange)
        {
            if (ObjectManager.GetTarget(ObjectManager.Player) != null) return true;

            if (ObjectManager.Aggressors.Any(x => x.TargetGuid == ObjectManager.Player.Guid))
            {
                MoveTowardsTank();
                return true;
            }

            hostileTargetLastPosition = ObjectManager.GetTarget(ObjectManager.Player).Position;
            // melee classes occasionally end up in a weird state where they are too close to hit the mob,
            // so we backpedal a bit to correct the position
            if (backpedaling && Environment.TickCount - backpedalStartTime > 500)
            {
                ObjectManager.Player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return true;

            // the server-side los check is broken on Kronos, so we have to rely on an error message on the client.
            // when we see it, move toward the unit a bit to correct the position.
            if (!ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)) || ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) > desiredRange)
            {
                if (ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) <= desiredRange)
                {
                    ObjectManager.Player.StopAllMovement();

                    ObjectManager.Player.Face(ObjectManager.GetTarget(ObjectManager.Player).Position);
                }
                else
                {
                    Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).GetPointBehindUnit(3), true);

                    ObjectManager.Player.MoveToward(locations[1]);
                    return true;
                }
            }
            else
            {
                ObjectManager.Player.StopAllMovement();

                // ensure we're facing the target
                if (!ObjectManager.Player.IsFacing(ObjectManager.GetTarget(ObjectManager.Player).Position))
                    ObjectManager.Player.Face(ObjectManager.GetTarget(ObjectManager.Player).Position);

                // make sure casters don't move or anything while they're casting by returning here
                if ((ObjectManager.Player.IsCasting || ObjectManager.Player.IsChanneling) && ObjectManager.Player.Class != Class.Warrior && ObjectManager.Player.Class != Class.Rogue)
                    return true;
            }

            return false;
        }

        public bool MoveTowardsTank()
        {
            if (ObjectManager.PartyLeader.Position.DistanceTo(ObjectManager.Player.Position) > 5)
            {
                Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.PartyLeader.Position, true);

                ObjectManager.Player.MoveToward(locations[1]);
                return true;
            }
            else
            {
                ObjectManager.Player.StopAllMovement();
                return false;
            }
        }

        public bool MoveBehindTarget(float distance)
        {
            if (ObjectManager.GetTarget(ObjectManager.Player) == null) return true;

            if (ObjectManager.Player.IsBehind(ObjectManager.GetTarget(ObjectManager.Player))
                && ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) < distance + 1
                && ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position) > distance - 1)
            {
                return false;
            }

            Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).GetPointBehindUnit(distance), true);

            ObjectManager.Player.MoveToward(locations[1]);
            return true;
        }
        public bool MoveBehindTankSpot(float distance)
        {
            if (distance < 3)
            {
                ObjectManager.Player.StopAllMovement();
                return false;
            }

            //Position tankPosition = new(Container.State.TankPosition.X, Container.State.TankPosition.Y, Container.State.TankPosition.Z);
            //Position position = GetPointBehindPosition(tankPosition, Container.State.TankFacing, distance);
            //Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, position, true);

            //if (!tankPosition.InLosWith(position) || locations.Length < 2 || !locations[1].InLosWith(position))
            //{
            //    return MoveBehindTankSpot(distance - 1);
            //}

            //if (ObjectManager.Player.IsBehind(tankPosition, Container.State.TankFacing)
            //    && ObjectManager.Player.Position.DistanceTo(tankPosition) < distance + 1
            //    && ObjectManager.Player.Position.DistanceTo(tankPosition) > distance - 1)
            //{
            //    ObjectManager.Player.StopAllMovement();
            //    return false;
            //}

            //ObjectManager.Pet?.FollowPlayer();

            //ObjectManager.Player.MoveToward(locations[1]);
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
            if (!ObjectManager.Player.InLosWith(ObjectManager.GetTarget(ObjectManager.Player)))
            {
                Position[] locations = Container.PathfindingClient.GetPath(ObjectManager.MapId, ObjectManager.Player.Position, ObjectManager.GetTarget(ObjectManager.Player).Position, true);

                ObjectManager.Player.MoveToward(locations[1]);
                return true;
            }
            else
            {
                ObjectManager.Player.StopAllMovement();
                return false;
            }
        }

        public bool TargetMovingTowardPlayer =>
            hostileTargetLastPosition != null &&
            hostileTargetLastPosition.DistanceTo(ObjectManager.Player.Position) > ObjectManager.GetTarget(ObjectManager.Player).Position.DistanceTo(ObjectManager.Player.Position);

        public bool TargetIsFleeing =>
            hostileTargetLastPosition != null &&
            hostileTargetLastPosition.DistanceTo(ObjectManager.Player.Position) < ObjectManager.GetTarget(ObjectManager.Player).Position.DistanceTo(ObjectManager.Player.Position);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        private void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            if (ObjectManager.GetTarget(ObjectManager.Player) == null) return;

            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(ObjectManager.GetTarget(ObjectManager.Player).Position);

            if (ObjectManager.Player.IsSpellReady(name)
                && distanceToTarget >= minRange
                && distanceToTarget <= maxRange
                && condition
                && !ObjectManager.Player.IsStunned
                && (!ObjectManager.Player.IsCasting && !ObjectManager.Player.IsChanneling || ObjectManager.Player.Class == Class.Warrior)
                && Wait.For("GlobalCooldown", 1000, true))
            {
                ObjectManager.Player.CastSpell(name, -1);
                callback?.Invoke();
            }
        }

        // shared by 
        public void TryUseAbility(string name, int requiredResource = 0, bool condition = true, Action callback = null)
        {
            uint playerResource = 0;

            if (ObjectManager.Player.Class == Class.Warrior)
                playerResource = ObjectManager.Player.Rage;
            else if (ObjectManager.Player.Class == Class.Rogue)
                playerResource = ObjectManager.Player.Energy;
            // todo: feral druids (bear/cat form)

            if (ObjectManager.Player.IsSpellReady(name) && playerResource >= requiredResource && condition && !ObjectManager.Player.IsStunned && !ObjectManager.Player.IsCasting)
            {
                ObjectManager.Player.CastSpell(name, -1);
                callback?.Invoke();
            }
        }

        // https://vanilla-wow.fandom.com/wiki/API_CastSpell
        // The id is counted from 1 through all spell types (tabs on the right side of SpellBookFrame).
        public void TryUseAbilityById(string name, uint id, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Rage >= requiredRage && condition && !ObjectManager.Player.IsStunned && !ObjectManager.Player.IsCasting)
            {
                ObjectManager.Player.CastSpell(id, -1);
                callback?.Invoke();
            }
        }

        public bool TargetIsHostile()
        {
            if (ObjectManager.GetTarget(ObjectManager.Player) == null)
                return false;
            return ObjectManager.Aggressors.Any(x => x.Guid == ObjectManager.GetTarget(ObjectManager.Player).Guid);
        }

        public void AssignDPSTarget()
        {
            ObjectManager.Player.SetTarget(ObjectManager.SkullTargetGuid);
        }
    }
}
