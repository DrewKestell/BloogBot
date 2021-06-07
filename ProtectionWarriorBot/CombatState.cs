using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Linq;

namespace ProtectionWarriorBot
{
    class CombatState : CombatStateBase, IBotState
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

        internal CombatState(Stack<IBotState> botStates, IDependencyContainer container, WoWUnit target) : base(botStates, container, target, 3)
        {
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            TryUseAbility(Bloodrage, condition: target.HealthPercent > 50);

            if (ObjectManager.Aggressors.Count() >= 3)
            {
                TryUseAbility(Retaliation);
            }
            if (ObjectManager.Aggressors.Count() >= 2 && (!target.HasDebuff(DemoralizingShout) || !target.HasDebuff(ThunderClap)))
            {
                TryUseAbility(DemoralizingShout, 10, !target.HasDebuff(DemoralizingShout) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(player.Position) < 10));

                TryUseAbility(ThunderClap, 20, !target.HasDebuff(ThunderClap) && ObjectManager.Aggressors.All(a => a.Position.DistanceTo(player.Position) < 10));
            }
            else if (ObjectManager.Aggressors.Count() == 1 || (target.HasDebuff(DemoralizingShout) && target.HasDebuff(ThunderClap)))
            {
                TryUseAbility(LastStand, condition: player.HealthPercent <= 8);

                TryUseAbility(Overpower, 5, player.CanOverpower);

                TryUseAbility(Berserking, 5, player.HealthPercent < 30);
                
                TryUseAbility(ShieldBash, 10, target.IsCasting && target.Mana > 0);
                
                TryUseAbility(Rend, 10, (!target.HasDebuff(Rend) && target.HealthPercent > 50 && (target.CreatureType != CreatureType.Elemental && target.CreatureType != CreatureType.Undead)));

                TryUseAbility(BattleShout, 10, !player.HasBuff(BattleShout));

                TryUseAbility(ConcussionBlow, 15, !target.IsStunned && target.HealthPercent > 40);

                TryUseAbility(Execute, 20, target.HealthPercent < 20);

                TryUseAbility(ShieldSlam, 20, target.HealthPercent > 30);
                
                TryUseAbility(HeroicStrike, 40, target.HealthPercent > 40 && !player.IsCasting);
            }
        }
    }
}
