using GameData.Core.Enums;
using GameData.Core.Interfaces;

namespace BotRunner.Constants
{
    public class BotContext(IObjectManager objectManager,
                      IWoWEventHandler woWEventHandler)
    {
        public IObjectManager ObjectManager { get; } = objectManager;
        public IWoWEventHandler WoWEventHandler { get; } = woWEventHandler;

        public static async Task<List<(CharacterAction, List<object>)>> GetNextActionsWithParamsAsync()
        {
            // Example response from the decision engine
            // This would be generated dynamically based on the game's state
            List<(CharacterAction, List<object>)> actionMap =
            [
                ( CharacterAction.GoTo, new List<object> { "HerbNodeLocation" } ),
                ( CharacterAction.InteractWith, new List<object> { "HerbNode" } ),
                ( CharacterAction.GoTo, new List<object> { "OreNodeLocation" } ),
                ( CharacterAction.InteractWith, new List<object> { "OreNode" } )
            ];

            return actionMap;
        }
    }
}
