using BloogBot.Game;

namespace BloogBot
{
    public class Npc
    {
        public Npc(
            int id,
            string name,
            bool isInnkeeper,
            bool sellsAmmo,
            bool repairs,
            bool quest,
            bool horde,
            bool alliance,
            Position position,
            string zone
            )
        {
            Id = id;
            Name = name;
            IsInnkeeper = isInnkeeper;
            SellsAmmo = sellsAmmo;
            Repairs = repairs;
            Quest = quest;
            Horde = horde;
            Alliance = alliance;
            Position = position;
            Zone = zone;
        }

        public int Id { get; }

        public string Name { get; }

        public bool IsInnkeeper { get; }

        public bool SellsAmmo { get; }

        public bool Repairs { get; }
        
        public bool Quest { get; }

        public bool Horde { get; }

        public bool Alliance { get; }

        public Position Position { get; }

        public string Zone { get; }

        public string DisplayName
        {
            get
            {
                string faction;
                if (Horde && Alliance)
                    faction = "H/A";
                else if (Horde)
                    faction = "H";
                else
                    faction = "A";

                return $"{Name} - {faction}";
            }
        }
    }
}
