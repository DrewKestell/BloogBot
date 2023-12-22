using System.Text;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Game
{
    public class Spell
    {
        public Spell(int id, int cost, string name, string description, string tooltip)
        {
            Id = id;
            Cost = cost;
            Name = name;
            Description = description;
            Tooltip = tooltip;
        }

        public int Id { get; }

        public int Cost { get; }

        public string Name { get; }

        public string Description { get; }

        public string Tooltip { get; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"Spell {Id}:");
            sb.AppendLine($"  Cost: {Cost}");
            sb.AppendLine($"  Name: {Name}");
            sb.AppendLine($"  Description: {Description}");
            sb.AppendLine($"  Tooltip: {Tooltip}");
            return sb.ToString();
        }
    }
    public class SpellEffect
    {
        public SpellEffect(string icon, int stackCount, EffectType type)
        {
            Icon = icon;
            StackCount = stackCount;
            Type = type;
        }

        public string Icon { get; }

        public int StackCount { get; }

        public EffectType Type { get; }
    }
}
