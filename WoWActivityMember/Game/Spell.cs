using System.Text;
using static WoWActivityMember.Constants.Enums;

namespace WoWActivityMember.Game
{
    public class Spell(int id, int cost, string name, string description, string tooltip)
    {
        public int Id { get; } = id;

        public int Cost { get; } = cost;

        public string Name { get; } = name;

        public string Description { get; } = description;

        public string Tooltip { get; } = tooltip;

        public override string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"Spell {Id}:");
            sb.AppendLine($"  Cost: {Cost}");
            sb.AppendLine($"  Name: {Name}");
            sb.AppendLine($"  Description: {Description}");
            sb.AppendLine($"  Tooltip: {Tooltip}");
            return sb.ToString();
        }
    }
    public class SpellEffect(string icon, int stackCount, EffectType type)
    {
        public string Icon { get; } = icon;

        public int StackCount { get; } = stackCount;

        public EffectType Type { get; } = type;
    }
}
