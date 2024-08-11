using BotRunner.Constants;
using WoWSharpClient.Manager;

namespace WoWSharpClient.Handlers
{
    public class SpellHandler(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;
        public void HandleInitialSpells(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                byte talentSpec = reader.ReadByte();
                ushort spellCount = reader.ReadUInt16();

                for (int i = 0; i < spellCount; i++)
                {
                    ushort spellID = reader.ReadUInt16();
                    short unknown = reader.ReadInt16(); // Possible additional data

                    //Console.WriteLine($"Spell ID: {spellID}, Unknown: {unknown}");
                }

                ushort cooldownCount = reader.ReadUInt16();

                for (int i = 0; i < cooldownCount; i++)
                {
                    ushort cooldownSpellID = reader.ReadUInt16();
                    ushort cooldownItemID = reader.ReadUInt16();
                    ushort cooldownSpellCategory = reader.ReadUInt16();
                    int cooldownTime = reader.ReadInt32();
                    uint cooldownCategoryTime = reader.ReadUInt32();

                    string cooldownCategoryTimeStr = (cooldownCategoryTime >> 31) != 0 ? "Infinite" : (cooldownCategoryTime & 0x7FFFFFFF).ToString();
                    //Console.WriteLine($"Cooldown Spell ID: {cooldownSpellID}, Cooldown Time: {cooldownTime}, Cooldown Category Time: {cooldownCategoryTimeStr}");
                }

                _woWSharpEventEmitter.FireOnInitialSpellsLoaded(); // Trigger event or further processing
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading initial spells: {e.Message}");
            }
        }

        public void HandleSpellLogMiss(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                uint spellId = reader.ReadUInt32();
                ulong casterGUID = reader.ReadUInt64();
                ulong targetGUID = reader.ReadUInt64();
                uint missType = reader.ReadUInt32();

                _woWSharpEventEmitter.FireOnSpellLogMiss(spellId, casterGUID, targetGUID, missType);
            }
            catch (EndOfStreamException e)
            {
                Console.WriteLine($"Error reading spell log miss: {e}");
            }
        }

        public void HandleSpellGo(Opcodes opcode, byte[] data)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            try
            {
                uint casterGUID = reader.ReadUInt32();
                uint targetGUID = reader.ReadUInt32();
                uint spellID = reader.ReadUInt32();

                _woWSharpEventEmitter.FireOnSpellGo(spellID, casterGUID, targetGUID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling SMSG_SPELL_GO: {ex.Message}");
            }
        }
    }
}
