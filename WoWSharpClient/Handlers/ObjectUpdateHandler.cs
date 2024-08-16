using BotRunner.Base;
using BotRunner.Constants;
using BotRunner.Interfaces;
using BotRunner.Models;
using Org.BouncyCastle.Utilities;
using PathfindingService.Models;
using System.Collections;
using WoWSharpClient.Manager;
using WoWSharpClient.Models;
using WoWSharpClient.Utils;
using static WoWSharpClient.Models.UpdateFields;

namespace WoWSharpClient.Handlers
{
    public class ObjectUpdateHandler(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;
        public void HandleUpdateObject(Opcodes opcode, byte[] data)
        {
            if (opcode == Opcodes.SMSG_COMPRESSED_UPDATE_OBJECT)
                data = PacketManager.Decompress(data.Skip(4).ToArray());

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var objectCount = reader.ReadUInt32();
            var hasTransport = reader.ReadByte();

            for (int i = 0; i < objectCount; i++)
                ParseNextUpdate(reader);
        }

        private void ParseNextUpdate(BinaryReader reader)
        {
            var updateType = (ObjectUpdateType)reader.ReadByte();
            switch (updateType)
            {
                case ObjectUpdateType.CREATE_OBJECT:
                case ObjectUpdateType.CREATE_OBJECT2:

                    ParseCreateObject(reader);
                    break;
                case ObjectUpdateType.MOVEMENT:
                    ParseMovementUpdate(reader);
                    break;
                case ObjectUpdateType.PARTIAL:
                    ParsePartialUpdate(reader);
                    break;
                case ObjectUpdateType.OUT_OF_RANGE_OBJECTS:
                    ParseOutOfRangeObjects(reader);
                    break;
                default:
                    Console.WriteLine($"Unhandled update type: {updateType}");
                    return;
            }
        }

        private void ParseCreateObject(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);

            byte[] guidBytes = BitConverter.GetBytes(guid);
            var objectType = (WoWObjectType)reader.ReadByte();
            ObjectUpdateFlags updateFlags = (ObjectUpdateFlags)reader.ReadByte();

            Models.Object woWObject = CreateWoWObject(objectType, guidBytes);

            if (woWObject != null)
            {
                _objectManager.Objects.Add(woWObject);
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_LIVING) != 0)
            {
                ParseUnitMovementInfo(reader, (Unit)woWObject);
            }
            else if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_HAS_POSITION) != 0)
            {
                ParseObjectPositionInfo(reader, woWObject);
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_HIGHGUID) != 0)
            {
                reader.ReadUInt32();
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_ALL) != 0)
            {
                reader.ReadInt32();
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_FULLGUID) != 0)
            {
                ulong victimGuid = ReaderUtils.ReadPackedGuid(reader);
                byte[] targetGuidByteArray = BitConverter.GetBytes(victimGuid);
                ((Unit)woWObject).TargetHighGuid = new HighGuid(targetGuidByteArray.Take(4).ToArray(), targetGuidByteArray.Skip(4).Take(4).ToArray());
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_TRANSPORT) != 0)
            {
                woWObject.Timestamp = reader.ReadUInt32();
            }

            if ((updateFlags & ObjectUpdateFlags.UPDATEFLAG_SELF) != 0)
            {
                _objectManager.PlayerGuid.LowGuidValue = woWObject.HighGuid.LowGuidValue;
                _objectManager.PlayerGuid.HighGuidValue = woWObject.HighGuid.HighGuidValue;
            }

            ParseUpdateBlock(reader, woWObject);

            if (woWObject.ObjectType != WoWObjectType.Player)
                _woWSharpEventEmitter.FireOnGameObjectCreated(new GameObjectCreatedArgs(guid, woWObject.ObjectType));
        }
        private void ParseMovementUpdate(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            ParseUnitMovementInfo(reader, _objectManager.Units.FirstOrDefault(x => x.Guid == guid) as Unit);
        }
        private void ParsePartialUpdate(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            ParseUpdateBlock(reader, _objectManager.Objects.First(x => x.Guid == guid) as Models.Object);
        }
        private void ParseOutOfRangeObjects(BinaryReader reader)
        {
            uint count = reader.ReadUInt32();
            for (int j = 0; j < count; j++)
            {
                var outOfRangeGuid = ReaderUtils.ReadPackedGuid(reader);
                _objectManager.Objects.Remove(_objectManager.Objects.First(x => x.Guid == outOfRangeGuid));
            }
        }
        private static Models.Object CreateWoWObject(WoWObjectType objectType, byte[] guidBytes)
        {
            Models.Object wowObject = objectType switch
            {
                WoWObjectType.None => new Models.Object(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.Item => new Item(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.Container => new Container(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.Unit => new Unit(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.Player => new Player(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.GameObj => new GameObject(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.DynamicObj => new DynamicObject(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                WoWObjectType.Corpse => new Corpse(new HighGuid(guidBytes.Take(4).ToArray(), guidBytes.Skip(4).ToArray())),
                _ => throw new NotImplementedException()
            };

            return wowObject;
        }

        private static void ParseObjectPositionInfo(BinaryReader reader, Models.Object currentObject)
        {
            if (currentObject != null)
            {
                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                float posZ = reader.ReadSingle();

                currentObject.Position = new Position(posX, posY, posZ);
                currentObject.Facing = reader.ReadSingle();
            }
        }

        private static void ParseUnitMovementInfo(BinaryReader reader, Unit currentUnit)
        {
            if (currentUnit != null)
            {
                currentUnit.MovementFlags = (MovementFlags)reader.ReadUInt32();
                currentUnit.LastUpdated = reader.ReadUInt32();

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

                currentUnit.FallTime = fallTime;
                currentUnit.WalkSpeed = walkSpeed;
                currentUnit.RunSpeed = runSpeed;
                currentUnit.RunBackSpeed = runBackSpeed;
                currentUnit.SwimSpeed = swimSpeed;
                currentUnit.SwimBackSpeed = swimBackSpeed;
                currentUnit.TurnRate = turnRate;
            }
        }

        private static void ParseUpdateBlock(BinaryReader reader, Models.Object @object)
        {
            // Read the block count (the first byte)
            byte blockCount = reader.ReadByte();

            // Parse the update mask
            byte[] updateMask = reader.ReadBytes(blockCount * 4);
            BitArray updateMaskBits = new(updateMask);

            for (int i = 0; i < updateMaskBits.Length; i++)
            {
                if (i < (int)EObjectFields.OBJECT_END)
                {
                    if (updateMaskBits[i])
                        ReadObjectField(reader, @object, (EObjectFields)i);
                }
                else if (@object.ObjectType == WoWObjectType.Unit || @object.ObjectType == WoWObjectType.Player)
                {
                    if (i < (int)EUnitFields.UNIT_END)
                    {
                        if (updateMaskBits[i])
                            ReadUnitField(reader, (Unit)@object, (EUnitFields)i);
                    }
                    else if (@object.ObjectType == WoWObjectType.Player && updateMaskBits[i])
                        ReadPlayerField(reader, (Player)@object, (EUnitFields)i);
                }
                else if (@object.ObjectType == WoWObjectType.Item || @object.ObjectType == WoWObjectType.Container)
                {
                    if (i < (int)EItemFields.ITEM_END)
                    {
                        if (updateMaskBits[i])
                            ReadItemField(reader, (Item)@object, (EItemFields)i);
                    }
                    else if (@object.ObjectType == WoWObjectType.Container && updateMaskBits[i])
                        ReadContainerField(reader, (Container)@object, (EContainerFields)i);
                }
                else if (@object.ObjectType == WoWObjectType.GameObj)
                {
                    if (updateMaskBits[i])
                        ReadGameObjectField(reader, (GameObject)@object, (EGameObjectFields)i);
                }
                else if (@object.ObjectType == WoWObjectType.DynamicObj)
                {
                    if (updateMaskBits[i])
                        ReadDynamicObjectField(reader, (DynamicObject)@object, (EDynamicObjectFields)i);
                }
                else if (@object.ObjectType == WoWObjectType.Corpse)
                {
                    if (updateMaskBits[i])
                        ReadCorpseField(reader, (Corpse)@object, (ECorpseFields)i);
                }
            }
        }

        private static void ReadObjectField(BinaryReader reader, Models.Object @object, EObjectFields field)
        {
            if (field <= EObjectFields.OBJECT_FIELD_GUID + 0x01)
            {
                if (field == EObjectFields.OBJECT_FIELD_GUID)
                    @object.HighGuid.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.HighGuid.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EObjectFields.OBJECT_FIELD_TYPE)
                reader.ReadUInt32();
            else if (field == EObjectFields.OBJECT_FIELD_ENTRY)
                @object.Entry = reader.ReadUInt32();
            else if (field == EObjectFields.OBJECT_FIELD_SCALE_X)
                @object.ScaleX = reader.ReadSingle();
            else if (field == EObjectFields.OBJECT_FIELD_PADDING)
                reader.ReadUInt32();
        }
        private static void ReadItemField(BinaryReader reader, Item item, EItemFields field)
        {
            if (field <= EItemFields.ITEM_FIELD_OWNER + 0x01)
            {
                if (field == EItemFields.ITEM_FIELD_OWNER)
                    item.Owner.LowGuidValue = reader.ReadBytes(4);
                else
                    item.Owner.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EItemFields.ITEM_FIELD_CONTAINED + 0x01)
            {
                if (field == EItemFields.ITEM_FIELD_CONTAINED)
                    item.Contained.LowGuidValue = reader.ReadBytes(4);
                else
                    item.Contained.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EItemFields.ITEM_FIELD_CREATOR + 0x01)
            {
                if (field == EItemFields.ITEM_FIELD_CREATOR)
                    item.CreatedBy.LowGuidValue = reader.ReadBytes(4);
                else
                    item.CreatedBy.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EItemFields.ITEM_FIELD_GIFTCREATOR + 0x01)
            {
                if (field == EItemFields.ITEM_FIELD_GIFTCREATOR)
                    item.GiftCreator.LowGuidValue = reader.ReadBytes(4);
                else
                    item.GiftCreator.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EItemFields.ITEM_FIELD_STACK_COUNT)
                item.StackCount = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_DURATION)
                item.Duration = reader.ReadUInt32();
            else if (field <= EItemFields.ITEM_FIELD_SPELL_CHARGES_04)
                item.SpellCharges[field - EItemFields.ITEM_FIELD_SPELL_CHARGES] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_FLAGS)
                item.Flags = (ItemDynFlags)reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_ENCHANTMENT)
                item.Enchantments[field - EItemFields.ITEM_FIELD_ENCHANTMENT] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_PROPERTY_SEED)
                item.PropertySeed = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID)
                item.RandomPropertiesId = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_ITEM_TEXT_ID)
                item.ItemTextId = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_DURABILITY)
                item.Durability = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_MAXDURABILITY)
                item.MaxDurability = reader.ReadUInt32();
        }
        private static void ReadContainerField(BinaryReader reader, Container @object, EContainerFields field)
        {
            if (field == EContainerFields.CONTAINER_FIELD_NUM_SLOTS)
                @object.NumOfSlots = (int)reader.ReadUInt32();
            else if (field == EContainerFields.CONTAINER_ALIGN_PAD)
                reader.ReadUInt32();
            else if (field <= EContainerFields.CONTAINER_FIELD_SLOT_LAST)
                @object.Slots[field - EContainerFields.CONTAINER_FIELD_SLOT_1] = reader.ReadUInt32();
        }
        private static void ReadGameObjectField(BinaryReader reader, GameObject @object, EGameObjectFields field)
        {
            if (field <= EGameObjectFields.OBJECT_FIELD_CREATED_BY + 0x01)
            {
                if (field == EGameObjectFields.OBJECT_FIELD_CREATED_BY)
                    @object.CreatedBy.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.CreatedBy.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EGameObjectFields.GAMEOBJECT_DISPLAYID)
                @object.DisplayId = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_FLAGS)
                @object.Flags = reader.ReadUInt32();
            else if (field < EGameObjectFields.GAMEOBJECT_STATE)
                @object.Rotation[field - EGameObjectFields.GAMEOBJECT_ROTATION] = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_STATE)
                @object.GoState = (GOState)reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_POS_X)
                @object.Position.X = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_POS_Y)
                @object.Position.Y = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_POS_Z)
                @object.Position.Z = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_FACING)
                @object.Facing = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_DYN_FLAGS)
                @object.DynamicFlags = (DynamicFlags)reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_FACTION)
                @object.Faction = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_TYPE_ID)
                @object.TypeId = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_LEVEL)
                @object.Level = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_ARTKIT)
                @object.ArtKit = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_ANIMPROGRESS)
                @object.AnimProgress = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_PADDING)
                reader.ReadUInt32();
        }
        private static void ReadDynamicObjectField(BinaryReader reader, DynamicObject @object, EDynamicObjectFields field)
        {
            if (field <= EDynamicObjectFields.DYNAMICOBJECT_CASTER + 0x01)
            {
                if (field == EDynamicObjectFields.DYNAMICOBJECT_CASTER)
                    @object.Caster.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.Caster.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_BYTES)
                @object.Bytes = reader.ReadBytes(4);
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_SPELLID)
                @object.SpellId = reader.ReadUInt32();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_RADIUS)
                @object.Radius = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_POS_X)
                @object.Position.X = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_POS_Y)
                @object.Position.Y = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_POS_Z)
                @object.Position.Z = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_FACING)
                @object.Facing = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_PAD)
                reader.ReadUInt32();
        }
        private static void ReadCorpseField(BinaryReader reader, Corpse @object, ECorpseFields field)
        {
            if (field <= ECorpseFields.CORPSE_FIELD_OWNER + 0x01)
            {
                if (field == ECorpseFields.CORPSE_FIELD_OWNER)
                    @object.OwnerGuid.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.OwnerGuid.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == ECorpseFields.CORPSE_FIELD_FACING)
                @object.Facing = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_POS_X)
                @object.Position.X = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_POS_Y)
                @object.Position.Y = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_POS_Z)
                @object.Position.Z = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_DISPLAY_ID)
                @object.DisplayId = reader.ReadUInt32();
            else if (field < ECorpseFields.CORPSE_FIELD_BYTES_1)
                @object.Items[field - ECorpseFields.CORPSE_FIELD_ITEM] = reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_BYTES_1)
                @object.Bytes1 = reader.ReadBytes(4);
            else if (field == ECorpseFields.CORPSE_FIELD_BYTES_2)
                @object.Bytes2 = reader.ReadBytes(4);
            else if (field == ECorpseFields.CORPSE_FIELD_GUILD)
                @object.Guild = reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_FLAGS)
                @object.Flags = (CorpseFlags)reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS)
                @object.DynamicFlags = (DynamicFlags)reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_PAD)
                reader.ReadUInt32();
        }
        private static void ReadPlayerField(BinaryReader reader, Player @object, EUnitFields field)
        {
            if (field <= EUnitFields.PLAYER_DUEL_ARBITER + 0x01)
            {
                if (field == EUnitFields.PLAYER_DUEL_ARBITER)
                    @object.DuelArbiter.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.DuelArbiter.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EUnitFields.PLAYER_FLAGS)
                @object.PlayerFlags = (PlayerFlags)reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_GUILDID)
                @object.GuildId = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_GUILDRANK)
                @object.GuildRank = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_BYTES)
            {
                @object.Bytes[0] = reader.ReadByte();
                @object.Bytes[1] = reader.ReadByte();
                @object.Bytes[2] = reader.ReadByte();
                @object.Bytes[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.PLAYER_BYTES_2)
            {
                @object.Bytes2[0] = reader.ReadByte();
                @object.Bytes2[1] = reader.ReadByte();
                @object.Bytes2[2] = reader.ReadByte();
                @object.Bytes2[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.PLAYER_BYTES_3)
            {
                @object.Bytes3[0] = reader.ReadByte();
                @object.Bytes3[1] = reader.ReadByte();
                @object.Bytes3[2] = reader.ReadByte();
                @object.Bytes3[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.PLAYER_DUEL_TEAM)
                @object.GuildTimestamp = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_QUEST_LOG_LAST_3)
            {
                uint questField = (uint)(field - EUnitFields.PLAYER_QUEST_LOG_1_1) % 3;

                switch (questField)
                {
                    case 0:
                        @object.QuestLog[(field - EUnitFields.PLAYER_QUEST_LOG_1_1) / 3].QuestId = reader.ReadUInt32();
                        break;
                    case 1:
                        @object.QuestLog[(field - EUnitFields.PLAYER_QUEST_LOG_1_1) / 3].QuestCounters = reader.ReadBytes(4);
                        break;
                    case 2:
                        @object.QuestLog[(field - EUnitFields.PLAYER_QUEST_LOG_1_1) / 3].QuestState = reader.ReadUInt32();
                        break;
                }
            }
            else if (field <= EUnitFields.PLAYER_VISIBLE_ITEM_LAST_PAD)
            {
                uint visibleItemField = (uint)(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) % 12;

                switch (visibleItemField)
                {
                    case 0:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].CreatedBy.LowGuidValue = reader.ReadBytes(4);
                        break;
                    case 1:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].CreatedBy.HighGuidValue = reader.ReadBytes(4);
                        break;
                    case 2:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].ItemId = reader.ReadUInt32();
                        break;
                    case 3:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].Owner.LowGuidValue = reader.ReadBytes(4);
                        break;
                    case 4:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].Owner.HighGuidValue = reader.ReadBytes(4);
                        break;
                    case 5:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].Contained.LowGuidValue = reader.ReadBytes(4);
                        break;
                    case 6:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].Contained.HighGuidValue = reader.ReadBytes(4);
                        break;
                    case 7:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].GiftCreator.LowGuidValue = reader.ReadBytes(4);
                        break;
                    case 8:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].GiftCreator.HighGuidValue = reader.ReadBytes(4);
                        break;
                    case 9:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].StackCount = reader.ReadUInt32();
                        break;
                    case 10:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].Durability = reader.ReadUInt32();
                        break;
                    case 11:
                        @object.VisibleItems[(field - EUnitFields.PLAYER_VISIBLE_ITEM_1_CREATOR) / 12].PropertySeed = reader.ReadUInt32();
                        break;
                    case 12:
                        reader.ReadUInt32();
                        break;
                }
            }
            else if (field < EUnitFields.PLAYER_FIELD_PACK_SLOT_1)
                @object.Inventory[field - EUnitFields.PLAYER_FIELD_INV_SLOT_HEAD] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_PACK_SLOT_LAST)
                @object.PackSlots[field - EUnitFields.PLAYER_FIELD_PACK_SLOT_1] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_BANK_SLOT_LAST)
                @object.BankSlots[field - EUnitFields.PLAYER_FIELD_BANK_SLOT_1] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_LAST)
                @object.BankBagSlots[field - EUnitFields.PLAYER_FIELD_BANKBAG_SLOT_1] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST)
                @object.VendorBuybackSlots[field - EUnitFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_1] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_KEYRING_SLOT_LAST)
                @object.KeyringSlots[field - EUnitFields.PLAYER_FIELD_KEYRING_SLOT_1] = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FARSIGHT)
                @object.Farsight = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_COMBO_TARGET)
            {
                if (field == EUnitFields.PLAYER_FIELD_COMBO_TARGET)
                    @object.ComboTarget.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.ComboTarget.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EUnitFields.PLAYER_XP)
                @object.XP = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_NEXT_LEVEL_XP)
                @object.NextLevelXP = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_SKILL_INFO_1_1 + 384)
            {
                uint questField = (uint)(field - EUnitFields.PLAYER_SKILL_INFO_1_1) % 4;

                switch (questField)
                {
                    case 0:
                        @object.SkillInfo[(field - EUnitFields.PLAYER_SKILL_INFO_1_1) / 4].SkillInt1 = reader.ReadUInt32();
                        break;
                    case 1:
                        @object.SkillInfo[(field - EUnitFields.PLAYER_SKILL_INFO_1_1) / 4].SkillInt2 = reader.ReadUInt32();
                        break;
                    case 2:
                        @object.SkillInfo[(field - EUnitFields.PLAYER_SKILL_INFO_1_1) / 4].SkillInt3 = reader.ReadUInt32();
                        break;
                    case 3:
                        @object.SkillInfo[(field - EUnitFields.PLAYER_SKILL_INFO_1_1) / 4].SkillInt4 = reader.ReadUInt32();
                        break;
                }
            }
            else if (field == EUnitFields.PLAYER_CHARACTER_POINTS1)
                @object.CharacterPoints1 = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_CHARACTER_POINTS2)
                @object.CharacterPoints2 = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_TRACK_CREATURES)
                @object.TrackCreatures = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_TRACK_RESOURCES)
                @object.TrackResources = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_BLOCK_PERCENTAGE)
                @object.BlockPercentage = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_DODGE_PERCENTAGE)
                @object.DodgePercentage = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_PARRY_PERCENTAGE)
                @object.ParryPercentage = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_CRIT_PERCENTAGE)
                @object.CritPercentage = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_RANGED_CRIT_PERCENTAGE)
                @object.RangedCritPercentage = reader.ReadUInt32();
            else if (field < EUnitFields.PLAYER_REST_STATE_EXPERIENCE)
                @object.ExploredZones[field - EUnitFields.PLAYER_EXPLORED_ZONES_1] = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_REST_STATE_EXPERIENCE)
                @object.RestStateExperience = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_COINAGE)
                @object.Coinage = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_POSSTAT4)
                @object.StatBonusesPos[field - EUnitFields.PLAYER_FIELD_POSSTAT0] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_NEGSTAT4)
                @object.StatBonusesNeg[field - EUnitFields.PLAYER_FIELD_NEGSTAT0] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + 6)
                @object.ResistBonusesPos[field - EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSPOSITIVE] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + 6)
                @object.ResistBonusesNeg[field - EUnitFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + 6)
                @object.ModDamageDonePos[field - EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + 6)
                @object.ModDamageDoneNeg[field - EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + 6)
                @object.ModDamageDonePct[field - EUnitFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT] = reader.ReadSingle();
            else if (field == EUnitFields.PLAYER_FIELD_BYTES)
            {
                @object.Bytes[0] = reader.ReadByte();
                @object.Bytes[1] = reader.ReadByte();
                @object.Bytes[2] = reader.ReadByte();
                @object.Bytes[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.PLAYER_AMMO_ID)
                @object.AmmoId = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_SELF_RES_SPELL)
                @object.SelfResSpell = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_PVP_MEDALS)
                @object.PvpMedals = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_LAST)
                @object.BuybackPrices[field - EUnitFields.PLAYER_FIELD_BUYBACK_PRICE_1] = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST)
                @object.BuybackTimestamps[field - EUnitFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_1] = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_SESSION_KILLS)
                @object.SessionKills = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_YESTERDAY_KILLS)
                @object.YesterdayKills = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_LAST_WEEK_KILLS)
                @object.LastWeekKills = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_THIS_WEEK_KILLS)
                @object.ThisWeekKills = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_THIS_WEEK_CONTRIBUTION)
                @object.ThisWeekContribution = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS)
                @object.LifetimeHonorableKills = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS)
                @object.LifetimeDishonorableKills = reader.ReadUInt32();
            else if (field == EUnitFields.PLAYER_FIELD_BYTES2)
            {
                @object.Bytes2[0] = reader.ReadByte();
                @object.Bytes2[1] = reader.ReadByte();
                @object.Bytes2[2] = reader.ReadByte();
                @object.Bytes2[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.PLAYER_FIELD_WATCHED_FACTION_INDEX)
                @object.WatchedFactionIndex = reader.ReadUInt32();
            else if (field <= EUnitFields.PLAYER_FIELD_COMBAT_RATING_1 + 20)
                @object.CombatRating[field - EUnitFields.PLAYER_FIELD_COMBAT_RATING_1] = reader.ReadUInt32();
        }
        private static void ReadUnitField(BinaryReader reader, Unit @object, EUnitFields field)
        {
            if (field <= EUnitFields.UNIT_FIELD_CHARM + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_CHARM)
                    @object.Charm.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.Charm.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_SUMMON + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_SUMMON)
                    @object.Summon.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.Summon.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_CHARMEDBY + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_CHARMEDBY)
                    @object.CharmedBy.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.CharmedBy.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_SUMMONEDBY + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_SUMMONEDBY)
                    @object.SummonedBy.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.SummonedBy.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_CREATEDBY + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_CREATEDBY)
                    @object.CreatedBy.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.CreatedBy.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_TARGET + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_TARGET)
                    @object.TargetHighGuid.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.TargetHighGuid.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_PERSUADED + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_PERSUADED)
                    @object.Persuaded.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.Persuaded.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field <= EUnitFields.UNIT_FIELD_CHANNEL_OBJECT + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_CHANNEL_OBJECT)
                    @object.ChannelObject.LowGuidValue = reader.ReadBytes(4);
                else
                    @object.ChannelObject.HighGuidValue = reader.ReadBytes(4);
            }
            else if (field == EUnitFields.UNIT_FIELD_HEALTH)
                @object.Health = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER1)
                @object.Powers[Powers.MANA] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER2)
                @object.Powers[Powers.RAGE] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER3)
                @object.Powers[Powers.FOCUS] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER4)
                @object.Powers[Powers.ENERGY] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER5)
                @object.Powers[Powers.HAPPINESS] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXHEALTH)
                @object.MaxHealth = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER1)
                @object.MaxPowers[Powers.MANA] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER2)
                @object.MaxPowers[Powers.RAGE] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER3)
                @object.MaxPowers[Powers.FOCUS] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER4)
                @object.MaxPowers[Powers.ENERGY] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER5)
                @object.MaxPowers[Powers.HAPPINESS] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_LEVEL)
                @object.Level = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_FACTIONTEMPLATE)
                @object.FactionTemplate = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BYTES_0)
            {
                @object.Bytes0[0] = reader.ReadByte();
                @object.Bytes0[1] = reader.ReadByte();
                @object.Bytes0[2] = reader.ReadByte();
                @object.Bytes0[3] = reader.ReadByte();
            }
            else if (field <= EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02)
                @object.VirtualItemSlotDisplay[field - EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_VIRTUAL_ITEM_INFO_05)
                @object.VirtualItemInfo[field - EUnitFields.UNIT_VIRTUAL_ITEM_INFO] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_FLAGS)
                @object.UnitFlags = (UnitFlags)reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURA_LAST)
                @object.AuraFields[field - EUnitFields.UNIT_FIELD_AURA] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURAFLAGS_05)
                @object.AuraFlags[field - EUnitFields.UNIT_FIELD_AURAFLAGS] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURALEVELS_LAST)
                @object.AuraLevels[field - EUnitFields.UNIT_FIELD_AURALEVELS] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURAAPPLICATIONS_LAST)
                @object.AuraApplications[field - EUnitFields.UNIT_FIELD_AURAAPPLICATIONS] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURASTATE)
                @object.AuraState = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_BASEATTACKTIME + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_BASEATTACKTIME)
                    @object.BaseAttackTime = reader.ReadUInt32();
                else
                    @object.BaseAttackTime1 = reader.ReadUInt32();
            }
            else if (field <= EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME + 0x01)
            {
                if (field == EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME)
                    @object.OffhandAttackTime = reader.ReadUInt32();
                else
                    @object.OffhandAttackTime1 = reader.ReadUInt32();
            }
            else if (field == EUnitFields.UNIT_FIELD_BOUNDINGRADIUS)
                @object.BoundingRadius = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_COMBATREACH)
                @object.CombatReach = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_DISPLAYID)
                @object.DisplayId = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_NATIVEDISPLAYID)
                @object.NativeDisplayId = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MOUNTDISPLAYID)
                @object.MountDisplayId = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MINDAMAGE)
                @object.MinDamage = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXDAMAGE)
                @object.MaxDamage = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE)
                @object.MinOffhandDamage = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE)
                @object.MaxOffhandDamage = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BYTES_1)
            {
                @object.Bytes1[0] = reader.ReadByte();
                @object.Bytes1[1] = reader.ReadByte();
                @object.Bytes1[2] = reader.ReadByte();
                @object.Bytes1[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.UNIT_FIELD_PETNUMBER)
                @object.PetNumber = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PET_NAME_TIMESTAMP)
                @object.PetNameTimestamp = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PETEXPERIENCE)
                @object.PetExperience = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PETNEXTLEVELEXP)
                @object.PetNextLevelExperience = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_DYNAMIC_FLAGS)
                @object.DynamicFlags = (DynamicFlags)reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_CHANNEL_SPELL)
                @object.ChannelingId = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_MOD_CAST_SPEED)
                @object.ModCastSpeed = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_CREATED_BY_SPELL)
                @object.CreatedBySpell = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_NPC_FLAGS)
                @object.NpcFlags = (NPCFlags)reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_NPC_EMOTESTATE)
                @object.NpcEmoteState = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_TRAINING_POINTS)
                @object.TrainingPoints = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT0)
                @object.Strength = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT1)
                @object.Agility = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT2)
                @object.Stamina = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT3)
                @object.Intellect = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT4)
                @object.Spirit = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_RESISTANCES_06)
                @object.Resistances[field - EUnitFields.UNIT_FIELD_RESISTANCES] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BASE_MANA)
                @object.BaseMana = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BASE_HEALTH)
                @object.BaseHealth = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BYTES_2)
            {
                @object.Bytes2[0] = reader.ReadByte();
                @object.Bytes2[1] = reader.ReadByte();
                @object.Bytes2[2] = reader.ReadByte();
                @object.Bytes2[3] = reader.ReadByte();
            }
            else if (field == EUnitFields.UNIT_FIELD_ATTACK_POWER)
                @object.AttackPower = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS)
                @object.AttackPowerMods = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER)
                @object.AttackPowerMultipler = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER)
                @object.RangedAttackPower = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS)
                @object.RangedAttackPowerMods = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER)
                @object.RangedAttackPowerMultipler = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE)
                @object.MinRangedDamage = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE)
                @object.MaxRangedDamage = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER_06)
                @object.PowerCostModifers[field - EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_06)
                @object.PowerCostMultipliers[field - EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PADDING)
                reader.ReadUInt32();
        }
    }
}
