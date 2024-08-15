using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using Communication;
using Moq;
using WoWSharpClient.Handlers;
using WoWSharpClient.Manager;
using WoWSharpClient.Models;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    public class ObjectUpdateHandlerTests
    {
        private readonly ObjectUpdateHandler _objectUpdateHandler;
        private readonly Mock<WoWSharpEventEmitter> _woWSharpEventEmitterMock;
        private readonly ObjectManager _objectManagerMock;
        private readonly ActivityMemberState _activityMember;

        public ObjectUpdateHandlerTests()
        {
            // Initialize your dependencies using mocks or stubs
            _woWSharpEventEmitterMock = new Mock<WoWSharpEventEmitter>();
            _activityMember = new ActivityMemberState();
            _objectManagerMock = new ObjectManager(_woWSharpEventEmitterMock.Object, _activityMember);

            // Initialize ObjectUpdateHandler with mocked dependencies
            _objectUpdateHandler = new ObjectUpdateHandler(_woWSharpEventEmitterMock.Object, _objectManagerMock);
        }

        [Fact]
        public void ShouldDecompressAndParseTransports()
        {
            var opcode = Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\20240815_1.bin");

            // Verify that objects with the expected GUIDs were added to the ObjectManager
            Assert.Equal(5, _objectManagerMock.Objects.Count);

            // First object
            ulong expectedGuid1 = 2287828610704211975;
            var addedObject1 = (GameObject)_objectManagerMock.Objects.First(o => o.Guid == expectedGuid1);

            Assert.NotNull(addedObject1);
            Assert.Equal(WoWObjectType.GameObj, addedObject1.ObjectType);
            Assert.Equal((uint)175080, addedObject1.Entry);
            Assert.Equal(1.000f, addedObject1.ScaleX);
            Assert.Equal((uint)3031, addedObject1.DisplayId);
            Assert.Equal(GOState.Ready, addedObject1.GoState);
            Assert.Equal((uint)15, addedObject1.TypeId);
            Assert.Equal((uint)303309, addedObject1.Level);
            Assert.Equal((uint)255, addedObject1.AnimProgress);

            // Second object
            ulong expectedGuid2 = 2287828610704211974;
            var addedObject2 = (GameObject)_objectManagerMock.Objects.First(o => o.Guid == expectedGuid2);

            Assert.NotNull(addedObject2);
            Assert.Equal(WoWObjectType.GameObj, addedObject2.ObjectType);
            Assert.Equal((uint)164871, addedObject2.Entry);
            Assert.Equal(1.000f, addedObject2.ScaleX);
            Assert.Equal((uint)3031, addedObject2.DisplayId);
            Assert.Equal(GOState.Ready, addedObject2.GoState);
            Assert.Equal((uint)15, addedObject2.TypeId);
            Assert.Equal((uint)356176, addedObject2.Level);
            Assert.Equal((uint)255, addedObject2.AnimProgress);

            // Third object
            ulong expectedGuid3 = 2287828610704211973;
            var addedObject32 = (GameObject)_objectManagerMock.Objects.First(o => o.Guid == expectedGuid3);

            Assert.NotNull(addedObject32);
            Assert.Equal(WoWObjectType.GameObj, addedObject32.ObjectType);
            Assert.Equal((uint)177233, addedObject32.Entry);
            Assert.Equal(1.000f, addedObject32.ScaleX);
            Assert.Equal((uint)3015, addedObject32.DisplayId);
            Assert.Equal(GOState.Ready, addedObject32.GoState);
            Assert.Equal((uint)15, addedObject32.TypeId);
            Assert.Equal((uint)316916, addedObject32.Level);
            Assert.Equal((uint)255, addedObject32.AnimProgress);

            // Fourth object
            ulong expectedGuid4 = 2287828610704211970;
            var addedObject4 = (GameObject)_objectManagerMock.Objects.First(o => o.Guid == expectedGuid4);

            Assert.NotNull(addedObject4);
            Assert.Equal(WoWObjectType.GameObj, addedObject4.ObjectType);
            Assert.Equal((uint)176244, addedObject4.Entry);
            Assert.Equal(1.000f, addedObject4.ScaleX);
            Assert.Equal((uint)3015, addedObject4.DisplayId);
            Assert.Equal(GOState.Ready, addedObject4.GoState);
            Assert.Equal((uint)15, addedObject4.TypeId);
            Assert.Equal((uint)316182, addedObject4.Level);
            Assert.Equal((uint)255, addedObject4.AnimProgress);

            // Fifth object
            ulong expectedGuid5 = 2287828610704211969;
            var addedObject5 = (GameObject)_objectManagerMock.Objects.First(o => o.Guid == expectedGuid5);

            Assert.NotNull(addedObject5);
            Assert.Equal(WoWObjectType.GameObj, addedObject5.ObjectType);
            Assert.Equal((uint)20808, addedObject5.Entry);
            Assert.Equal(1.000f, addedObject5.ScaleX);
            Assert.Equal((uint)3015, addedObject5.DisplayId);
            Assert.Equal(GOState.Ready, addedObject5.GoState);
            Assert.Equal((uint)15, addedObject5.TypeId);
            Assert.Equal((uint)350660, addedObject5.Level);
            Assert.Equal((uint)255, addedObject5.AnimProgress);
        }

        [Fact]
        public void ShouldDecompressAndParsePlayerCharacter()
        {
            var opcode = Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\20240815_1.bin");

            // Call the HandleUpdateObject method on ObjectUpdateHandler
            _objectUpdateHandler.HandleUpdateObject(opcode, data);

            // Verify that objects with the expected GUIDs were added to the ObjectManager
            Assert.Equal(12, _objectManagerMock.Objects.Count);
        }
        [Fact]
        public void ShouldDecompressAndParseWorldObjects()
        {
            var opcode = Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\20240815_1.bin");

            // Call the HandleUpdateObject method on ObjectUpdateHandler
            _objectUpdateHandler.HandleUpdateObject(opcode, data);

            // Verify that objects with the expected GUIDs were added to the ObjectManager
            Assert.Equal(12, _objectManagerMock.Objects.Count);
        }
    }
}
