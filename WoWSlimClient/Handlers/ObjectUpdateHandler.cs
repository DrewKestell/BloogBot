using WoWSlimClient.Manager;
using WoWSlimClient.Models;
using static WoWSlimClient.Models.UpdateFields;

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

            //Console.WriteLine(BitConverter.ToString(data));

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            try
            {
                var objectCount = Math.Min(reader.ReadUInt32(), 40);
                var hasTransport = reader.ReadByte();
                var updateType = (ObjectUpdateType)reader.ReadByte();

                Console.WriteLine($"Block count: {objectCount}");
                Console.WriteLine($"Has Transport: {hasTransport}");
                Console.WriteLine($"Update Type: {updateType}");

                for (int i = 0; i < objectCount; i++)
                {
                    switch (updateType)
                    {
                        case ObjectUpdateType.CREATE_OBJECT:
                        case ObjectUpdateType.CREATE_OBJECT2:
                            var guid = ReadPackedGuid(reader);
                            Console.WriteLine($"{i + 1}-Guid: {guid}");
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
        private static WoWObject CreateWoWObject(WoWObjectType objectType, byte[] guidBytes)
        {
            WoWObject wowObject = objectType switch
            {
                WoWObjectType.None => new WoWObject(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.Item => new WoWItem(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.Container => new WoWContainer(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.Unit => new WoWUnit(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.Player => new WoWPlayer(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.GameObj => new WoWGameObject(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.DynamicObj => new WoWDynamicObject(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                WoWObjectType.Corpse => new WoWCorpse(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray()),
                _ => null
            };

            return wowObject;
        }
        private static void ParseCreateObject(BinaryReader reader, ulong guid)
        {
            byte[] guidBytes = BitConverter.GetBytes(guid);
            var objectType = (WoWObjectType)reader.ReadByte();
            ObjectUpdateFlags updateFlags = (ObjectUpdateFlags)reader.ReadByte();

            Console.WriteLine($"-Object Type: {objectType}");
            Console.WriteLine($"-UpdateFlags: {updateFlags}");

            WoWObject woWObject = CreateWoWObject(objectType, guidBytes);

            //Console.WriteLine($"WoWObject woWObject: {JsonConvert.SerializeObject(woWObject)}");
            if (woWObject != null)
            {
                ObjectManager.Instance.Objects.Add(woWObject);
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                ParseUnitMovementInfo(reader, guid);
            }
            else if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
            {
                ParseObjectPositionInfo(reader, guid);
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {

                ulong highGuid = reader.ReadUInt32();
                Console.WriteLine($"-High Guid: {highGuid}");
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_ALL) != 0)
            {

                bool updateAll = reader.ReadInt32() == 1;
                Console.WriteLine($"-Update All: {updateAll}");
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_FULLGUID) != 0)
            {

                ulong victimGuid = ReadPackedGuid(reader);
                Console.WriteLine($"-Victim Guid: {victimGuid}");
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                ulong timestamp = reader.ReadUInt32();
                Console.WriteLine($"-Transport Timestamp: {timestamp} {TimeSpan.FromMilliseconds(timestamp)}");
            }

            ProcessUpdateMasks(reader, guid);
        }

        private static void ParseObjectPositionInfo(BinaryReader reader, ulong guid)
        {
            WoWObject currentObject = ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid);

            float posX = reader.ReadSingle();
            float posY = reader.ReadSingle();
            float posZ = reader.ReadSingle();

            currentObject.Position = new Position(posX, posY, posZ);
            currentObject.Facing = reader.ReadSingle();
            Console.WriteLine($"-Position X,Y,Z: {posX} {posY} {posZ}");
            Console.WriteLine($"-Facing: {currentObject.Facing}");
        }

        private static void ParseUnitMovementInfo(BinaryReader reader, ulong guid)
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

            Console.WriteLine($"-Movement Flags: {currentUnit.MovementFlags}");
            Console.WriteLine($"-Position X,Y,Z: {posX} {posY} {posZ}");
            Console.WriteLine($"-Facing: {currentUnit.Facing}");
            Console.WriteLine($"-Fall Time: {fallTime}");
            Console.WriteLine($"-Walk Speed: {walkSpeed}");
            Console.WriteLine($"-Run Speed: {runSpeed}");
            Console.WriteLine($"-Run Back Speed: {runBackSpeed}");
            Console.WriteLine($"-Swim Speed: {swimSpeed}");
            Console.WriteLine($"-Swim Back Speed: {swimBackSpeed}");
            Console.WriteLine($"-Turn Rate: {turnRate}");
        }

        private static void ProcessUpdateMasks(BinaryReader reader, ulong guid)
        {
            byte maskBlocksCount = reader.ReadByte();

            if (maskBlocksCount == 0)
                return;

            uint[] maskBlocks = new uint[maskBlocksCount];
            Console.WriteLine($"-Mask Blocks: {maskBlocks.Length}");
            for (int i = 0; i < maskBlocksCount; i++)
            {
                maskBlocks[i] = reader.ReadUInt32();
                Console.WriteLine($"--Mask Block: {BitConverter.ToString(BitConverter.GetBytes(maskBlocks[i]))}");
            }

            TypeMask objectTypeMask = ParseObjectUpdateFields(reader, guid, maskBlocks);

            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_ITEM))
            {
                ParseItemFields(reader, maskBlocks);
            }
            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_CONTAINER))
            {
                ParseContainerFields(reader, guid, maskBlocks);
            }
            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_UNIT))
            {
                ParseUnitFields(reader, guid, maskBlocks);
            }
            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_PLAYER))
            {
                ParsePlayerFields(reader, guid, maskBlocks);
            }
            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_GAMEOBJECT))
            {
                ParseGameObjectFields(reader, guid, maskBlocks);
            }
            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_DYNAMICOBJECT))
            {
                ParseDynamicObjectFields(reader, guid, maskBlocks);
            }
            if (objectTypeMask.HasFlag(TypeMask.TYPEMASK_CORPSE))
            {
                ParseCorpseFields(reader, guid, maskBlocks);
            }
        }

        private static TypeMask ParseObjectUpdateFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            TypeMask objectTypeMask = TypeMask.TYPEMASK_OBJECT;

            List<EObjectFields> eObjectFields = Enum.GetValues(typeof(EObjectFields)).Cast<EObjectFields>().ToList();

            for (int i = (int)EObjectFields.OBJECT_FIELD_GUID; i < (int)EObjectFields.OBJECT_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((EObjectFields)i)
                    {
                        case EObjectFields.OBJECT_FIELD_GUID:
                            var highGuid = (HighGuid)reader.ReadUInt32();
                            var lowGuid = (HighGuid)reader.ReadUInt32();
                            Console.WriteLine($"--[{(EObjectFields)i}]{highGuid} {lowGuid}");
                            break;
                        case EObjectFields.OBJECT_FIELD_TYPE:
                            objectTypeMask = (TypeMask)reader.ReadUInt32();
                            Console.WriteLine($"--[{(EObjectFields)i}]{objectTypeMask}");
                            break;
                        case EObjectFields.OBJECT_FIELD_ENTRY:
                            var objectFieldEntry = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EObjectFields)i}]{objectFieldEntry}");
                            ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid).Entry = objectFieldEntry;
                            break;
                        case EObjectFields.OBJECT_FIELD_SCALE_X:
                            var objectFieldScale = reader.ReadSingle();
                            Console.WriteLine($"--[{(EObjectFields)i}]{objectFieldScale:0.000}");
                            ObjectManager.Instance.Objects.FirstOrDefault(x => x.Guid == guid).ScaleX = objectFieldScale;
                            break;
                        case EObjectFields.OBJECT_FIELD_PADDING:
                            var objectFieldPadding = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EObjectFields)i}]{objectFieldPadding}");
                            break;
                    }
                }
            }
            return objectTypeMask;
        }
        private static void ParseItemFields(BinaryReader reader, uint[] maskBlocks)
        {
            for (int i = (int)EItemFields.ITEM_FIELD_OWNER; i < (int)EItemFields.ITEM_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;
                if (maskMatch)
                {
                    switch ((EItemFields)i)
                    {
                        case EItemFields.ITEM_FIELD_OWNER:
                            // Which player owns this item
                            var ownerHigh = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EItemFields)i}]: {BitConverter.ToString(ownerHigh)} {BitConverter.ToUInt32(ownerHigh)}");
                            break;
                        case EItemFields.ITEM_FIELD_GIFTCREATOR:
                            var giftCreatorHigh = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EItemFields)i}]: {BitConverter.ToString(giftCreatorHigh)} {BitConverter.ToUInt32(giftCreatorHigh)}");
                            break;
                        case EItemFields.ITEM_FIELD_CREATOR:
                            var creatorHigh = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EItemFields)i}]: {BitConverter.ToString(creatorHigh)} {BitConverter.ToUInt32(creatorHigh)}");
                            break;
                        case EItemFields.ITEM_FIELD_CONTAINED:
                            var contained1 = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EItemFields)i}]: {BitConverter.ToString(contained1)} {BitConverter.ToUInt32(contained1)}");
                            break;
                        case EItemFields.ITEM_FIELD_STACK_COUNT:
                        case EItemFields.ITEM_FIELD_DURATION:
                        case EItemFields.ITEM_FIELD_FLAGS:
                        case EItemFields.ITEM_FIELD_PROPERTY_SEED:
                        case EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID:
                        case EItemFields.ITEM_FIELD_ITEM_TEXT_ID:
                        case EItemFields.ITEM_FIELD_DURABILITY:
                        case EItemFields.ITEM_FIELD_MAXDURABILITY:
                            var uintValue = reader.ReadUInt32();
                            if ((EItemFields)i == EItemFields.ITEM_FIELD_FLAGS)
                            {
                                Console.WriteLine($"--[{(EItemFields)i}]: {BitConverter.ToString(BitConverter.GetBytes(uintValue))} {(ItemDynFlags)uintValue}");
                            }
                            else
                            {
                                Console.WriteLine($"--[{(EItemFields)i}]: {uintValue}");
                            }
                            break;
                        case EItemFields.ITEM_FIELD_SPELL_CHARGES:
                        case EItemFields.ITEM_FIELD_SPELL_CHARGES_01:
                        case EItemFields.ITEM_FIELD_SPELL_CHARGES_02:
                        case EItemFields.ITEM_FIELD_SPELL_CHARGES_03:
                        case EItemFields.ITEM_FIELD_SPELL_CHARGES_04:
                            var spellCharges = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EItemFields)i}]: {spellCharges}");
                            break;
                        case EItemFields.ITEM_FIELD_ENCHANTMENT:
                            for (int j = 0; j < 21; j++)
                            {
                                var enchantment = reader.ReadUInt32();
                                Console.WriteLine($"--[{EItemFields.ITEM_FIELD_ENCHANTMENT} {j}]: {enchantment}");
                            }
                            i += 20; // Skip over the enchantment fields
                            break;
                    }
                }
            }
        }
        private static void ParseContainerFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            uint numSlots = 0;
            for (int i = (int)EContainerFields.CONTAINER_FIELD_NUM_SLOTS; i < (int)EContainerFields.CONTAINER_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;
                if (maskMatch)
                {
                    switch ((EContainerFields)i)
                    {
                        case EContainerFields.CONTAINER_FIELD_NUM_SLOTS:
                            numSlots = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EContainerFields)i}]: {numSlots}");
                            for (int j = 0; j < numSlots; j++)
                            {
                                var highGuid = reader.ReadBytes(4);
                                var lowGuid = reader.ReadBytes(4);
                                Console.WriteLine($"---[{(EContainerFields)i}]{j}:{BitConverter.ToString(highGuid)}");
                                Console.WriteLine($"---[{(EContainerFields)i}]{j}:{BitConverter.ToString(lowGuid)}");
                                Console.WriteLine($"---[{(EContainerFields)i}]{j}:{BitConverter.ToString(reader.ReadBytes(3))}");
                                ProcessUpdateMasks(reader, guid);
                            }
                            //int j = 0;
                            //while (reader.BaseStream.Position < reader.BaseStream.Length)
                            //{
                            //    Console.WriteLine($"---[{(EContainerFields)i}]{j++}:{BitConverter.ToString(reader.ReadBytes(4))}");
                            //}
                            break;
                        case EContainerFields.CONTAINER_ALIGN_PAD:
                            var alignPad = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EContainerFields)i}]: {alignPad}");
                            break;
                        case EContainerFields.CONTAINER_FIELD_SLOT_1:
                            Console.WriteLine($"--[{(EContainerFields)i}]:");
                            break;
                        case EContainerFields.CONTAINER_FIELD_SLOT_LAST:
                            Console.WriteLine($"--[{(EContainerFields)i}]:");
                            break;
                    }
                }
            }
        }
        private static void ParseUnitFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            WoWUnit wowUnit = ObjectManager.Instance.Units.FirstOrDefault(x => x.Guid == guid);

            if (wowUnit == null)
            {
                Console.WriteLine($"Unit with GUID {guid} not found.");
                return;
            }

            for (int i = (int)EUnitFields.UNIT_FIELD_CHARM; i < (int)EUnitFields.UNIT_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    ReadUnitField(reader, wowUnit, (EUnitFields)i);
                }
            }
        }
        private static void ReadUnitField(BinaryReader reader, WoWUnit wowUnit, EUnitFields field)
        {
            switch (field)
            {
                case EUnitFields.UNIT_FIELD_CHARM:
                    wowUnit.CharmGuid = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.CharmGuid}");
                    break;
                case EUnitFields.UNIT_FIELD_SUMMON:
                    wowUnit.SummonGuid = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.SummonGuid}");
                    break;
                case EUnitFields.UNIT_FIELD_CHARMEDBY:
                    wowUnit.CharmedByGuid = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.CharmedByGuid}");
                    break;
                case EUnitFields.UNIT_FIELD_SUMMONEDBY:
                    wowUnit.SummonedByGuid = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.SummonedByGuid}");
                    break;
                case EUnitFields.UNIT_FIELD_CREATEDBY:
                    wowUnit.CreatedBy = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.CreatedBy}");
                    break;
                case EUnitFields.UNIT_FIELD_TARGET:
                    wowUnit.TargetGuid = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.TargetGuid}");
                    break;
                case EUnitFields.UNIT_FIELD_PERSUADED:
                    wowUnit.PersuadedGuid = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.PersuadedGuid}");
                    break;
                case EUnitFields.UNIT_FIELD_CHANNEL_OBJECT:
                    wowUnit.ChannelObject = reader.ReadUInt64();
                    Console.WriteLine($"--[{field}]: {wowUnit.ChannelObject}");
                    break;
                case EUnitFields.UNIT_FIELD_HEALTH:
                    wowUnit.Health = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Health}");
                    break;
                case EUnitFields.UNIT_FIELD_POWER1:
                    wowUnit.Mana = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Mana}");
                    break;
                case EUnitFields.UNIT_FIELD_POWER2:
                    wowUnit.Rage = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Rage}");
                    break;
                case EUnitFields.UNIT_FIELD_POWER3:
                    wowUnit.Energy = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Energy}");
                    break;
                case EUnitFields.UNIT_FIELD_POWER4:
                    wowUnit.Focus = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Focus}");
                    break;
                case EUnitFields.UNIT_FIELD_POWER5:
                    wowUnit.Happiness = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Happiness}");
                    break;
                case EUnitFields.UNIT_FIELD_MAXHEALTH:
                    wowUnit.MaxHealth = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.MaxHealth}");
                    break;
                case EUnitFields.UNIT_FIELD_MAXPOWER1:
                    wowUnit.MaxMana = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.MaxMana}");
                    break;
                case EUnitFields.UNIT_FIELD_MAXPOWER2:
                    wowUnit.MaxRage = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.MaxRage}");
                    break;
                case EUnitFields.UNIT_FIELD_MAXPOWER3:
                    wowUnit.MaxEnergy = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.MaxEnergy}");
                    break;
                case EUnitFields.UNIT_FIELD_MAXPOWER4:
                    wowUnit.MaxFocus = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.MaxFocus}");
                    break;
                case EUnitFields.UNIT_FIELD_MAXPOWER5:
                    wowUnit.MaxHappiness = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.MaxHappiness}");
                    break;
                case EUnitFields.UNIT_FIELD_LEVEL:
                    wowUnit.Level = (int)reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.Level}");
                    break;
                case EUnitFields.UNIT_FIELD_FACTIONTEMPLATE:
                    wowUnit.FactionTemplate = reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.FactionTemplate}");
                    break;
                case EUnitFields.UNIT_FIELD_BYTES_0:
                    wowUnit.Bytes0 = reader.ReadBytes(4);
                    Console.WriteLine($"--[{field}]: {wowUnit.Bytes0}");
                    break;
                case EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY:
                case EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_01:
                case EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02:
                    wowUnit.VirtualItemSlotDisplay[field - EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY] = reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.VirtualItemSlotDisplay[field - EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY]}");
                    break;
                case EUnitFields.UNIT_VIRTUAL_ITEM_INFO:
                case EUnitFields.UNIT_VIRTUAL_ITEM_INFO_01:
                case EUnitFields.UNIT_VIRTUAL_ITEM_INFO_02:
                case EUnitFields.UNIT_VIRTUAL_ITEM_INFO_03:
                case EUnitFields.UNIT_VIRTUAL_ITEM_INFO_04:
                case EUnitFields.UNIT_VIRTUAL_ITEM_INFO_05:
                    wowUnit.VirtualItemInfo[field - EUnitFields.UNIT_VIRTUAL_ITEM_INFO] = reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.VirtualItemInfo[field - EUnitFields.UNIT_VIRTUAL_ITEM_INFO]}");
                    break;
                case EUnitFields.UNIT_FIELD_FLAGS:
                    wowUnit.Flags = reader.ReadUInt32();
                    break;
                case EUnitFields.UNIT_FIELD_AURA:
                case EUnitFields.UNIT_FIELD_AURA_LAST:
                    wowUnit.AuraFields[field - EUnitFields.UNIT_FIELD_AURA] = reader.ReadUInt32();
                    Console.WriteLine($"--[{field}]: {wowUnit.AuraFields[field - EUnitFields.UNIT_FIELD_AURA]}");
                    break;
                default:
                    Console.WriteLine($"Unhandled unit field: {field}");
                    break;
            }
        }
        private static void ParsePlayerFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            for (int i = (int)EUnitFields.UNIT_END; i < (int)EUnitFields.PLAYER_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((EUnitFields)i)
                    {
                        case EUnitFields.PLAYER_DUEL_ARBITER:
                            break;
                        case EUnitFields.PLAYER_FLAGS:
                            break;
                        case EUnitFields.PLAYER_GUILDID:
                            break;
                        case EUnitFields.PLAYER_GUILDRANK:
                            break;
                        case EUnitFields.PLAYER_BYTES:
                            break;
                        case EUnitFields.PLAYER_BYTES_2:
                            break;
                        case EUnitFields.PLAYER_BYTES_3:
                            break;
                        case EUnitFields.PLAYER_DUEL_TEAM:
                            break;
                        case EUnitFields.PLAYER_GUILD_TIMESTAMP:
                            break;
                        case EUnitFields.PLAYER_QUEST_LOG_1_1:
                            break;
                        case EUnitFields.PLAYER_QUEST_LOG_1_2:
                            break;
                        case EUnitFields.PLAYER_QUEST_LOG_1_3:
                            break;
                        case EUnitFields.PLAYER_QUEST_LOG_LAST_1:
                            break;
                        case EUnitFields.PLAYER_QUEST_LOG_LAST_2:
                            break;
                        case EUnitFields.PLAYER_QUEST_LOG_LAST_3:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_1_0:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_1_PROPERTIES:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_1_PAD:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_LAST_CREATOR:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_LAST_0:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_LAST_PROPERTIES:
                            break;
                        case EUnitFields.PLAYER_VISIBLE_ITEM_LAST_PAD:
                            break;
                        case EUnitFields.PLAYER_FIELD_INV_SLOT_HEAD:
                            break;
                        case EUnitFields.PLAYER_FIELD_PACK_SLOT_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_PACK_SLOT_LAST:
                            break;
                        case EUnitFields.PLAYER_FIELD_BANK_SLOT_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_BANK_SLOT_LAST:
                            break;
                        case EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_LAST:
                            break;
                        case EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST:
                            break;
                        case EUnitFields.PLAYER_FIELD_KEYRING_SLOT_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_KEYRING_SLOT_LAST:
                            break;
                        case EUnitFields.PLAYER_FARSIGHT:
                            break;
                        case EUnitFields.PLAYER_FIELD_COMBO_TARGET:
                            break;
                        case EUnitFields.PLAYER_XP:
                            break;
                        case EUnitFields.PLAYER_NEXT_LEVEL_XP:
                            break;
                        case EUnitFields.PLAYER_SKILL_INFO_1_1:
                            break;
                        case EUnitFields.PLAYER_CHARACTER_POINTS1:
                            break;
                        case EUnitFields.PLAYER_CHARACTER_POINTS2:
                            break;
                        case EUnitFields.PLAYER_TRACK_CREATURES:
                            break;
                        case EUnitFields.PLAYER_TRACK_RESOURCES:
                            break;
                        case EUnitFields.PLAYER_BLOCK_PERCENTAGE:
                            break;
                        case EUnitFields.PLAYER_DODGE_PERCENTAGE:
                            break;
                        case EUnitFields.PLAYER_PARRY_PERCENTAGE:
                            break;
                        case EUnitFields.PLAYER_CRIT_PERCENTAGE:
                            break;
                        case EUnitFields.PLAYER_RANGED_CRIT_PERCENTAGE:
                            break;
                        case EUnitFields.PLAYER_EXPLORED_ZONES_1:
                            break;
                        case EUnitFields.PLAYER_REST_STATE_EXPERIENCE:
                            break;
                        case EUnitFields.PLAYER_FIELD_COINAGE:
                            break;
                        case EUnitFields.PLAYER_FIELD_POSSTAT0:
                            break;
                        case EUnitFields.PLAYER_FIELD_POSSTAT1:
                            break;
                        case EUnitFields.PLAYER_FIELD_POSSTAT2:
                            break;
                        case EUnitFields.PLAYER_FIELD_POSSTAT3:
                            break;
                        case EUnitFields.PLAYER_FIELD_POSSTAT4:
                            break;
                        case EUnitFields.PLAYER_FIELD_NEGSTAT0:
                            break;
                        case EUnitFields.PLAYER_FIELD_NEGSTAT1:
                            break;
                        case EUnitFields.PLAYER_FIELD_NEGSTAT2:
                            break;
                        case EUnitFields.PLAYER_FIELD_NEGSTAT3:
                            break;
                        case EUnitFields.PLAYER_FIELD_NEGSTAT4:
                            break;
                        case EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE:
                            break;
                        case EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE:
                            break;
                        case EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS:
                            break;
                        case EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG:
                            break;
                        case EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT:
                            break;
                        case EUnitFields.PLAYER_FIELD_BYTES:
                            break;
                        case EUnitFields.PLAYER_AMMO_ID:
                            break;
                        case EUnitFields.PLAYER_SELF_RES_SPELL:
                            break;
                        case EUnitFields.PLAYER_FIELD_PVP_MEDALS:
                            break;
                        case EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_LAST:
                            break;
                        case EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1:
                            break;
                        case EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST:
                            break;
                        case EUnitFields.PLAYER_FIELD_SESSION_KILLS:
                            break;
                        case EUnitFields.PLAYER_FIELD_YESTERDAY_KILLS:
                            break;
                        case EUnitFields.PLAYER_FIELD_LAST_WEEK_KILLS:
                            break;
                        case EUnitFields.PLAYER_FIELD_THIS_WEEK_KILLS:
                            break;
                        case EUnitFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION:
                            break;
                        case EUnitFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS:
                            break;
                        case EUnitFields.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS:
                            break;
                        case EUnitFields.PLAYER_FIELD_YESTERDAY_CONTRIBUTION:
                            break;
                        case EUnitFields.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION:
                            break;
                        case EUnitFields.PLAYER_FIELD_LAST_WEEK_RANK:
                            break;
                        case EUnitFields.PLAYER_FIELD_BYTES2:
                            break;
                        case EUnitFields.PLAYER_FIELD_WATCHED_FACTION_INDEX:
                            break;
                        case EUnitFields.PLAYER_FIELD_COMBAT_RATING_1:
                            break;
                    }

                }
            }
        }
        private static void ParseGameObjectFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            for (int i = (int)EGameObjectFields.OBJECT_FIELD_CREATED_BY; i < (int)EGameObjectFields.GAMEOBJECT_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((EGameObjectFields)i)
                    {
                        case EGameObjectFields.OBJECT_FIELD_CREATED_BY:
                            var createdByGuid = reader.ReadUInt64();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{createdByGuid}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_DISPLAYID:
                            var displayId = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{displayId}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_FLAGS:
                            var flags = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{flags}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_ROTATION:
                            var rotation = reader.ReadSingle();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{rotation}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_STATE:
                            var state = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{state}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_POS_X:
                            var posX = reader.ReadSingle();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{posX}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_POS_Y:
                            var posY = reader.ReadSingle();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{posY}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_POS_Z:
                            var posZ = reader.ReadSingle();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{posZ}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_FACING:
                            var facing = reader.ReadSingle();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{facing}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_DYN_FLAGS:
                            var dynFlags = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{dynFlags}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_FACTION:
                            var faction = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{faction}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_TYPE_ID:
                            var typeId = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{typeId}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_LEVEL:
                            var level = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{level}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_ARTKIT:
                            var artKit = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{artKit}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_ANIMPROGRESS:
                            var animProgress = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{animProgress}");
                            break;
                        case EGameObjectFields.GAMEOBJECT_PADDING:
                            var padding = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EGameObjectFields)i}]{BitConverter.ToString(padding)}");
                            break;
                    }
                }
            }
        }
        private static void ParseDynamicObjectFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            for (int i = (int)EDynamicObjectFields.DYNAMICOBJECT_CASTER; i < (int)EDynamicObjectFields.DYNAMICOBJECT_PAD; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((EDynamicObjectFields)i)
                    {
                        case EDynamicObjectFields.DYNAMICOBJECT_CASTER:
                            var casterGuid = reader.ReadUInt64();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{casterGuid}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_BYTES:
                            var bytes = reader.ReadBytes(4);
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{BitConverter.ToString(bytes)}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_SPELLID:
                            var spellId = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{spellId}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_RADIUS:
                            var radius = reader.ReadSingle();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{radius}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_POS_X:
                            var posX = reader.ReadSingle();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{posX}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_POS_Y:
                            var posY = reader.ReadSingle();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{posY}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_POS_Z:
                            var posZ = reader.ReadSingle();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{posZ}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_FACING:
                            var facing = reader.ReadSingle();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{facing}");
                            break;
                        case EDynamicObjectFields.DYNAMICOBJECT_PAD:
                            var padding = reader.ReadUInt32();
                            Console.WriteLine($"--[{(EDynamicObjectFields)i}]{padding}");
                            break;
                    }
                }
            }
        }
        private static void ParseCorpseFields(BinaryReader reader, ulong guid, uint[] maskBlocks)
        {
            for (int i = (int)ECorpseFields.CORPSE_FIELD_OWNER; i < (int)ECorpseFields.CORPSE_END; i++)
            {
                bool maskMatch = (maskBlocks[i / 32] & (1 << (i % 32))) != 0;

                if (maskMatch)
                {
                    switch ((ECorpseFields)i)
                    {
                        case ECorpseFields.CORPSE_FIELD_OWNER:
                            break;
                        case ECorpseFields.CORPSE_FIELD_FACING:
                            break;
                        case ECorpseFields.CORPSE_FIELD_POS_X:
                            break;
                        case ECorpseFields.CORPSE_FIELD_POS_Y:
                            break;
                        case ECorpseFields.CORPSE_FIELD_POS_Z:
                            break;
                        case ECorpseFields.CORPSE_FIELD_DISPLAY_ID:
                            break;
                        case ECorpseFields.CORPSE_FIELD_ITEM:
                            break;
                        case ECorpseFields.CORPSE_FIELD_BYTES_1:
                            break;
                        case ECorpseFields.CORPSE_FIELD_BYTES_2:
                            break;
                        case ECorpseFields.CORPSE_FIELD_GUILD:
                            break;
                        case ECorpseFields.CORPSE_FIELD_FLAGS:
                            break;
                        case ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS:
                            break;
                        case ECorpseFields.CORPSE_FIELD_PAD:
                            break;
                        case ECorpseFields.CORPSE_END:
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