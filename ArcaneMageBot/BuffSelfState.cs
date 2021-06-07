using BloogBot.AI;
using BloogBot.Game;
using BloogBot.Game.Objects;
using System.Collections.Generic;

namespace ArcaneMageBot
{
    class BuffSelfState : IBotState
    {
        const string ArcaneIntellect = "Arcane Intellect";
        const string FrostArmor = "Frost Armor";
        const string IceArmor = "Ice Armor";
        const string DampenMagic = "Dampen Magic";

        readonly Stack<IBotState> botStates;
        readonly IDependencyContainer container;
        readonly LocalPlayer player;

        public BuffSelfState(Stack<IBotState> botStates, IDependencyContainer container)
        {
            this.botStates = botStates;
            this.container = container;
            player = ObjectManager.Player;
        }

        public void Update()
        {
            if ((!player.KnowsSpell(ArcaneIntellect) || player.HasBuff(ArcaneIntellect)) && (player.HasBuff(FrostArmor) || player.HasBuff(IceArmor)) && (!player.KnowsSpell(DampenMagic) || player.HasBuff(DampenMagic)))
            {
                botStates.Pop();
                botStates.Push(new ConjureItemsState(botStates, container));
                return;
            }
            
            TryCastSpell(ArcaneIntellect, castOnSelf: true);

            if (player.KnowsSpell(IceArmor))
                TryCastSpell(IceArmor);
            else
                TryCastSpell(FrostArmor);

            TryCastSpell(DampenMagic, castOnSelf: true);
        }

        void TryCastSpell(string name, bool castOnSelf = false)
        {
            if (!player.HasBuff(name) && player.KnowsSpell(name) && player.IsSpellReady(name))
            {
                var castOnSelfString = castOnSelf ? ",1" : "";
                player.LuaCall($"CastSpellByName('{name}'{castOnSelfString})");
            }
        }
    }
}
