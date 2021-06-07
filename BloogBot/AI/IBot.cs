using System;
using System.Collections.Generic;

namespace BloogBot.AI
{
    public interface IBot
    {
        string Name { get; }

        string FileName { get; }

        IDependencyContainer GetDependencyContainer(BotSettings botSettings, Probe probe, IEnumerable<Hotspot> hotspots);

        bool Running();

        void Start(IDependencyContainer container, Action stopCallback);

        void Stop();

        void Travel(IDependencyContainer container, bool reverseTravelPath, Action callback);

        void StartPowerlevel(IDependencyContainer container, Action stopCallback);

        void Test(IDependencyContainer container);
    }
}
