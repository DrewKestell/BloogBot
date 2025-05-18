using GameData.Core.Enums;
using GameData.Core.Models;

namespace GameData.Core.Frames
{
    public interface ICharacterSelectScreen
    {
        bool IsOpen { get; }
        bool HasEnteredWorld { get; }
        bool HasReceivedCharacterList { get; set; }
        void CreateCharacter(
            string name,
            Race race,
            Gender gender,
            Class @class,
            byte skinColor,
            byte face,
            byte hairStyle,
            byte hairColor,
            byte miscAttribute);
        void DeleteCharacter(ulong characterGuid);
        void EnterWorld(ulong characterGuid);
        List<CharacterSelect> CharacterSelects { get; }
        void RefreshCharacterListFromServer();
    }
}
