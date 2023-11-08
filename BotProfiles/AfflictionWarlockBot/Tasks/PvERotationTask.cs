using RaidMemberBot.AI;
using RaidMemberBot.AI.SharedStates;
using RaidMemberBot.Game.Statics;
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
            if (ObjectManager.Instance.Aggressors.Count == 0)
            {
                BotTasks.Pop();
                return;
            }

            if (Container.HostileTarget == null || Container.HostileTarget.HealthPercent <= 0 || (Container.HostileTarget.GotDebuff(CurseOfAgony) && Container.HostileTarget.GotDebuff(Immolate) && Container.HostileTarget.GotDebuff(Corruption)))
            {
                Container.HostileTarget = ObjectManager.Instance.Aggressors.First(u => !u.GotDebuff(CurseOfAgony) || !u.GotDebuff(Immolate) || !u.GotDebuff(Corruption));
                ObjectManager.Instance.Player.SetTarget(Container.HostileTarget);
            }

            if (Update(Container.HostileTarget, 30))
                return;

            ObjectManager.Instance.Pet?.Attack();

            // if target is low on health, turn off wand and cast drain soul
            if (Container.HostileTarget.HealthPercent <= 20)
            {
                Lua.Instance.Execute(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }

            WoWItem wand = Inventory.Instance.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null && (ObjectManager.Instance.Player.ManaPercent <= 10 || Container.HostileTarget.HealthPercent <= 60 && Container.HostileTarget.HealthPercent > 20 && ObjectManager.Instance.Player.Channeling == 0 && ObjectManager.Instance.Player.IsCasting))
                Lua.Instance.Execute(WandLuaScript);
            else
            {
                //TryCastSpell(DeathCoil, 0, 30, (target.IsCasting || Container.HostileTarget.IsChanneling) && Container.HostileTarget.HealthPercent > 20);

                TryCastSpell(LifeTap, 0, int.MaxValue, ObjectManager.Instance.Player.HealthPercent > 85 && ObjectManager.Instance.Player.ManaPercent < 80);

                TryCastSpell(CurseOfAgony, 0, 30, !Container.HostileTarget.GotDebuff(CurseOfAgony) && Container.HostileTarget.HealthPercent > 90);

                TryCastSpell(Immolate, 0, 30, !Container.HostileTarget.GotDebuff(Immolate) && Container.HostileTarget.HealthPercent > 30);

                TryCastSpell(Corruption, 0, 30, !Container.HostileTarget.GotDebuff(Corruption) && Container.HostileTarget.HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 30, !Container.HostileTarget.GotDebuff(SiphonLife) && Container.HostileTarget.HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 30, Container.HostileTarget.HealthPercent > 40 || wand == null);
            }
        }
    }
}
