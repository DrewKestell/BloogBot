using GameData.Core.Enums;
using Microsoft.Extensions.Logging;
using Moq;
using WoWSharpClient.Handlers;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    public class SMSG_UPDATE_OBJECT_Tests
    {
        private readonly ObjectUpdateHandler _objectUpdateHandler;
        private readonly WoWSharpObjectManager _objectManager;
        private readonly Mock<Logger<WoWSharpObjectManager>> _logger = new();

        public SMSG_UPDATE_OBJECT_Tests()
        {
            // Initialize your dependencies using mocks or stubs
            _objectManager = new("127.0.0.1", _logger.Object);

            // Initialize ObjectUpdateHandler with mocked dependencies
            _objectUpdateHandler = new ObjectUpdateHandler(_objectManager);
        }

        [Fact]
        public void ShouldDecompressAndParseAllCompressedUpdateObjectPackets()
        {
            var opcode = Opcode.SMSG_UPDATE_OBJECT;
            var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString());

            var files = Directory.GetFiles(directoryPath, "20240815_*.bin")
                .OrderBy(path =>
                {
                    var fileName = Path.GetFileNameWithoutExtension(path);
                    var parts = fileName.Split('_');
                    return parts.Length > 1 && int.TryParse(parts[1], out var index) ? index : int.MaxValue;
                });

            foreach (var filePath in files)
            {
                byte[] data = FileReader.ReadBinaryFile(filePath);
                _objectUpdateHandler.HandleUpdateObject(opcode, data);
            }
        }
    }
}
