namespace GameData.Core.Interfaces
{
    public interface IWoWLocalPet : IWoWUnit
    {
        void Attack();
        bool CanUse(string consumeShadows);
        void Cast(string consumeShadows);
        void FollowPlayer();
        bool IsHappy();
    }
}
