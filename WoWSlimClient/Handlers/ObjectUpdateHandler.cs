using Newtonsoft.Json;
using Org.BouncyCastle.Utilities;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

namespace WoWSlimClient.Handlers
{
    internal class ObjectUpdateHandler
    {
        public static void HandleUpdateObject(Opcodes opcode, byte[] data)
        {
            if (opcode == Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT)
            {
                data = PacketManager.Decompress(data.Skip(4).ToArray());
            }

            using (var stream = new MemoryStream(data))
            using (var reader = new BinaryReader(stream))
            {
                try
                {
                    var objectCount = reader.ReadUInt32();
                    var hasTransport = reader.ReadByte();
                    var updateType = (ObjectUpdateType)reader.ReadByte();

                    for (int i = 0; i < 1; i++)
                    {
                        switch (updateType)
                        {
                            case ObjectUpdateType.CREATE_OBJECT:
                            case ObjectUpdateType.CREATE_OBJECT2:
                                var guid = ReadPackedGuid(reader);
                                ParseCreateObject(reader, guid);
                                break;
                            case ObjectUpdateType.MOVEMENT:
                                break;
                            case ObjectUpdateType.PARTIAL:
                                break;
                            //case ObjectUpdateType.NEAR_OBJECTS:
                            //    break;
                            case ObjectUpdateType.OUT_OF_RANGE_OBJECTS:
                                uint count = reader.ReadUInt32();
                                for (int j = 0; j < count; j++)
                                {
                                    var outOfRangeGuid = ReadPackedGuid(reader);
                                    ObjectManager.Instance.Objects.Remove(ObjectManager.Instance.Objects.First(x => x.Guid == outOfRangeGuid));
                                }
                                break;
                            default:
                                Console.WriteLine($" {BitConverter.ToString(data)}");
                                return;
                        }
                    }
                }
                catch (EndOfStreamException e)
                {
                    Console.WriteLine($"Exception: {e}");
                }
            }
        }

