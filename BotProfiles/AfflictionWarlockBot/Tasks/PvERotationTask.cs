using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Constants;
using RaidMemberBot.Game;
using RaidMemberBot.Game.Statics;
using RaidMemberBot.Mem;
using RaidMemberBot.Objects;
using System.Collections.Generic;
using System.Linq;
using static RaidMemberBot.Constants.Enums;

namespace AfflictionWarlockBot
{
    class PvERotationTask : CombatRotationTask, IBotTask
    {
        const string WandLuaScript = "if IsAutoRepeatAction(11) == nil then CastSpellByName('Shoot') end";
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        const string Corruption = "Corruption";
        const string CurseOfAgony = "Curse of Agony";
        const string DeathCoil = "Death Coil";
        const string DrainSoul = "Drain Soul";
        const string Immolate = "Immolate";
        const string LifeTap = "Life Tap";
        const string ShadowBolt = "Shadow Bolt";
        const string SiphonLife = "Siphon Life";

        internal PvERotationTask(IClassContainer container, Stack<IBotTask> botTasks) : base(container, botTasks) { }

        public void Update()
        {
            if (ObjectManager.Aggressors.Count() == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (Container.HostileTarget == null || Container.HostileTarget.HealthPercent <= 0 || (Container.HostileTarget.HasDebuff(CurseOfAgony) && Container.HostileTarget.HasDebuff(Immolate) && Container.HostileTarget.HasDebuff(Corruption)))
            {
                if (ObjectManager.Aggressors.Any(u => !u.HasDebuff(CurseOfAgony) || !u.HasDebuff(Immolate) || !u.HasDebuff(Corruption)))
                {
                    Container.HostileTarget = ObjectManager.Aggressors.First(u => !u.HasDebuff(CurseOfAgony) || !u.HasDebuff(Immolate) || !u.HasDebuff(Corruption));
                    ObjectManager.Player.SetTarget(Container.HostileTarget.Guid);
                }
                else if (!ObjectManager.Player.IsCasting)
                {
                    ObjectManager.Player.StartAttack();
                }
            }

            if (Update(Container.HostileTarget, 30))
                return;

            ObjectManager.Pet?.Attack();

            // if target is low on health, turn off wand and cast drain soul
            if (Container.HostileTarget.HealthPercent <= 20)
            {
                Functions.LuaCall(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }
            else
            {
                //TryCastSpell(DeathCoil, 0, 30, (target.IsCasting || Container.HostileTarget.IsChanneling) && Container.HostileTarget.HealthPercent > 20);

                TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Player.HealthPercent > 85 && ObjectManager.Player.ManaPercent < 80);

                TryCastSpell(CurseOfAgony, 0, 30, !Container.HostileTarget.HasDebuff(CurseOfAgony) && Container.HostileTarget.HealthPercent > 90);

                TryCastSpell(Immolate, 0, 30, !Container.HostileTarget.HasDebuff(Immolate) && Container.HostileTarget.HealthPercent > 30);

                TryCastSpell(Corruption, 0, 30, !Container.HostileTarget.HasDebuff(Corruption) && Container.HostileTarget.HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 30, !Container.HostileTarget.HasDebuff(SiphonLife) && Container.HostileTarget.HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 30, Container.HostileTarget.HealthPercent > 40);
            }
        }
    }
}
