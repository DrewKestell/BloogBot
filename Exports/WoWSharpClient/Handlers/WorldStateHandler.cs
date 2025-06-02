using GameData.Core.Enums;
using GameData.Core.Interfaces;

namespace WoWSharpClient.Handlers
{
    public class WorldStateHandler(WoWSharpObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _eventEmitter = objectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = objectManager;
        public void HandleInitWorldStates(Opcode opcode, byte[] data)
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
                // Read ZoneId (4 bytes)
                uint zoneId = reader.ReadUInt32();
                // Read PhaseId (4 bytes)
                uint phaseId = reader.ReadUInt32();
                // Read Unknown1 (4 bytes)
                uint unknown1 = reader.ReadUInt32();
                // Read Unknown2 (2 bytes)
                ushort unknown2 = reader.ReadUInt16();

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
                        break;
                    }

                    worldStates.Add(new WorldState { StateId = worldStateId, StateValue = value });
                }

                // Process the world states as needed
                _eventEmitter.FireOnWorldStatesInit(worldStates);
            }
            catch (EndOfStreamException e)
            {
                //Console.WriteLine($"Error reading world states: {e}");
            }
        }
    }
}