        private static void ParseCreateObject(BinaryReader reader, ulong guid)
        {
            var objectType = (WoWObjectType)reader.ReadByte();
            byte[] guidBytes = BitConverter.GetBytes(guid);
            WoWObject woWObject = null;

            switch (objectType)
            {
                case WoWObjectType.None:
                    woWObject = new WoWObject(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.Item:
                    woWObject = new WoWItem(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.Container:
                    woWObject = new WoWContainer(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.Unit:
                    woWObject = new WoWUnit(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.Player:
                    woWObject = new WoWPlayer(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.GameObj:
                    woWObject = new WoWGameObject(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.DynamicObj:
                    woWObject = new WoWDynamicObject(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                case WoWObjectType.Corpse:
                    woWObject = new WoWCorpse(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray());
                    break;
                default:
                    Console.WriteLine($"Unknown Object Type:{guid} {objectType}");
                    break;
            }

            if (woWObject != null)
            {
                ObjectManager.Instance.Objects.Add(woWObject);
            }

            ObjectUpdateFlags updateFlags = (ObjectUpdateFlags)reader.ReadByte();

            Console.WriteLine();
            Console.Write($"Guid: {guid}\n-ObjectType: {objectType}\n-Update Flags: {updateFlags}");
            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                WoWUnit currentUnit = ObjectManager.Instance.Units.FirstOrDefault(x => x.Guid == guid);
                currentUnit.MovementFlags = (MovementFlags)reader.ReadUInt32();
                currentUnit.LastUpated = reader.ReadUInt32();

                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                float posZ = reader.ReadSingle();

                currentUnit.Position = new Position(posX, posY, posZ);
                currentUnit.Facing = reader.ReadSingle();

                float fallTime = reader.ReadSingle();
                float walkSpeed = reader.ReadSingle();
                float runSpeed = reader.ReadSingle();
                float runBackSpeed = reader.ReadSingle();
                float swimSpeed = reader.ReadSingle();
                float swimBackSpeed = reader.ReadSingle();
                float turnRate = reader.ReadSingle();
                Console.Write($"\n-Position X,Y,Z: {posX} {posY} {posZ}\n-Facing: {currentUnit.Facing}");
            }
            else if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
            {
                WoWObject currentObject = ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid);

                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                float posZ = reader.ReadSingle();

                currentObject.Position = new Position(posX, posY, posZ);
                currentObject.Facing = reader.ReadSingle();
                Console.Write($"\n-Position X,Y,Z: {posX} {posY} {posZ}\n-Facing: {currentObject.Facing}");
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                ulong timestamp = reader.ReadUInt32();
                Console.Write($"\n-Transport Timestamp: {timestamp}");
            }

            byte isPlayer = reader.ReadByte();
            byte[] unknownHardcoded = reader.ReadBytes(3);

            byte maskBlocksCount = reader.ReadByte();

            uint[] maskBlocks = new uint[maskBlocksCount];
            Console.Write($"\n-Mask Blocks: {maskBlocks.Length}");
            for (int i = 0; i < maskBlocksCount; i++)
            {
                maskBlocks[i] = reader.ReadUInt32();
                Console.Write($"\n--Mask Block: {maskBlocks[i]}");
            }

            ParseUpdateFIelds(reader, guid, objectType, updateFlags, maskBlocks);
        }

        private static void ParseUpdateFIelds(BinaryReader reader, ulong guid, WoWObjectType objectType, ObjectUpdateFlags updateFlags, uint[] maskBlocks)
        {
            TypeMask objectTypeMask = ParseObjectUpdateFields(reader, guid, updateFlags, maskBlocks);

            switch (objectType)
            {
                case WoWObjectType.Item:
                    ParseItemUpdateFields(reader, guid, maskBlocks);
                    break;
                case WoWObjectType.Container:
                    ParseContainerUpdateFields(reader, guid, maskBlocks);
                    break;
                    //case WoWObjectType.Unit:
                    //    ParseUnitUpdateFields(reader, guid, maskBlocks);
                    //    break;
                    //case WoWObjectType.Player:
                    //    ParsePlayerUpdateFields(reader, guid, maskBlocks);
                    //    break;
                    //case WoWObjectType.GameObj:
                    //    ParseGameObjectUpdateFields(reader, guid, maskBlocks);
                    //    break;
                    //case WoWObjectType.DynamicObj:
                    //    ParseDynamicObjectUpdateFields(reader, guid, maskBlocks);
                    //    break;
                    //case WoWObjectType.Corpse:
                    //    ParseCorpseUpdateFields(reader, guid, maskBlocks);
                    //    break;
            }
        }

        private static TypeMask ParseObjectUpdateFields(BinaryReader reader, ulong guid, ObjectUpdateFlags objectUpdateFlags, uint[] maskBlocks)
        {
            TypeMask objectTypeMask = TypeMask.TYPEMASK_OBJECT;

            byte[] lowGuid = [0x00, 0x00, 0x00, 0x00];
            byte[] highGuid = [0x00, 0x00, 0x00, 0x00];
            List<UpdateFields.EObjectFields> eObjectFields = Enum.GetValues(typeof(UpdateFields.EObjectFields)).Cast<UpdateFields.EObjectFields>().ToList();

            for (int i = (int)eObjectFields[0]; i < (int)eObjectFields[eObjectFields.Count - 1]; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.EObjectFields)i)
                    {
                        case UpdateFields.EObjectFields.OBJECT_FIELD_GUID:
                            lowGuid = reader.ReadBytes(4);
                            if (((objectUpdateFlags & ObjectUpdateFlags.UPDATEFLAG_HIGHGUID) != 0 
                                || (objectUpdateFlags & ObjectUpdateFlags.UPDATEFLAG_FULLGUID) != 0 
                                || (objectUpdateFlags & ObjectUpdateFlags.UPDATEFLAG_ALL) != 0) && (objectTypeMask & TypeMask.TYPEMASK_PLAYER) == 0)
                            {
                                highGuid = reader.ReadBytes(4);
                            }

                            Console.Write($"\n--[{(UpdateFields.EObjectFields)i}]{BitConverter.ToString(lowGuid)}");
                            Console.Write($"\n--[{(UpdateFields.EObjectFields)i}]{BitConverter.ToString(highGuid)}");
                            i++;
                            break;
                        case UpdateFields.EObjectFields.OBJECT_FIELD_TYPE:
                            objectTypeMask = (TypeMask)reader.ReadUInt32();
                            Console.Write($"\n--[{(UpdateFields.EObjectFields)i}]{objectTypeMask}");
                            break;
                        case UpdateFields.EObjectFields.OBJECT_FIELD_ENTRY:
                            var objectFieldEntry = reader.ReadUInt32();
                            Console.Write($"\n--[{(UpdateFields.EObjectFields)i}]{objectFieldEntry}");
                            ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid).Entry = objectFieldEntry;
                            break;
                        case UpdateFields.EObjectFields.OBJECT_FIELD_SCALE_X:
                            var objectFieldScale = reader.ReadSingle();
                            Console.Write($"\n--[{(UpdateFields.EObjectFields)i}]{objectFieldScale}");
                            ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid).ScaleX = objectFieldScale;
                            break;
                        case UpdateFields.EObjectFields.OBJECT_FIELD_PADDING:
                            var objectFieldPadding = reader.ReadBytes(4);
                            Console.Write($"\n--[{(UpdateFields.EObjectFields)i}]{objectFieldPadding}");
                            break;
                    }
                }
            }
            return objectTypeMask;
        }

