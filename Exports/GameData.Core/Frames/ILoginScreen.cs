namespace GameData.Core.Frames
{
    public interface ILoginScreen
    {
        bool IsOpen { get; }
        void Login(string username, string password);
        uint QueuePosition { get; }
        void CancelLogin();
        bool IsLoggedIn { get; }
    }
}
