using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace ProtectionWarriorBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
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
        const string SunderArmor = "Sunder Armor";
        const string Taunt = "Taunt";
        const string ThunderClap = "Thunder Clap";

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        WoWUnit target;
        LocalPlayer player;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                botTasks.Pop();
                return;
            }


            if (ObjectManager.Instance.Aggressors.FindAll(x => x.TargetGuid != player.Guid).Count > 0)
            {
                target = ObjectManager.Instance.Aggressors.FindAll(x => x.TargetGuid != player.Guid).First();
                player.SetTarget(target);

                TryUseAbility(Taunt);
            }
            else
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            if (player.CurrentStance != "Defensive Stance")
            {
                TryCastSpell("Defensive Stance");
            }

            TryUseAbility(Bloodrage, condition: target.HealthPercent > 50);

            if (ObjectManager.Instance.Aggressors.Count() >= 3)
            {
                TryUseAbility(Retaliation);
            }
            if (ObjectManager.Instance.Aggressors.Count() >= 2 && (!target.HasDebuff(DemoralizingShout) || !target.HasDebuff(ThunderClap)))
            {
                TryUseAbility(DemoralizingShout, 10, !target.HasDebuff(DemoralizingShout));

                TryUseAbility(ThunderClap, 20, !target.HasDebuff(ThunderClap));
            }

            TryUseAbility(LastStand, condition: player.HealthPercent <= 8);

            TryUseAbility("Revenge", 5, player.CanOverpower);

            TryUseAbility(Berserking, 5, player.HealthPercent < 30);

            TryUseAbility(ShieldBash, 10, target.IsCasting && target.Mana > 0);

            TryUseAbility(Rend, 10, !target.HasDebuff(Rend) && target.HealthPercent > 50 && target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead);

            TryUseAbility(BattleShout, 10, !player.HasBuff(BattleShout));

            TryUseAbility(ConcussionBlow, 15, !target.IsStunned && target.HealthPercent > 40);

            TryUseAbility(Execute, 20, target.HealthPercent < 20);

            TryUseAbility(ShieldSlam, 20, target.HealthPercent > 30);

            TryUseAbility(SunderArmor, 15);

            TryUseAbility(HeroicStrike, 40, target.HealthPercent > 40 && !player.IsCasting);
        }
    }
}
