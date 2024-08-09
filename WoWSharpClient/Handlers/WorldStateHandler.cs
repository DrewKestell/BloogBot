using BotRunner.Constants;
using BotRunner.Interfaces;
using WoWSharpClient.Manager;

namespace WoWSharpClient.Handlers
{
    public class WorldStateHandler(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;
        public void HandleInitWorldStates(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                // Check if there's enough data for the fixed header fields
                if (reader.BaseStream.Length < 18)
                {
                    return;
                }

                // Read MapId (4 bytes)
                uint mapId = reader.ReadUInt32();
                //Console.WriteLine($"MapId: {mapId}");

                // Read ZoneId (4 bytes)
                uint zoneId = reader.ReadUInt32();
                //Console.WriteLine($"ZoneId: {zoneId}");

                // Read PhaseId (4 bytes)
                uint phaseId = reader.ReadUInt32();
                //Console.WriteLine($"PhaseId: {phaseId}");

                // Read Unknown1 (4 bytes)
                uint unknown1 = reader.ReadUInt32();
                //Console.WriteLine($"Unknown1: {unknown1}");

                // Read Unknown2 (2 bytes)
                ushort unknown2 = reader.ReadUInt16();
                //Console.WriteLine($"Unknown2: {unknown2}");

                // Read the World State entries
                List<WorldState> worldStates = [];
                while (reader.BaseStream.Position < reader.BaseStream.Length)
                {
                    // Check if there's enough data for the next WorldStateId and Value (8 bytes)
                    if (reader.BaseStream.Length - reader.BaseStream.Position < 8)
                    {
                        throw new EndOfStreamException("Not enough data remaining to read the next WorldStateId and Value.");
                    }

                    uint worldStateId = reader.ReadUInt32();
                    uint value = reader.ReadUInt32();

                    // Check for termination condition
                    if (worldStateId == 0 && value == 0)
                    {
                        //Console.WriteLine("Termination condition encountered: WorldStateId = 0, Value = 0");
                        break;
                    }

                    worldStates.Add(new WorldState { StateId = worldStateId, StateValue = value });
                    //Console.WriteLine($"WorldStateId: {worldStateId}, Value: {value}");
                }

                // Process the world states as needed
                _woWSharpEventEmitter.FireOnWorldStatesInit(worldStates);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading world states: {e}");
            }
        }
    }
}
