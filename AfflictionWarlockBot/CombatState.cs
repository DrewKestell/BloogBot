using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Enums;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AfflictionWarlockBot
{
    class CombatState : CombatStateBase, IBotState
    {
        const string TurnOffWandLuaScript = "if IsAutoRepeatAction(11) ~= nil then CastSpellByName('Shoot') end";

        const string Corruption = "Corruption";
        const string CurseOfAgony = "Curse of Agony";
        const string DeathCoil = "Death Coil";
        const string DrainSoul = "Drain Soul";
        const string Immolate = "Immolate";
        const string LifeTap = "Life Tap";
        const string ShadowBolt = "Shadow Bolt";
        const string SiphonLife = "Siphon Life";
        const string UnstableAffliction = "Unstable Affliction";
        const string Haunt = "Haunt";
        const string DarkPact = "Dark Pact";

        readonly LocalPlayer player;
        readonly WoWUnit target;

        internal CombatState(
            Stack<IBotState> botStates,
            IDependencyContainer container,
            WoWUnit target,
            bool loot = true) : base(botStates, container, target, 30, loot)
        {
            player = ObjectManager.Player;
            this.target = target;
        }

        public new void Update()
        {
            if (base.Update())
                return;

            ObjectManager.Pet?.Attack();

            if (ObjectManager.Pet != null &&
                ObjectManager.Pet.IsInCombat &&
                !player.InLosWith(target.Position))
            {
                // We don't have LOS. Chances are our pet is out of our LOS. Move it back.
                player.LuaCall("PetFollow()");
            }

            // if target is low on health, turn off wand and cast drain soul
            if (target.HealthPercent <= 20)
            {
                player.LuaCall(TurnOffWandLuaScript);
                TryCastSpell(DrainSoul, 0, 29);
            }

            var wand = Inventory.GetEquippedItem(EquipSlot.Ranged);
            if (wand != null &&
                (player.ManaPercent <= 10 ||
                (target.HealthPercent <= 60 && target.HealthPercent > 20) &&
                !player.IsChanneling &&
                !player.IsCasting))
            {
                player.LuaCall("RunMacroText('/cast !Shoot')");
            }
            else
            {
                TryCastSpell(DeathCoil, 0, 30, (target.IsCasting || target.IsChanneling) && target.HealthPercent > 20);

                TryCastSpell(DarkPact, 0, int.MaxValue, player.ManaPercent < 80);

                TryCastSpell(LifeTap, 0, int.MaxValue, player.HealthPercent > 85 && player.ManaPercent < 80);

                TryCastSpell(Haunt, 0, 30, !target.HasAura(Haunt));

                TryCastSpell(CurseOfAgony, 0, 30, !target.HasAura(CurseOfAgony) && target.HealthPercent > 90);

                if (player.KnowsSpell(UnstableAffliction))
                {
                    TryCastSpell(UnstableAffliction, 0, 30,
                        condition:
                            !target.HasAura(UnstableAffliction) &&
                            target.HealthPercent > 30);
                }
                else
                {
                    TryCastSpell(Immolate, 0, 30,
                        condition:
                            !target.HasAura(Immolate) &&
                            target.HealthPercent > 30);
                }

                TryCastSpell(Corruption, 0, 30, !target.HasAura(Corruption) && target.HealthPercent > 30);

                TryCastSpell(SiphonLife, 0, 30, !target.HasAura(SiphonLife) && target.HealthPercent > 50);

                TryCastSpell(ShadowBolt, 0, 30, target.HealthPercent > 40 || wand == null);
            }

            if (player.HealthPercent < 50)
            {
                Inventory.GetAllItems()
                    .Where(i => i.Info.Name != null && HealthstoneRanks.ContainsKey(i.Info.Name))
                    .OrderByDescending(i => HealthstoneRanks[i.Info.Name])
                    .FirstOrDefault()?.Use();
            }
        }

        static readonly ReadOnlyDictionary<string, int> HealthstoneRanks =
            new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
        {
            { "Minor Healthstone", 1 },
            { "Lesser Healthstone", 2 },
            { "Healthstone", 3 },
            { "Greater Healthstone", 4 },
            { "Major Healthstone", 5 },
            { "Master Healthstone", 6 },
            { "Demonic Healthstone", 7 },
            { "Fel Healthstone", 8 },
        });
    }
}