        private static void ParseItemUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            for (int i = (int)UpdateFields.EItemFields.ITEM_FIELD_OWNER; i < (int)UpdateFields.EItemFields.ITEM_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.EItemFields)i)
                    {
                        case UpdateFields.EItemFields.ITEM_FIELD_OWNER:
                        case UpdateFields.EItemFields.ITEM_FIELD_CONTAINED:
                        case UpdateFields.EItemFields.ITEM_FIELD_CREATOR:
                        case UpdateFields.EItemFields.ITEM_FIELD_GIFTCREATOR:
                            var guidValue = reader.ReadUInt64();
                            Console.WriteLine($"\n--[{(UpdateFields.EItemFields)i}]: {guidValue}");
                            i++; // Increment by one more as these fields are of size 2 (8 bytes)
                            break;
                        case UpdateFields.EItemFields.ITEM_FIELD_STACK_COUNT:
                        case UpdateFields.EItemFields.ITEM_FIELD_DURATION:
                        case UpdateFields.EItemFields.ITEM_FIELD_FLAGS:
                        case UpdateFields.EItemFields.ITEM_FIELD_PROPERTY_SEED:
                        case UpdateFields.EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID:
                        case UpdateFields.EItemFields.ITEM_FIELD_ITEM_TEXT_ID:
                        case UpdateFields.EItemFields.ITEM_FIELD_DURABILITY:
                        case UpdateFields.EItemFields.ITEM_FIELD_MAXDURABILITY:
                            var uintValue = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EItemFields)i}]: {uintValue}");
                            break;
                        case UpdateFields.EItemFields.ITEM_FIELD_SPELL_CHARGES:
                        case UpdateFields.EItemFields.ITEM_FIELD_SPELL_CHARGES_01:
                        case UpdateFields.EItemFields.ITEM_FIELD_SPELL_CHARGES_02:
                        case UpdateFields.EItemFields.ITEM_FIELD_SPELL_CHARGES_03:
                        case UpdateFields.EItemFields.ITEM_FIELD_SPELL_CHARGES_04:
                            var spellCharges = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EItemFields)i}]: {spellCharges}");
                            break;
                        case UpdateFields.EItemFields.ITEM_FIELD_ENCHANTMENT:
                            for (int j = 0; j < 21; j++)
                            {
                                var enchantment = reader.ReadUInt32();
                                Console.WriteLine($"\n--[{UpdateFields.EItemFields.ITEM_FIELD_ENCHANTMENT} {j}]: {enchantment}");
                            }
                            break;
                    }
                }
            }
        }

        private static void ParseContainerUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            // First parse item fields
            ParseItemUpdateFields(reader, guid, maskBlocks);

            for (int i = (int)UpdateFields.EContainerFields.CONTAINER_FIELD_NUM_SLOTS; i < (int)UpdateFields.EContainerFields.CONTAINER_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.EContainerFields)i)
                    {
                        case UpdateFields.EContainerFields.CONTAINER_FIELD_NUM_SLOTS:
                        case UpdateFields.EContainerFields.CONTAINER_ALIGN_PAD:
                            var uintValue = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EContainerFields)i}]: {uintValue}");
                            break;
                        case UpdateFields.EContainerFields.CONTAINER_FIELD_SLOT_1:
                            for (int j = 0; j < 56; j++)
                            {
                                var slotItem = reader.ReadUInt32();
                                Console.WriteLine($"\n--[{UpdateFields.EContainerFields.CONTAINER_FIELD_SLOT_1} {j}]: {slotItem}");
                            }
                            break;
                        case UpdateFields.EContainerFields.CONTAINER_FIELD_SLOT_LAST:
                            var slotLastItem = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EContainerFields)i}]: {slotLastItem}");
                            break;
                    }
                }
            }
        }

        private static void ParseUnitUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            WoWUnit wowUnit = ObjectManager.Instance.Units.FirstOrDefault(x => x.Guid == guid);

            if (wowUnit == null)
            {
                Console.WriteLine($"Unit with GUID {guid} not found.");
                return;
            }

            List<UpdateFields.EUnitFields> eUnitFields = Enum.GetValues(typeof(UpdateFields.EUnitFields)).Cast<UpdateFields.EUnitFields>().ToList();
            for (int i = (int)UpdateFields.EUnitFields.UNIT_FIELD_CHARM; i < (int)UpdateFields.EUnitFields.UNIT_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    ReadUnitField(reader, wowUnit, (UpdateFields.EUnitFields)i);
                }
            }
        }

        private static void ReadUnitField(BinaryReader reader, WoWUnit wowUnit, UpdateFields.EUnitFields field)
        {
            switch (field)
            {
                case UpdateFields.EUnitFields.UNIT_FIELD_CHARM:
                    wowUnit.CharmGuid = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_SUMMON:
                    wowUnit.SummonGuid = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_CHARMEDBY:
                    wowUnit.CharmedByGuid = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_SUMMONEDBY:
                    wowUnit.SummonedByGuid = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_CREATEDBY:
                    wowUnit.CreatedBy = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_TARGET:
                    wowUnit.TargetGuid = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_PERSUADED:
                    wowUnit.PersuadedGuid = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_CHANNEL_OBJECT:
                    wowUnit.ChannelObject = reader.ReadUInt64();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_HEALTH:
                    wowUnit.Health = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_POWER1:
                    wowUnit.Mana = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_POWER2:
                    wowUnit.Rage = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_POWER3:
                    wowUnit.Energy = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_POWER4:
                    wowUnit.Focus = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_POWER5:
                    wowUnit.Happiness = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_MAXHEALTH:
                    wowUnit.MaxHealth = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_MAXPOWER1:
                    wowUnit.MaxMana = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_MAXPOWER2:
                    wowUnit.MaxRage = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_MAXPOWER3:
                    wowUnit.MaxEnergy = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_MAXPOWER4:
                    wowUnit.MaxFocus = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_MAXPOWER5:
                    wowUnit.MaxHappiness = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_LEVEL:
                    wowUnit.Level = (int)reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_FACTIONTEMPLATE:
                    wowUnit.FactionTemplate = reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_BYTES_0:
                    wowUnit.Bytes0 = reader.ReadBytes(4);
                    break;
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_01:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02:
                    wowUnit.VirtualItemSlotDisplay[field - UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY] = reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO_01:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO_02:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO_03:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO_04:
                case UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO_05:
                    wowUnit.VirtualItemInfo[field - UpdateFields.EUnitFields.UNIT_VIRTUAL_ITEM_INFO] = reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_FLAGS:
                    wowUnit.Flags = reader.ReadUInt32();
                    break;
                case UpdateFields.EUnitFields.UNIT_FIELD_AURA:
                case UpdateFields.EUnitFields.UNIT_FIELD_AURA_LAST:
                    wowUnit.AuraFields[field - UpdateFields.EUnitFields.UNIT_FIELD_AURA] = reader.ReadUInt32();
                    break;
                default:
                    Console.WriteLine($"Unhandled unit field: {field}");
                    break;
            }

            Console.WriteLine($"[ParseUnitUpdateFields][{field}] {JsonConvert.SerializeObject(wowUnit)}");
        }

        private static void ParsePlayerUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            ParseUnitUpdateFields(reader, guid, maskBlocks);

            List<UpdateFields.EUnitFields> eUnitFIelds = Enum.GetValues(typeof(UpdateFields.EUnitFields)).Cast<UpdateFields.EUnitFields>().ToList();
            for (int i = (int)UpdateFields.EUnitFields.UNIT_END; i < (int)eUnitFIelds[eUnitFIelds.Count - 1]; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.EUnitFields)i)
                    {
                        case UpdateFields.EUnitFields.PLAYER_DUEL_ARBITER:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FLAGS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_GUILDID:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_GUILDRANK:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_BYTES:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_BYTES_2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_BYTES_3:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_DUEL_TEAM:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_GUILD_TIMESTAMP:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_QUEST_LOG_1_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_QUEST_LOG_1_2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_QUEST_LOG_1_3:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_QUEST_LOG_LAST_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_QUEST_LOG_LAST_2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_QUEST_LOG_LAST_3:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_1_0:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_1_PAD:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_LAST_CREATOR:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_LAST_0:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_LAST_PROPERTIES:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_VISIBLE_ITEM_LAST_PAD:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_INV_SLOT_HEAD:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_PACK_SLOT_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_PACK_SLOT_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BANK_SLOT_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BANK_SLOT_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_KEYRING_SLOT_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_KEYRING_SLOT_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FARSIGHT:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_COMBO_TARGET:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_XP:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_NEXT_LEVEL_XP:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_SKILL_INFO_1_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_CHARACTER_POINTS1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_CHARACTER_POINTS2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_TRACK_CREATURES:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_TRACK_RESOURCES:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_BLOCK_PERCENTAGE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_DODGE_PERCENTAGE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_PARRY_PERCENTAGE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_CRIT_PERCENTAGE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_RANGED_CRIT_PERCENTAGE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_EXPLORED_ZONES_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_REST_STATE_EXPERIENCE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_COINAGE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_POSSTAT0:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_POSSTAT1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_POSSTAT2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_POSSTAT3:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_POSSTAT4:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_NEGSTAT0:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_NEGSTAT1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_NEGSTAT2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_NEGSTAT3:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_NEGSTAT4:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BYTES:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_AMMO_ID:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_SELF_RES_SPELL:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_PVP_MEDALS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_SESSION_KILLS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_YESTERDAY_KILLS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_LAST_WEEK_KILLS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_THIS_WEEK_KILLS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_YESTERDAY_CONTRIBUTION:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_LAST_WEEK_RANK:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_BYTES2:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_WATCHED_FACTION_INDEX:
                            break;
                        case UpdateFields.EUnitFields.PLAYER_FIELD_COMBAT_RATING_1:
                            break;
                    }

                }
            }
        }
        private static void ParseDynamicObjectUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            List<UpdateFields.EDynamicObjectFields> eObjectFields = Enum.GetValues(typeof(UpdateFields.EDynamicObjectFields)).Cast<UpdateFields.EDynamicObjectFields>().ToList();
            for (int i = (int)eObjectFields[0]; i < (int)eObjectFields[eObjectFields.Count - 1]; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.EDynamicObjectFields)i)
                    {
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_CASTER:
                            var casterGuid = reader.ReadUInt64();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{casterGuid}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_BYTES:
                            var bytes = reader.ReadBytes(4);
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{BitConverter.ToString(bytes)}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_SPELLID:
                            var spellId = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{spellId}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_RADIUS:
                            var radius = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{radius}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_POS_X:
                            var posX = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{posX}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_POS_Y:
                            var posY = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{posY}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_POS_Z:
                            var posZ = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{posZ}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_FACING:
                            var facing = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{facing}");
                            break;
                        case UpdateFields.EDynamicObjectFields.DYNAMICOBJECT_PAD:
                            var padding = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{padding}");
                            break;
                    }
                }
            }
        }
        private static void ParseGameObjectUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            List<UpdateFields.EGameObjectFields> eObjectFields = Enum.GetValues(typeof(UpdateFields.EGameObjectFields)).Cast<UpdateFields.EGameObjectFields>().ToList();

            for (int i = (int)eObjectFields[0]; i < (int)eObjectFields[eObjectFields.Count - 1]; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.EGameObjectFields)i)
                    {
                        case UpdateFields.EGameObjectFields.OBJECT_FIELD_CREATED_BY:
                            var createdByGuid = reader.ReadUInt64();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{createdByGuid}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_DISPLAYID:
                            var spellId = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{spellId}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_FLAGS:
                            var flags = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{flags}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_ROTATION:
                            var rotation = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{rotation}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_STATE:
                            var state = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{state}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_POS_X:
                            var posX = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{posX}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_POS_Y:
                            var posY = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{posY}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_POS_Z:
                            var posZ = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{posZ}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_FACING:
                            var facing = reader.ReadSingle();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{facing}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_DYN_FLAGS:
                            var dynFlags = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{dynFlags}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_FACTION:
                            var faction = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{faction}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_TYPE_ID:
                            var typeId = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{typeId}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_LEVEL:
                            var level = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{level}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_ARTKIT:
                            var artKit = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{artKit}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_ANIMPROGRESS:
                            var animProgress = reader.ReadUInt32();
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{animProgress}");
                            break;
                        case UpdateFields.EGameObjectFields.GAMEOBJECT_PADDING:
                            var padding = reader.ReadBytes(4);
                            Console.WriteLine($"\n--[{(UpdateFields.EDynamicObjectFields)i}]{BitConverter.ToString(padding)}");
                            break;
                    }
                }
            }
        }
        private static void ParseCorpseUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            List<UpdateFields.ECorpseFields> eObjectFields = Enum.GetValues(typeof(UpdateFields.ECorpseFields)).Cast<UpdateFields.ECorpseFields>().ToList();
            for (int i = (int)eObjectFields[0]; i < (int)eObjectFields[eObjectFields.Count - 1]; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((UpdateFields.ECorpseFields)i)
                    {
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_OWNER:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_FACING:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_POS_X:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_POS_Y:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_POS_Z:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_DISPLAY_ID:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_ITEM:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_BYTES_1:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_BYTES_2:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_GUILD:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_FLAGS:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_FIELD_PAD:
                            break;
                        case UpdateFields.ECorpseFields.CORPSE_END:
                            break;
                    }
                }
            }
        }

        private static ulong ReadPackedGuid(BinaryReader reader)
        {
            ulong guid = 0;
            byte mask = reader.ReadByte();
            int bitIndex = 0;

            while (mask != 0)
            {
                if ((mask & 1) != 0)
                {
                    byte guidByte = reader.ReadByte();
                    guid |= (ulong)guidByte << (bitIndex * 8);
                }
                mask >>= 1;
                bitIndex++;
            }

            return guid;
        }
    }
}