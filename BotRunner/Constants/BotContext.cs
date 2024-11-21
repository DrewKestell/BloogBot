using BotRunner.Interfaces;

namespace BotRunner.Constants
{
    public class BotContext(IObjectManager objectManager,
                      IWoWEventHandler woWEventHandler)
    {
        public IObjectManager ObjectManager { get; } = objectManager;
        public IWoWEventHandler WoWEventHandler { get; } = woWEventHandler;

        public async Task<List<(Action, List<object>)>> GetNextActionsWithParamsAsync()
        {
            // Example response from the decision engine
            // This would be generated dynamically based on the game's state
            List<(Action, List<object>)> actionMap =
            [
                ( Action.GoTo, new List<object> { "HerbNodeLocation" } ),
                ( Action.InteractWith, new List<object> { "HerbNode" } ),
                ( Action.GoTo, new List<object> { "OreNodeLocation" } ),
                ( Action.InteractWith, new List<object> { "OreNode" } )
            ];

            return actionMap;
        }
    }
}
