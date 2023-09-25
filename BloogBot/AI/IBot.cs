using BloogBot.Models.Dto;
using System;

namespace BloogBot.AI
{
    public interface IBot
    {
        string Name { get; }

        string FileName { get; }

        IDependencyContainer GetDependencyContainer(BotSettings botSettings, CharacterState probe);

        bool Running();

        void Start(IDependencyContainer container, Action stopCallback);

        void Stop();

        void Logout();

        void Login(string accountName);

        void AddState(IBotState state);

        void ClearStack();

        void Test(IDependencyContainer container);
    }
}
