using BotRunner.Constants;

namespace BotRunner.Frames
{
    public interface ICharacterSelectScreen
    {
        bool IsOpen { get; }
        void CreateCharacter(Race race,
            Gender gender, 
            Class @class, 
            int skinColor, 
            int face, 
            int hairStyle, 
            int hairColor, 
            int miscAttribute, 
            string name);
        void DeleteCharacter(ulong characterGuid);
        void EnterWorld(ulong characterGuid);
    }
}
