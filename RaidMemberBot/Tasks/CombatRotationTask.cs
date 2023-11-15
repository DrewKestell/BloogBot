using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;
using static RaidMemberBot.Game.Statics.WoWEventHandler;

namespace RaidMemberBot.AI.SharedStates
{
    public abstract class CombatRotationTask : BotTask
    {
        const string FacingErrorMessage = "You are facing the wrong way!";
        const string LosErrorMessage = "Target not in line of sight";
        const string BattleStance = "Battle Stance";

        Position currentWaypoint;
        Position hostileTargetLastPosition;

        bool backpedaling;
        int backpedalStartTime;
        bool noLos;
        int noLosStartTime;

        public CombatRotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Combat)
        {
            Instance.OnErrorMessage += OnErrorMessageCallback;
        }

        ~CombatRotationTask()
        {
            Instance.OnErrorMessage -= OnErrorMessageCallback;
        }

        public bool Update(WoWUnit target, int desiredRange)
        {
            Container.HostileTarget = target;
            ObjectManager.Player.SetTarget(target.Guid);

            hostileTargetLastPosition = Container.HostileTarget.Position;

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
            if (!ObjectManager.Player.InLosWith(target.Position) || ObjectManager.Player.Position.DistanceTo(target.Position) > desiredRange)
            {
                Position[] locations = NavigationClient.Instance.CalculatePath(ObjectManager.MapId, ObjectManager.Player.Position, Container.HostileTarget.Position, true);

                if (locations.Count(loc => ObjectManager.Player.InLosWith(loc)) > 1)
                {
                    currentWaypoint = locations.Where(loc => ObjectManager.Player.InLosWith(loc)).ToArray()[1];
                }
                else
                {
                    currentWaypoint = locations[1];
                }

                ObjectManager.Player.MoveToward(currentWaypoint);
            }
            else
            {
                ObjectManager.Player.StopAllMovement();
             
                // ensure the correct target is set
                if (ObjectManager.Player.TargetGuid != Container.HostileTarget.Guid)
                    ObjectManager.Player.SetTarget(target.Guid);

                // ensure we're facing the target
                if (!ObjectManager.Player.IsFacing(target.Position))
                    ObjectManager.Player.Face(target.Position);

                // make sure casters don't move or anything while they're casting by returning here
                if ((ObjectManager.Player.IsCasting || ObjectManager.Player.IsChanneling) && ObjectManager.Player.Class != Class.Warrior)
                    return true;
            }

            // see if somebody else stole the mob we were targeting
            if (target.TappedByOther)
            {
                CleanUp();
                return true;
            }

            // when killing certain summoned units (like totems), our local reference to target will still have 100% health even after the totem is destroyed
            // so we need to lookup the target again in the object manager, and if it's null, we can assume it's dead and leave combat.
            //var checkTarget = ObjectManager.Units.FirstOrDefault(u => u.Guid == Container.HostileTarget.Guid);
            //if (target.Health == 0 || Container.HostileTarget.TappedByOther || checkTarget == null)
            //{
            //    ObjectManager.Player.StopAllMovement();

            //    if (Wait.For("PopCombatState", 1500))
            //    {
            //        CleanUp();
            //        BotTasks.Push(new LootTask(Container, BotTasks));
            //    }

            //    return true;
            //}

            // ensure auto-attack is turned on ONLY if player does not have a wand           

            return false;
        }

        public bool TargetMovingTowardPlayer =>
            hostileTargetLastPosition != null &&
            hostileTargetLastPosition.DistanceTo(ObjectManager.Player.Position) > Container.HostileTarget.Position.DistanceTo(ObjectManager.Player.Position);

        public bool TargetIsFleeing =>
            hostileTargetLastPosition != null &&
            hostileTargetLastPosition.DistanceTo(ObjectManager.Player.Position) < Container.HostileTarget.Position.DistanceTo(ObjectManager.Player.Position);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            float distanceToTarget = ObjectManager.Player.Position.DistanceTo(Container.HostileTarget.Position);

            if (ObjectManager.Player.IsSpellReady(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !ObjectManager.Player.IsStunned && ((!ObjectManager.Player.IsCasting && !ObjectManager.Player.IsChanneling) || ObjectManager.Player.Class == Class.Warrior))
            {
                Functions.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }

        // shared by 
        public void TryUseAbility(string name, int requiredResource = 0, bool condition = true, Action callback = null)
        {
            int playerResource = 0;

            if (ObjectManager.Player.Class == Class.Warrior)
                playerResource = ObjectManager.Player.Rage;
            else if (ObjectManager.Player.Class == Class.Rogue)
                playerResource = ObjectManager.Player.Energy;
            // todo: feral druids (bear/cat form)

            if (ObjectManager.Player.IsSpellReady(name) && playerResource >= requiredResource && condition && !ObjectManager.Player.IsStunned && !ObjectManager.Player.IsCasting)
            {
                Functions.LuaCall($"CastSpellByName('{name}')");
                callback?.Invoke();
            }
        }

        // https://vanilla-wow.fandom.com/wiki/API_CastSpell
        // The id is counted from 1 through all spell types (tabs on the right side of SpellBookFrame).
        public void TryUseAbilityById(string name, int id, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (ObjectManager.Player.IsSpellReady(name) && ObjectManager.Player.Rage >= requiredRage && condition && !ObjectManager.Player.IsStunned && !ObjectManager.Player.IsCasting)
            {
                Functions.LuaCall($"CastSpell({id}, 'spell')");
                callback?.Invoke();
            }
        }

        void CleanUp()
        {
            ObjectManager.Player.StopAllMovement();
            BotTasks.Pop();
            Instance.OnErrorMessage -= OnErrorMessageCallback;
        }

        void OnErrorMessageCallback(object sender, OnUiMessageArgs e)
        {
            if (e.Message == FacingErrorMessage && !backpedaling)
            {
                backpedaling = true;
                backpedalStartTime = Environment.TickCount;
                ObjectManager.Player.StartMovement(ControlBits.Back);
            }
            else if (e.Message == LosErrorMessage)
            {
                noLos = true;
                noLosStartTime = Environment.TickCount;
            }
        }
    }
}
