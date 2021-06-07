using BloogBot.Game;
using System.Collections.Generic;
using System.Linq;

namespace BloogBot
{
    static public class HotspotGenerator
    {
        static IList<Position> positions = new List<Position>();

        static public bool Recording { get; private set; }

        static public int PositionCount => positions.Count;

        static public void Record()
        {
            Recording = true;
            positions.Clear();
        }

        static public void AddWaypoint(Position position) => positions.Add(position);

        static public void Cancel() => Recording = false;

        static public Position[] Save()
        {
            Recording = false;
            return positions.ToArray();
        }
    }
}
