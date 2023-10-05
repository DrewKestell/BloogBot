using RaidMemberBot.AI;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Helpers;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using static RaidMemberBot.Constants.Enums;

namespace FrostMageBot
{
    class MoveToTargetTask : IBotTask
    {
        const string waitKey = "FrostMagePull";

        const string Fireball = "Fireball";
        const string Frostbolt = "Frostbolt";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;
        readonly int range;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.botTasks = botTasks;
            this.container = container;
            this.target = target;
            player = ObjectManager.Instance.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (Spellbook.Instance.IsSpellReady(Frostbolt))
                pullingSpell = Frostbolt;
            else
                pullingSpell = Fireball;

            range = 28 + (player.GetTalentRank(3, 11) * 3);
        }

        public void Update()
        {
            if (player.IsCasting)
                return;

            if (target.TappedByOther)
            {
                player.StopMovement(ControlBits.Nothing);
                botTasks.Pop();
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Location.GetDistanceTo(target.Location);
            if (distanceToTarget <= range && player.InLosWith(target.Location))
            {
                if (player.IsMoving)
                    player.StopMovement(ControlBits.Nothing);

                if (Wait.For(waitKey, 250))
                {
                    player.StopMovement(ControlBits.Nothing);
                    Wait.Remove(waitKey);
                    
                    if (!player.IsInCombat)
                        Lua.Instance.Execute($"CastSpellByName('{pullingSpell}')");

                    botTasks.Pop();
                    botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                    return;
                }
            }
            else
            {
                var nextWaypoint = Navigation.Instance.CalculatePath(player.MapId, player.Location, target.Location, false);
                player.MoveToward(nextWaypoint[0]);
            }
        }
    }
}
