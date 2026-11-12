using BloogBot.AI;
using BloogBot.AI.SharedStates;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AfflictionWarlockBot
{
    class BuffSelfState : SpellCastingStateBase, IBotState
    {
        const string DemonArmor = "Demon Armor";
        const string DemonSkin = "Demon Skin";
        const string FelArmor = "Fel Armor";
        const string CreateSpellstone = "Create Spellstone";
        const string SoulstoneResurrection = "Soulstone Resurrection";
        const string CreateSoulstone = "Create Soulstone";
        const string CreateHealthstone = "Create Healthstone";

        readonly Stack<IBotState> botStates;
        readonly LocalPlayer player;

        public BuffSelfState(Stack<IBotState> botStates) : base(ObjectManager.Player, ObjectManager.Player)
        {
            this.botStates = botStates;
            player = ObjectManager.Player;
            player.SetTarget(player.Guid);
        }

        public void Update()
        {
            if (player.IsCasting)
            {
                return;
            }

            if (!player.HasAura(DemonSkin) && !player.HasAura(DemonArmor) && !player.HasAura(FelArmor))
            {
                if (player.KnowsSpell(FelArmor))
                {
                    TryCastSpell(FelArmor);
                }
                else if (player.KnowsSpell(DemonArmor))
                {
                    TryCastSpell(DemonArmor);
                }
                else
                {
                    TryCastSpell(DemonSkin);
                }
            }
            else if (HasExcessiveSoulShards)
            {
                DeleteSoulShard();
            }
            else if (
                !player.MainhandIsEnchanted &&
                player.KnowsSpell(CreateSpellstone) &&
                GetSoulShards.Any())
            {
                var spellstones = GetSpellstones();
                if (spellstones.Count() == 0)
                {
                    TryCastSpell(CreateSpellstone);
                }
                else
                {
                    var spellstone = spellstones.First();
                    player.LuaCall($@"
                        RunMacroText('/use {spellstone.Info.Name}')
                        RunMacroText('/use 16')
                    ");
                }
            }
            else if (
                !player.HasAura(SoulstoneResurrection) &&
                player.KnowsSpell(CreateSoulstone) &&
                !SoulstoneOnCooldown() &&
                GetSoulShards.Any())
            {
                var soulstones = GetSoulstones();
                if (soulstones.Count() == 0)
                {
                    TryCastSpell(CreateSoulstone);
                }
                else
                {
                    soulstones.First().Use();
                }
            }
            else if (
                GetHealthstones().Count() == 0 &&
                player.KnowsSpell(CreateHealthstone) &&
                GetSoulShards.Any())
            {
                TryCastSpell(CreateHealthstone);
            }
            else
            {
                botStates.Pop();
            }
        }

        void DeleteSoulShard()
        {
            var ss = GetSoulShards.Last();
            player.LuaCall($"PickupContainerItem({Inventory.GetBagId(ss.Guid)},{Inventory.GetSlotId(ss.Guid)})");
            player.LuaCall("DeleteCursorItem()");
        }

        IEnumerable<WoWItem> GetSpellstones()
        {
            return Inventory.GetAllItems()
                .Where(i => i.Info.Name != null && SpellstoneRanks.ContainsKey(i.Info.Name))
                .OrderByDescending(i => SpellstoneRanks[i.Info.Name]);
        }

        IEnumerable<WoWItem> GetSoulstones()
        {
            return Inventory.GetAllItems()
                .Where(i => i.Info.Name != null && SoulstoneRanks.ContainsKey(i.Info.Name))
                .OrderByDescending(i => SoulstoneRanks[i.Info.Name]);
        }

        IEnumerable<WoWItem> GetHealthstones()
        {
            return Inventory.GetAllItems()
                .Where(i => i.Info.Name != null && HealthstoneRanks.ContainsKey(i.Info.Name))
                .OrderByDescending(i => HealthstoneRanks[i.Info.Name]);
        }

        bool SoulstoneOnCooldown()
        {
            var soulstone = GetSoulstones().FirstOrDefault();
            if (soulstone == null)
            {
                return false;
            }

            return player.LuaCallWithResults($@"
                {{0}} = GetItemCooldown({soulstone.ItemId})
            ")[0] != "0";
        }

        bool HasExcessiveSoulShards => GetSoulShards.Count() > 1;

        IEnumerable<WoWItem> GetSoulShards => Inventory.GetAllItems().Where(i => i.Info.Name == "Soul Shard");

        static readonly ReadOnlyDictionary<string, int> SpellstoneRanks =
            new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
        {
            { "Spellstone", 1 },
            { "Greater Spellstone", 2 },
            { "Major Spellstone", 3 },
            { "Master Spellstone", 4 },
            { "Demonic Spellstone", 5 },
            { "Grand Spellstone", 6 },
        });

        static readonly ReadOnlyDictionary<string, int> SoulstoneRanks =
            new ReadOnlyDictionary<string, int>(new Dictionary<string, int>
        {
            { "Minor Soulstone", 1 },
            { "Lesser Soulstone", 2 },
            { "Soulstone", 3 },
            { "Greater Soulstone", 4 },
            { "Major Soulstone", 5 },
            { "Master Soulstone", 6 },
            { "Demonic Soulstone", 7 },
        });

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
