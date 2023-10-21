using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;

namespace EnhancementShamanBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string Clearcasting = "Clearcasting";
        const string EarthShock = "Earth Shock";
        const string FlameShock = "Flame Shock";
        const string FlametongueWeapon = "Flametongue Weapon";
        const string GroundingTotem = "Grounding Totem";
        const string HealingWave = "Healing Wave";
        const string ManaSpringTotem = "Mana Spring Totem";
        const string LightningShield = "Lightning Shield";
        const string RockbiterWeapon = "Rockbiter Weapon";
        const string SearingTotem = "Searing Totem";
        const string StoneclawTotem = "Stoneclaw Totem";
        const string StoneskinTotem = "Stoneskin Totem";
        const string Stormstrike = "Stormstrike";
        const string TremorTotem = "Tremor Totem";
        const string WindfuryWeapon = "Windfury Weapon";

        readonly string[] fearingCreatures = new[] { "Scorpid Terror" };
        readonly string[] fireImmuneCreatures = new[] { "Rogue Flame Spirit", "Burning Destroyer" };
        readonly string[] natureImmuneCreatures = new[] { "Swirling Vortex", "Gusting Vortex", "Dust Stormer" };

        readonly Stack<IBotTask> botTasks;
        readonly IClassContainer container;
        readonly LocalPlayer player;
        WoWUnit target;

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks)
        {
            this.botTasks = botTasks;
            this.container = container;
            player = ObjectManager.Instance.Player;
        }

        public void Update()
        {
            if (Container.Player.HealthPercent < 30 && Container.HostileTarget.HealthPercent > 50 && Container.Player.Mana >= Container.Player.GetManaCost(HealingWave))
            {
                BotTasks.Push(new HealTask(Container, BotTasks));
                return;
            }

            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (target == null || Container.HostileTarget.HealthPercent <= 0)
            {
                target = ObjectManager.Instance.Aggressors.First();
            }

            if (Update(target, 3))
                return;

            TryCastSpell(GroundingTotem, 0, int.MaxValue, ObjectManager.Instance.Aggressors.Any(a => a.IsCasting && Container.HostileTarget.Mana > 0));

            TryCastSpell(TremorTotem, 0, int.MaxValue, fearingCreatures.Contains(target.Name) && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(Container.Player.Location) < 29 && u.HealthPercent > 0 && u.Name.Contains(TremorTotem)));

            TryCastSpell(WindfuryWeapon, 0, int.MaxValue, !Container.Player.IsMainhandEnchanted() && Spellbook.Instance.IsSpellReady(WindfuryWeapon));

            TryCastSpell(StoneclawTotem, 0, int.MaxValue, ObjectManager.Instance.Aggressors.Count() > 1);

            TryCastSpell(ManaSpringTotem, 0, int.MaxValue, !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(Container.Player.Location) < 19 && u.HealthPercent > 0 && u.Name.Contains(ManaSpringTotem)));

            TryCastSpell(StoneskinTotem, 0, int.MaxValue, Container.HostileTarget.Mana == 0 && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(Container.Player.Location) < 19 && u.HealthPercent > 0 && (u.Name.Contains(StoneclawTotem) || u.Name.Contains(StoneskinTotem) || u.Name.Contains(TremorTotem))));

            TryCastSpell(SearingTotem, 0, int.MaxValue, Container.HostileTarget.HealthPercent > 70 && !fireImmuneCreatures.Contains(target.Name) && Container.HostileTarget.Location.GetDistanceTo(Container.Player.Location) < 20 && !ObjectManager.Instance.Units.Any(u => u.Location.GetDistanceTo(Container.Player.Location) < 19 && u.HealthPercent > 0 && u.Name.Contains(SearingTotem)));

            TryCastSpell(Stormstrike, 0, 5);

            TryCastSpell(FlameShock, 0, 20, !target.HasDebuff(FlameShock) && Container.HostileTarget.HealthPercent > 70 || natureImmuneCreatures.Contains(target.Name) && !fireImmuneCreatures.Contains(target.Name));

            TryCastSpell(EarthShock, 0, 20, !natureImmuneCreatures.Contains(target.Name) && !Spellbook.Instance.IsSpellReady(Stormstrike) && Container.HostileTarget.HealthPercent < 70 || Container.HostileTarget.HasDebuff(Stormstrike) || Container.HostileTarget.IsCasting || Container.HostileTarget.IsChanneling || Container.Player.HasBuff(Clearcasting));

            TryCastSpell(LightningShield, 0, int.MaxValue, !natureImmuneCreatures.Contains(target.Name) && !Container.Player.HasBuff(LightningShield));

            TryCastSpell(RockbiterWeapon, 0, int.MaxValue, !Container.Player.IsMainhandEnchanted() && Spellbook.Instance.IsSpellReady(RockbiterWeapon) && !Spellbook.Instance.IsSpellReady(FlametongueWeapon) && !Spellbook.Instance.IsSpellReady(WindfuryWeapon));

            TryCastSpell(FlametongueWeapon, 0, int.MaxValue, !Container.Player.IsMainhandEnchanted() && Spellbook.Instance.IsSpellReady(FlametongueWeapon) && !Spellbook.Instance.IsSpellReady(WindfuryWeapon));
        }
    }
}
