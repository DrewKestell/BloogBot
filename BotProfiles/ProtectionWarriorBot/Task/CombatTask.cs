using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionWarriorBot
{
    class CombatTask : CombatRotationTask, IBotTask
    {
        const string BattleShout = "Battle Shout";
        const string Berserking = "Berserking";
        const string Bloodrage = "Bloodrage";
        const string ConcussionBlow = "Concussion Blow";
        const string DemoralizingShout = "Demoralizing Shout";
        const string Execute = "Execute";
        const string HeroicStrike = "Heroic Strike";
        const string LastStand = "Last Stand";
        const string Overpower = "Overpower";
        const string Rend = "Rend";
        const string Retaliation = "Retaliation";
        const string ShieldBash = "Shield Bash";
        const string ShieldSlam = "Shield Slam";
        const string ThunderClap = "Thunder Clap";

        readonly WoWUnit target;
        readonly LocalPlayer player;

        internal CombatTask(IClassContainer container, Stack<IBotTask> botTasks, List<WoWUnit> targets) : base(container, botTasks, targets, 3)
        {
            player = ObjectManager.Instance.Player;
            this.target = targets[0];
        }

        public new void Update()
        {
            if (base.Update())
                return;

            TryUseAbility(Bloodrage, condition: target.HealthPercent > 50);

            if (ObjectManager.Instance.Aggressors.Count() >= 3)
            {
                TryUseAbility(Retaliation);
            }
            if (ObjectManager.Instance.Aggressors.Count() >= 2 && (!target.HasDebuff(DemoralizingShout) || !target.HasDebuff(ThunderClap)))
            {
                TryUseAbility(DemoralizingShout, 10, !target.HasDebuff(DemoralizingShout) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(player.Location) < 10));

                TryUseAbility(ThunderClap, 20, !target.HasDebuff(ThunderClap) && ObjectManager.Instance.Aggressors.All(a => a.Location.GetDistanceTo(player.Location) < 10));
            }
            else if (ObjectManager.Instance.Aggressors.Count() == 1 || (target.HasDebuff(DemoralizingShout) && target.HasDebuff(ThunderClap)))
            {
                TryUseAbility(LastStand, condition: player.HealthPercent <= 8);

                TryUseAbility(Overpower, 5, player.CanOverpower);

                TryUseAbility(Berserking, 5, player.HealthPercent < 30);
                
                TryUseAbility(ShieldBash, 10, target.IsCasting && target.Mana > 0);
                
                TryUseAbility(Rend, 10, !target.HasDebuff(Rend) && target.HealthPercent > 50 && target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead);

                TryUseAbility(BattleShout, 10, !player.HasBuff(BattleShout));

                TryUseAbility(ConcussionBlow, 15, !target.IsStunned && target.HealthPercent > 40);

                TryUseAbility(Execute, 20, target.HealthPercent < 20);

                TryUseAbility(ShieldSlam, 20, target.HealthPercent > 30);
                
                TryUseAbility(HeroicStrike, 40, target.HealthPercent > 40 && player.Casting == 0);
            }
        }
    }
}
