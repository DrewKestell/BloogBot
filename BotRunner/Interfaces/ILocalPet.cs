namespace BotRunner.Interfaces
{
    public interface ILocalPet : IWoWUnit
    {
        int ChannelingId { get; }

        void Attack();
        bool CanUse(string consumeShadows);
        void Cast(string consumeShadows);
        void FollowPlayer();
        bool IsHappy();
    }
}
