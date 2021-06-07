using BloogBot.Game;
using BloogBot.Game.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BloogBot
{
    static public class TravelPathGenerator
    {
        // TODO: this is wrong. we need a better way to notify the UI.
        static Action callback;

        static Position previousPosition;
        static readonly IList<Position> positions = new List<Position>();

        static public void Initialize(Action parCallback)
        {
            callback = parCallback;
        }

        static public bool Recording { get; private set; }

        static public int PositionCount => positions.Count;

        static public async void Record(WoWPlayer player, Action<string> log)
        {
            Recording = true;
            previousPosition = player.Position;
            positions.Clear();

            while (Recording)
            {
                if (previousPosition.DistanceTo(player.Position) > 1)
                {
                    positions.Add(player.Position);
                    previousPosition = player.Position;
                    log("Adding waypoint " + positions.Count);
                }
                else
                    log("Player hasn't moved. Holding...");

                callback();
                await Task.Delay(1000);
            }
        }

        static public void Cancel() => Recording = false;

        static public Position[] Save()
        {
            Recording = false;
            return positions.ToArray();
        }
    }
}
