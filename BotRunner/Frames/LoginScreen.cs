namespace BotRunner.Frames
{
    public interface ILoginScreen
    {
        bool IsOpen { get; }
        void Login(string username, string password);
        uint QueuePosition { get; }
        bool InQueue { get; }
        void CancelLogin();
    }
}
