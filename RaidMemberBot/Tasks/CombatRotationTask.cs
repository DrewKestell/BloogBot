using RaidMemberBot.Client;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;
using static RaidMemberBot.Game.Statics.WoWEventHandler;

namespace RaidMemberBot.AI.SharedStates
{
    public abstract class CombatRotationTask
    {
        const string FacingErrorMessage = "You are facing the wrong way!";
        const string LosErrorMessage = "Target not in line of sight";
        const string BattleStance = "Battle Stance";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly LocalPlayer player;
        WoWUnit target;

        bool backpedaling;
        int backpedalStartTime;
        bool noLos;
        int noLosStartTime;

        public CombatRotationTask(IClassContainer container, Stack<IBotTask> botTasks)
        {
            this.container = container;
            player = ObjectManager.Instance.Player;

            this.botTasks = botTasks;

            Instance.OnErrorMessage += OnErrorMessageCallback;
        }

        public bool Update(WoWUnit target, int desiredRange)
        {
            this.target = target;
            // melee classes occasionally end up in a weird state where they are too close to hit the mob,
            // so we backpedal a bit to correct the position
            if (backpedaling && Environment.TickCount - backpedalStartTime > 500)
            {
                player.StopMovement(ControlBits.Back);
                backpedaling = false;
            }
            if (backpedaling)
                return true;

            // the server-side los check is broken on Kronos, so we have to rely on an error message on the client.
            // when we see it, move toward the unit a bit to correct the position.
            if (noLos && Environment.TickCount - noLosStartTime > 1000)
            {
                player.StopMovement(ControlBits.Front);
                noLos = false;
            }
            if (!player.InLosWith(target))
            {
                Location[] waypoints = SocketClient.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);

                if (waypoints.Length > 1)
                {
                    player.MoveToward(waypoints[1]);
                }
            }

            // see if somebody else stole the mob we were targeting
            if (target.TappedByOther)
            {
                CleanUp();
                return true;
            }

            // when killing certain summoned units (like totems), our local reference to target will still have 100% health even after the totem is destroyed
            // so we need to lookup the target again in the object manager, and if it's null, we can assume it's dead and leave combat.
            var checkTarget = ObjectManager.Instance.Units.FirstOrDefault(u => u.Guid == target.Guid);
            if (target.Health == 0 || target.TappedByOther || checkTarget == null)
            {
                player.StopAllMovement();

                if (Wait.For("PopCombatState", 1500))
                {
                    CleanUp();
                    botTasks.Push(new LootTask(container, botTasks, target));
                }

                return true;
            }

            // ensure the correct target is set
            if (player.TargetGuid != target.Guid)
                player.SetTarget(target.Guid);

            // ensure we're facing the target
            if (!player.IsFacing(target.Location))
                player.Face(target.Location);

            // make sure casters don't move or anything while they're casting by returning here
            if ((player.IsCasting || player.IsChanneling) && player.Class != ClassId.Warrior)
                return true;

            // ensure we're in range of the target
            if (player.Location.GetDistanceTo(target.Location) > desiredRange)
            {
                var nextWaypoint = SocketClient.Instance.CalculatePath(ObjectManager.Instance.Player.MapId, player.Location, target.Location, false);

                if (nextWaypoint.Length > 1)
                    player.MoveToward(nextWaypoint[1]);
            }
            else if (player.MovementState != MovementFlags.None && player.Location.GetDistanceTo(target.Location) < desiredRange - 1)
                player.StopAllMovement();

            // ensure auto-attack is turned on ONLY if player does not have a wand
            var wand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            if (wand == null)
            {
                var autoAttackAction = player.Class == ClassId.Warrior ? 84 : 12;

                // first check if auto attack is in the correct action slot
                var isInCorrectSpot = Lua.Instance.ExecuteWithResult($"{{0}} = IsAttackAction({autoAttackAction})");
                if (isInCorrectSpot.Length == 0 || isInCorrectSpot[0] != "1")
                {
                    var error = "You must place the <Attack> action from your spellbook on the last slot on your primary action bar.";
                    Lua.Instance.Execute($"message('{error}')");
                    return false;
                }

                var autoAttackLuaScript = $"if IsCurrentAction('{autoAttackAction}') == nil then CastSpellByName('Attack') end";
                Lua.Instance.Execute(autoAttackLuaScript);
            }

            return false;
        }

        public void TryCastSpell(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, minRange, maxRange, condition, callback, castOnSelf);

        public void TryCastSpell(string name, bool condition = true, Action callback = null, bool castOnSelf = false) =>
            TryCastSpellInternal(name, 0, int.MaxValue, condition, callback, castOnSelf);

        void TryCastSpellInternal(string name, int minRange, int maxRange, bool condition = true, Action callback = null, bool castOnSelf = false)
        {
            var distanceToTarget = player.Location.GetDistanceTo(target.Location);

            if (Spellbook.Instance.IsSpellReady(name) && distanceToTarget >= minRange && distanceToTarget <= maxRange && condition && !player.IsStunned && ((!player.IsCasting && !player.IsChanneling) || player.Class == ClassId.Warrior))
            {
                Spellbook.Instance.Cast(name);
                callback?.Invoke();
            }
        }

        // shared by 
        public void TryUseAbility(string name, int requiredResource = 0, bool condition = true, Action callback = null)
        {
            int playerResource = 0;

            if (player.Class == ClassId.Warrior)
                playerResource = player.Rage;
            else if (player.Class == ClassId.Rogue)
                playerResource = player.Energy;
            // todo: feral druids (bear/cat form)

            if (Spellbook.Instance.IsSpellReady(name) && playerResource >= requiredResource && condition && !player.IsStunned && !player.IsCasting)
            {
                Spellbook.Instance.Cast(name);
                callback?.Invoke();
            }
        }

        // https://vanilla-wow.fandom.com/wiki/API_CastSpell
        // The id is counted from 1 through all spell types (tabs on the right side of SpellBookFrame).
        public void TryUseAbilityById(string name, int id, int requiredRage = 0, bool condition = true, Action callback = null)
        {
            if (Spellbook.Instance.IsSpellReady(name) && player.Rage >= requiredRage && condition && !player.IsStunned && !player.IsCasting)
            {
                Lua.Instance.Execute($"CastSpell({id}, 'spell')");
                callback?.Invoke();
            }
        }

        void CleanUp()
        {
            player.StopAllMovement();
            botTasks.Pop();
            Instance.OnErrorMessage -= OnErrorMessageCallback;
        }

        void OnErrorMessageCallback(object sender, OnUiMessageArgs e)
        {
            if (e.Message == FacingErrorMessage && !backpedaling)
            {
                backpedaling = true;
                backpedalStartTime = Environment.TickCount;
                player.StartMovement(ControlBits.Back);
            }
            else if (e.Message == LosErrorMessage)
            {
                Console.WriteLine("No LOS");
                noLos = true;
                noLosStartTime = Environment.TickCount;
            }
        }
    }
}
