using System;

namespace BloogBot.AI
{
    public interface IBot
    {
        string Name { get; }

        string FileName { get; }

        IDependencyContainer GetDependencyContainer(BotSettings botSettings, Probe probe);

        bool Running();

        void Start(IDependencyContainer container, Action stopCallback);

        void Stop();

        void Test(IDependencyContainer container);
    }
}
