
using GameData.Core.Enums;

namespace WoWSharpClient.Handlers
{
    public class StandStateHandler(WoWSharpObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _eventEmitter = objectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = objectManager;
        public void HandleStandStateUpdate(Opcode opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                // Check if the packet length is correct
                if (reader.BaseStream.Length < 1)
                {
                    throw new EndOfStreamException("Packet is too short to contain the stand state.");
                }

                // Read Stand State (1 byte)
                byte standState = reader.ReadByte();

                // Process the stand state update as needed
                _eventEmitter.FireOnStandStateUpdate(standState);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading stand state update packet: {e.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
            }
        }
    }
}
