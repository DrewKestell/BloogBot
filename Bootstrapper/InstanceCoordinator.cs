using BloogBot.Models;
using BloogBot.Models.Dto;
using System.Windows.Documents;

namespace Bootstrapper
{
    public static class InstanceCoordinator
    {
        public static PartyMemberPreference[] _partyMemberPreferences;
        public static int MaxActivityCapacity { get; private set; }

        public static readonly CharacterState[] _allCharacterStates = {
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
            new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(), new CharacterState(),
        };

        public static void ConsumeMessage(CharacterState characterState)
        {
            for (int i = 0; i < MaxActivityCapacity; i++)
            {
                if (_allCharacterStates[i].ProcessId == characterState.ProcessId)
                {
                    _allCharacterStates[i] = characterState;
                    return;
                }
            }
            for (int i = 0; i < MaxActivityCapacity; i++)
            {
                if (_allCharacterStates[i].ProcessId == 0)
                {
                    _allCharacterStates[i] = characterState;
                    return;
                }
            }
        }

        public static bool RemoveInstanceByProcessId(int processId)
        {
            for (int i = 0; i < MaxActivityCapacity; i++)
            {
                if (_allCharacterStates[i].ProcessId == processId)
                {
                    _allCharacterStates[i] = new CharacterState();
                    return true;
                }
            }
            return false;
        }

        public static CharacterState GetInstanceByProcessId(int processId)
        {
            for (int i = 0; i < _partyMemberPreferences.Length; i++)
            {
                if (_allCharacterStates[i].ProcessId == processId)
                {
                    return _allCharacterStates[i];
                }
            }
            return _allCharacterStates[0];
        }

        public static void SetActivityState(PartyMemberPreference[] partyMemberPreferences, int maxCapacity = 10)
        {
            _partyMemberPreferences = partyMemberPreferences;
            MaxActivityCapacity = maxCapacity;
        }
    }
}
