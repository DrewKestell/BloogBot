using Communication;
using Moq;
using WoWSharpClient.Handlers;

namespace WoWSharpClient.Tests.Handlers
{
    public class SMSG_SPELLLOGMISS_Tests
    {
        private readonly CharacterSelectHandler _characterSelectHandler;
        private readonly Mock<WoWSharpEventEmitter> _woWSharpEventEmitterMock;
        private readonly WoWSharpObjectManager _objectManagerMock;
        private readonly ActivitySnapshot _activityMember;
    }
}
