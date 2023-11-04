using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
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

        Location currentWaypoint;
        Location hostileTargetLastLocation;

        bool backpedaling;
        int backpedalStartTime;
        bool noLos;
        int noLosStartTime;

        public CombatRotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks, TaskType.Combat)
        {
            currentWaypoint = container.Player.Location;
            Instance.OnErrorMessage += OnErrorMessageCallback;
        }

        public bool Update(WoWUnit target, int desiredRange)
        {
            Container.HostileTarget = target;
            Container.Player.SetTarget(target);

            hostileTargetLastLocation = Container.HostileTarget.Location;

            // melee classes occasionally end up in a weird state where they are too close to hit the mob,
            // so we backpedal a bit to correct the position
            if (backpedaling && Environment.TickCount - backpedalStartTime > 500)
            {
                Container.Player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return true;

            // the server-side los check is broken on Kronos, so we have to rely on an error message on the client.
            // when we see it, move toward the unit a bit to correct the position.
            if (!Container.Player.InLosWith(target) || Container.Player.Location.GetDistanceTo(target.Location) > desiredRange)
            {
                Location[] locations = NavigationClient.Instance.CalculatePath(Container.Player.MapId, Container.Player.Location, Container.HostileTarget.Location, true);

                if (locations.Count(loc => Container.Player.InLosWith(loc)) > 1)
                {
                    currentWaypoint = locations.Where(loc => Container.Player.InLosWith(loc)).ToArray()[1];
                }
                else
                {
                    currentWaypoint = locations[1];
                }

                Container.Player.MoveToward(currentWaypoint);
            }
            else
            {
                Container.Player.StopAllMovement();
             
                // ensure the correct target is set
                if (Container.Player.TargetGuid != Container.HostileTarget.Guid)
                    Container.Player.SetTarget(target.Guid);

                // ensure we're facing the target
                if (!Container.Player.IsFacing(target.Location))
                    Container.Player.Face(target.Location);

                // make sure casters don't move or anything while they're casting by returning here
                if ((Container.Player.IsCasting || Container.Player.IsChanneling) && Container.Player.Class != ClassId.Warrior)
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
            //var checkTarget = ObjectManager.Instance.Units.FirstOrDefault(u => u.Guid == Container.HostileTarget.Guid);
            //if (target.Health == 0 || Container.HostileTarget.TappedByOther || checkTarget == null)
            //{
            //    Container.Player.StopAllMovement();

            //    if (Wait.For("PopCombatState", 1500))
            //    {
            //        CleanUp();
            //        BotTasks.Push(new LootTask(Container, BotTasks));
            //    }

            //    return true;
            //}

            // ensure auto-attack is turned on ONLY if player does not have a wand
            WoWItem wand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null)
            {
                ItemClass itemClass = wand.Info.ItemClass;
            }
            if (Container.Player.Class != ClassId.Warlock && Container.Player.Class != ClassId.Priest && Container.Player.Class != ClassId.Mage && Container.Player.Class != ClassId.Hunter)
            {
                int autoAttackAction = Container.Player.Class == ClassId.Warrior ? 84 : 12;

                // first check if auto attack is in the correct action slot
                string[] isInCorrectSpot = Lua.Instance.ExecuteWithResult($"{{0}} = IsAttackAction({autoAttackAction})");
                if (isInCorrectSpot.Length == 0 || isInCorrectSpot[0] != "1")
                {
                    string error = "You must place the <Attack> action from your spellbook on the last slot on your primary action bar.";
                    Lua.Instance.Execute($"message('{error}')");
                    return false;
                }

                string autoAttackLuaScript = $"if IsCurrentAction('{autoAttackAction}') == nil then CastSpellByName('Attack') end";
                Lua.Instance.Execute(autoAttackLuaScript);
            }

            return false;
        }

        public bool TargetMovingTowardPlayer =>
            hostileTargetLastLocation != null &&
            hostileTargetLastLocation.GetDistanceTo(Container.Player.Location) > Container.HostileTarget.Location.GetDistanceTo(Container.Player.Location);

        public bool TargetIsFleeing =>
            hostileTargetLastLocation != null &&
            hostileTargetLastLocation.GetDistanceTo(Container.Player.Location) < Container.HostileTarget.Location.GetDistanceTo(Container.Player.Location);

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            float distanceToTarget = Container.Player.Location.GetDistanceTo(Container.HostileTarget.Location);

            if (Spellbook.Instance.IsSpellReady(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !Container.Player.IsStunned && ((!Container.Player.IsCasting && !Container.Player.IsChanneling) || Container.Player.Class == ClassId.Warrior))
            {
                Spellbook.Instance.Cast(name);
                callback?.Invoke();
            }
        }

        // shared by 
        public void TryUseAbility(string name, int requiredResource = 0, bool condition = true, Action callback = null)
        {
            int playerResource = 0;

            if (Container.Player.Class == ClassId.Warrior)
                playerResource = Container.Player.Rage;
            else if (Container.Player.Class == ClassId.Rogue)
                playerResource = Container.Player.Energy;
            // todo: feral druids (bear/cat form)

            if (Spellbook.Instance.IsSpellReady(name) && playerResource >= requiredResource && condition && !Container.Player.IsStunned && !Container.Player.IsCasting)
            {
                Spellbook.Instance.Cast(name);
                callback?.Invoke();
            }
        }

        // https://vanilla-wow.fandom.com/wiki/API_CastSpell
        // The id is counted from 1 through all spell types (tabs on the right side of SpellBookFrame).
        public void TryUseAbilityById(string name, int id, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (Spellbook.Instance.IsSpellReady(name) && Container.Player.Rage >= requiredRage && condition && !Container.Player.IsStunned && !Container.Player.IsCasting)
            {
                Lua.Instance.Execute($"CastSpell({id}, 'spell')");
                callback?.Invoke();
            }
        }

        void CleanUp()
        {
            Container.Player.StopAllMovement();
            BotTasks.Pop();
            Instance.OnErrorMessage -= OnErrorMessageCallback;
        }

        void OnErrorMessageCallback(object sender, OnUiMessageArgs e)
        {
            if (e.Message == FacingErrorMessage && !backpedaling)
            {
                backpedaling = true;
                backpedalStartTime = Environment.TickCount;
                Container.Player.StartMovement(ControlBits.Back);
            }
            else if (e.Message == LosErrorMessage)
            {
                noLos = true;
                noLosStartTime = Environment.TickCount;
            }
        }
    }
}
