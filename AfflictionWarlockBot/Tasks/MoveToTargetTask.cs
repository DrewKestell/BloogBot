using BloogBot;
using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace AfflictionWarlockBot
{
    class MoveToTargetTask : IBotTask
    {
        const string SummonImp = "Summon Imp";
        const string SummonVoidwalker = "Summon Voidwalker";
        const string CurseOfAgony = "Curse of Agony";
        const string ShadowBolt = "Shadow Bolt";

        readonly IClassContainer container;
        readonly Stack<IBotTask> botTasks;
        readonly WoWUnit target;
        readonly LocalPlayer player;
        readonly StuckHelper stuckHelper;

        readonly string pullingSpell;

        internal MoveToTargetTask(IClassContainer container, Stack<IBotTask> botTasks, WoWUnit target)
        {
            this.container = container;
            this.botTasks = botTasks;
            this.target = target;
            player = ObjectManager.Player;
            stuckHelper = new StuckHelper(container, botTasks);

            if (player.KnowsSpell(CurseOfAgony))
                pullingSpell = CurseOfAgony;
            else
                pullingSpell = ShadowBolt;
        }

        public void Update()
        {
            if (target.TappedByOther)
            {
                player.StopAllMovement();
                botTasks.Pop();
                return;
            }

            if (ObjectManager.Pet == null && (player.KnowsSpell(SummonImp) || player.KnowsSpell(SummonVoidwalker)))
            {
                player.StopAllMovement();
                botTasks.Push(new SummonVoidwalkerTask(container, botTasks));
                return;
            }

            stuckHelper.CheckIfStuck();

            var distanceToTarget = player.Position.DistanceTo(target.Position);
            if (distanceToTarget < 27 && !player.IsCasting && player.IsSpellReady(pullingSpell))
            {
                if (player.IsMoving)
                    player.StopAllMovement();

                if (Wait.For("AfflictionWarlockPullDelay", 250))
                {
                    player.StopAllMovement();
                    player.LuaCall($"CastSpellByName('{pullingSpell}')");
                    botTasks.Pop();
                    botTasks.Push(new CombatTask(container, botTasks, new List<WoWUnit>() { target }));
                }

                return;
            }

            var nextWaypoint = Navigation.GetNextWaypoint(ObjectManager.MapId, player.Position, target.Position, false);
            player.MoveToward(nextWaypoint);
        }
    }
}
