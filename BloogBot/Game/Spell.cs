namespace BloogBot.Game
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
    }
}
