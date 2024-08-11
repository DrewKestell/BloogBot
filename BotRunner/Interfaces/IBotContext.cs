namespace BotRunner.Interfaces
{
    /// <summary>
    /// Represents the context in which a bot operates, providing access to various managers and handlers.
    /// </summary>
    public interface IBotContext
    {
        /// <summary>
        /// Gets the object manager, which handles the management of in-game objects.
        /// </summary>
        IObjectManager ObjectManager { get; }

        /// <summary>
        /// Gets the event handler, which manages event handling for the bot.
        /// </summary>
        IWoWEventHandler EventHandler { get; }

        /// <summary>
        /// Gets the class container, which provides access to class-specific functionalities.
        /// </summary>
        IClassContainer Container { get; }

        /// <summary>
        /// Gets the stack of bot tasks, representing the tasks that the bot needs to perform.
        /// </summary>
        Stack<IBotTask> BotTasks { get; }
    }
}
