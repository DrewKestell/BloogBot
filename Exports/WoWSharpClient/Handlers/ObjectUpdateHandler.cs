using System.Collections;
using GameData.Core.Enums;
using WoWSharpClient.Client;
using WoWSharpClient.Models;
using WoWSharpClient.Parsers;
using WoWSharpClient.Utils;
using static GameData.Core.Enums.UpdateFields;

namespace WoWSharpClient.Handlers
{
    public static class ObjectUpdateHandler
    {
        public static void HandleUpdateObject(Opcode opcode, byte[] data)
        {
            if (opcode == Opcode.SMSG_COMPRESSED_UPDATE_OBJECT)
                data = PacketManager.Decompress([.. data.Skip(4)]);

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var objectCount = reader.ReadUInt32();
            var hasTransport = reader.ReadBoolean();

            for (int i = 0; i < objectCount; i++)
                ParseNextUpdate(reader);
        }

        private static void ParseNextUpdate(BinaryReader reader)
        {
            var updateType = (ObjectUpdateType)reader.ReadByte();
            try
            {
                switch (updateType)
                {
                    case ObjectUpdateType.CREATE_OBJECT:
                    case ObjectUpdateType.CREATE_OBJECT2:
                        ParseCreateObjectBlock(reader);
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
                        throw new Exception($"Unhandled update type: {updateType}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{updateType}] {ex}");
            }
        }

        private static void ParseCreateObjectBlock(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var objectType = (WoWObjectType)reader.ReadByte();
            MovementInfoUpdate movementUpdateData = ParseMovementInfo(reader);
            var update = new WoWSharpObjectManager.ObjectStateUpdate(
                guid,
                WoWSharpObjectManager.ObjectUpdateOperation.Add,
                objectType,
                movementUpdateData,
                []
            );

            ReadValuesUpdateBlock(reader, update);

            WoWSharpObjectManager.Instance.QueueUpdate(update);
        }

        private static MovementInfoUpdate ParseMovementInfo(BinaryReader reader)
        {
            ObjectUpdateFlags updateFlags = (ObjectUpdateFlags)reader.ReadByte();
            MovementInfoUpdate movementUpdateData = new();

            if (updateFlags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_LIVING))
                movementUpdateData = MovementPacketHandler.ParseMovementBlock(reader);
            else if (updateFlags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_HAS_POSITION))
                movementUpdateData = ParseObjectPositionInfo(reader);

            if (updateFlags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_HIGHGUID))
                movementUpdateData.HighGuid = reader.ReadUInt32();
            if (updateFlags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_ALL))
                movementUpdateData.UpdateAll = reader.ReadUInt32();
            if (updateFlags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_FULLGUID))
                movementUpdateData.TargetGuid = ReaderUtils.ReadPackedGuid(reader);
            if (updateFlags.HasFlag(ObjectUpdateFlags.UPDATEFLAG_TRANSPORT))
                reader.ReadUInt32();

            return movementUpdateData;
        }

        private static MovementInfoUpdate ParseObjectPositionInfo(BinaryReader reader)
        {
            var data = new MovementInfoUpdate
            {
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Z = reader.ReadSingle(),

                Facing = reader.ReadSingle(),
            };

            return data;
        }

        private static void ParseMovementUpdate(BinaryReader reader)
        {
            var guid = ReaderUtils.ReadPackedGuid(reader);
            var movementUpdateData = MovementPacketHandler.ParseMovementInfo(reader);
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate = new(
                guid,
                WoWSharpObjectManager.ObjectUpdateOperation.Update,
                WoWObjectType.None,
                movementUpdateData,
                []
            );

            WoWSharpObjectManager.Instance.QueueUpdate(objectUpdate);
        }

        private static void ParsePartialUpdate(BinaryReader reader)
        {
            ulong guid = ReaderUtils.ReadPackedGuid(reader);

            var update = new WoWSharpObjectManager.ObjectStateUpdate(
                guid,
                WoWSharpObjectManager.ObjectUpdateOperation.Update,
                WoWObjectType.None,
                null,
                []
            );

            ReadValuesUpdateBlock(reader, update);

            WoWSharpObjectManager.Instance.QueueUpdate(update);
        }

        private static void ParseOutOfRangeObjects(BinaryReader reader)
        {
            uint count = reader.ReadUInt32();
            for (int j = 0; j < count; j++)
            {
                var guid = ReaderUtils.ReadPackedGuid(reader);
                WoWSharpObjectManager.Instance.QueueUpdate(
                    new(
                        guid,
                        WoWSharpObjectManager.ObjectUpdateOperation.Remove,
                        WoWObjectType.None,
                        null,
                        []
                    )
                );
            }
        }

        private static void ReadValuesUpdateBlock(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate
        )
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
                        ReadObjectField(reader, objectUpdate, (EObjectFields)i);
                }
                else if (
                    objectUpdate.ObjectType == WoWObjectType.Unit
                    || objectUpdate.ObjectType == WoWObjectType.Player
                )
                {
                    if (i < (int)EUnitFields.UNIT_END)
                    {
                        if (updateMaskBits[i])
                            ReadUnitField(reader, objectUpdate, (EUnitFields)i);
                    }
                    else if (objectUpdate.ObjectType == WoWObjectType.Player && updateMaskBits[i])
                        ReadPlayerField(reader, objectUpdate, (EPlayerFields)i);
                }
                else if (
                    objectUpdate.ObjectType == WoWObjectType.Item
                    || objectUpdate.ObjectType == WoWObjectType.Container
                )
                {
                    if (i < (int)EItemFields.ITEM_END)
                    {
                        if (updateMaskBits[i])
                            ReadItemField(reader, objectUpdate, (EItemFields)i);
                    }
                    else if (
                        objectUpdate.ObjectType == WoWObjectType.Container
                        && updateMaskBits[i]
                    )
                        ReadContainerField(reader, objectUpdate, (EContainerFields)i);
                }
                else if (objectUpdate.ObjectType == WoWObjectType.GameObj)
                {
                    if (updateMaskBits[i])
                        ReadGameObjectField(reader, objectUpdate, (EGameObjectFields)i);
                }
                else if (objectUpdate.ObjectType == WoWObjectType.DynamicObj)
                {
                    if (updateMaskBits[i])
                        ReadDynamicObjectField(reader, objectUpdate, (EDynamicObjectFields)i);
                }
                else if (objectUpdate.ObjectType == WoWObjectType.Corpse)
                {
                    if (updateMaskBits[i])
                        ReadCorpseField(reader, objectUpdate, (ECorpseFields)i);
                }
            }
        }

        private static void ReadObjectField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EObjectFields field
        )
        {
            if (field <= EObjectFields.OBJECT_FIELD_GUID + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EObjectFields.OBJECT_FIELD_TYPE)
                objectUpdate.UpdatedFields[(uint)field] = (WoWObjectType)reader.ReadUInt32();
            else if (field == EObjectFields.OBJECT_FIELD_ENTRY)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EObjectFields.OBJECT_FIELD_SCALE_X)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EObjectFields.OBJECT_FIELD_PADDING)
                reader.ReadUInt32();
        }

        private static void ReadGameObjectField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EGameObjectFields field
        )
        {
            if (field <= EGameObjectFields.OBJECT_FIELD_CREATED_BY + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EGameObjectFields.GAMEOBJECT_DISPLAYID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field < EGameObjectFields.GAMEOBJECT_STATE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_STATE)
                objectUpdate.UpdatedFields[(uint)field] = (GOState)reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_POS_X)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_POS_Y)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_POS_Z)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_FACING)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EGameObjectFields.GAMEOBJECT_DYN_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (DynamicFlags)reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_FACTION)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_TYPE_ID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_LEVEL)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_ARTKIT)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_ANIMPROGRESS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EGameObjectFields.GAMEOBJECT_PADDING)
                reader.ReadUInt32();
        }

        private static void ReadDynamicObjectField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EDynamicObjectFields field
        )
        {
            if (field <= EDynamicObjectFields.DYNAMICOBJECT_CASTER + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_BYTES)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_SPELLID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_RADIUS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_POS_X)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_POS_Y)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_POS_Z)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_FACING)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EDynamicObjectFields.DYNAMICOBJECT_PAD)
                reader.ReadUInt32();
        }

        private static void ReadCorpseField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            ECorpseFields field
        )
        {
            if (field <= ECorpseFields.CORPSE_FIELD_OWNER + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == ECorpseFields.CORPSE_FIELD_FACING)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_POS_X)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_POS_Y)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_POS_Z)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == ECorpseFields.CORPSE_FIELD_DISPLAY_ID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field < ECorpseFields.CORPSE_FIELD_BYTES_1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_BYTES_1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == ECorpseFields.CORPSE_FIELD_BYTES_2)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == ECorpseFields.CORPSE_FIELD_GUILD)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (CorpseFlags)reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_DYNAMIC_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (DynamicFlags)reader.ReadUInt32();
            else if (field == ECorpseFields.CORPSE_FIELD_PAD)
                reader.ReadUInt32();
        }

        private static void ReadUnitField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EUnitFields field
        )
        {
            if (field <= EUnitFields.UNIT_FIELD_CHARM + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_SUMMON + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_CHARMEDBY + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_SUMMONEDBY + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_CREATEDBY + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_TARGET + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_PERSUADED + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EUnitFields.UNIT_FIELD_CHANNEL_OBJECT + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EUnitFields.UNIT_FIELD_HEALTH)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER2)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER3)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER4)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_POWER5)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXHEALTH)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER2)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER3)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER4)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXPOWER5)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_LEVEL)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_FACTIONTEMPLATE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BYTES_0)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field <= EUnitFields.UNIT_VIRTUAL_ITEM_SLOT_DISPLAY_02)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_VIRTUAL_ITEM_INFO_05)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (UnitFlags)reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURA_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURAFLAGS_05)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURALEVELS_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_AURAAPPLICATIONS_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_AURASTATE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BASEATTACKTIME)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_OFFHANDATTACKTIME)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_RANGEDATTACKTIME)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_BOUNDINGRADIUS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_COMBATREACH)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_FIELD_DISPLAYID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_NATIVEDISPLAYID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MOUNTDISPLAYID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MINDAMAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXDAMAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MINOFFHANDDAMAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXOFFHANDDAMAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BYTES_1)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EUnitFields.UNIT_FIELD_PETNUMBER)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PET_NAME_TIMESTAMP)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PETEXPERIENCE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PETNEXTLEVELEXP)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_DYNAMIC_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (DynamicFlags)reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_CHANNEL_SPELL)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_MOD_CAST_SPEED)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EUnitFields.UNIT_CREATED_BY_SPELL)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_NPC_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (NPCFlags)reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_NPC_EMOTESTATE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_TRAINING_POINTS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT0)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT2)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT3)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_STAT4)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_RESISTANCES_06)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BASE_MANA)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BASE_HEALTH)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_BYTES_2)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EUnitFields.UNIT_FIELD_ATTACK_POWER)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_ATTACK_POWER_MODS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_ATTACK_POWER_MULTIPLIER)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MODS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_RANGED_ATTACK_POWER_MULTIPLIER)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MINRANGEDDAMAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_MAXRANGEDDAMAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_POWER_COST_MODIFIER_06)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EUnitFields.UNIT_FIELD_POWER_COST_MULTIPLIER_06)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EUnitFields.UNIT_FIELD_PADDING)
                reader.ReadUInt32();
        }

        private static void ReadPlayerField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EPlayerFields field
        )
        {
            if (field <= EPlayerFields.PLAYER_DUEL_ARBITER + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EPlayerFields.PLAYER_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (PlayerFlags)reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_GUILDID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_GUILDRANK)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_BYTES)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EPlayerFields.PLAYER_BYTES_2)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EPlayerFields.PLAYER_BYTES_3)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EPlayerFields.PLAYER_DUEL_TEAM)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_QUEST_LOG_LAST_3)
            {
                uint questField = (field - EPlayerFields.PLAYER_QUEST_LOG_1_1) % 3;

                switch (questField)
                {
                    case 0:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
                        break;
                    case 1:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 2:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
                        break;
                }
            }
            else if (field <= EPlayerFields.PLAYER_VISIBLE_ITEM_19_PAD)
            {
                uint visibleItemField = (field - EPlayerFields.PLAYER_VISIBLE_ITEM_1_CREATOR) % 12;

                switch (visibleItemField)
                {
                    case 0:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 1:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 2:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
                        break;
                    case 3:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 4:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 5:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 6:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 7:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 8:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
                        break;
                    case 9:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
                        break;
                    case 10:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
                        break;
                    case 11:
                        objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
                        break;
                }
            }
            else if (field < EPlayerFields.PLAYER_FIELD_PACK_SLOT_1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_PACK_SLOT_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_BANK_SLOT_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_BANKBAG_SLOT_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_VENDORBUYBACK_SLOT_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_KEYRING_SLOT_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FARSIGHT)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_COMBO_TARGET)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EPlayerFields.PLAYER_XP)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_NEXT_LEVEL_XP)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_SKILL_INFO_1_1 + 384)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_CHARACTER_POINTS1)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_CHARACTER_POINTS2)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_TRACK_CREATURES)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_TRACK_RESOURCES)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_BLOCK_PERCENTAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_DODGE_PERCENTAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_PARRY_PERCENTAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_CRIT_PERCENTAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_RANGED_CRIT_PERCENTAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field < EPlayerFields.PLAYER_REST_STATE_EXPERIENCE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_REST_STATE_EXPERIENCE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_COINAGE)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_POSSTAT4)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_NEGSTAT4)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_RESISTANCEBUFFMODSNEGATIVE + 6)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + 6)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_POS + 6)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_NEG + 6)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_MOD_DAMAGE_DONE_PCT + 6)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadSingle();
            else if (field == EPlayerFields.PLAYER_FIELD_BYTES)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EPlayerFields.PLAYER_AMMO_ID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_SELF_RES_SPELL)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_PVP_MEDALS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_BUYBACK_PRICE_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_BUYBACK_TIMESTAMP_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_KILLS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_YESTERDAY_KILLS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_LAST_WEEK_KILLS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_LAST_WEEK_KILLS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_LAST_WEEK_CONTRIBUTION)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_LIFETIME_HONORABLE_KILLS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_LIFETIME_DISHONORABLE_KILLS)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EPlayerFields.PLAYER_FIELD_BYTES2)
                objectUpdate.UpdatedFields[(uint)field] = new byte[]
                {
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                    reader.ReadByte(),
                };
            else if (field == EPlayerFields.PLAYER_FIELD_WATCHED_FACTION_INDEX)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EPlayerFields.PLAYER_FIELD_COMBAT_RATING_1 + 20)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
        }

        private static void ReadItemField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EItemFields field
        )
        {
            Console.WriteLine($"[ReadItemField] {field}");
            if (field <= EItemFields.ITEM_FIELD_OWNER + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EItemFields.ITEM_FIELD_CONTAINED + 0x01)
            {
                var bytes = reader.ReadBytes(4);
                objectUpdate.UpdatedFields[(uint)field] = bytes;
            }
            else if (field <= EItemFields.ITEM_FIELD_CREATOR + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field <= EItemFields.ITEM_FIELD_GIFTCREATOR + 0x01)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadBytes(4);
            else if (field == EItemFields.ITEM_FIELD_STACK_COUNT)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_DURATION)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field <= EItemFields.ITEM_FIELD_SPELL_CHARGES_04)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_FLAGS)
                objectUpdate.UpdatedFields[(uint)field] = (ItemDynFlags)reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_ENCHANTMENT)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_PROPERTY_SEED)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_RANDOM_PROPERTIES_ID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_ITEM_TEXT_ID)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_DURABILITY)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
            else if (field == EItemFields.ITEM_FIELD_MAXDURABILITY)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
        }

        private static void ReadContainerField(
            BinaryReader reader,
            WoWSharpObjectManager.ObjectStateUpdate objectUpdate,
            EContainerFields field
        )
        {
            if (field == EContainerFields.CONTAINER_FIELD_NUM_SLOTS)
                objectUpdate.UpdatedFields[(uint)field] = (int)reader.ReadUInt32();
            else if (field == EContainerFields.CONTAINER_ALIGN_PAD)
                reader.ReadUInt32();
            else if (field <= EContainerFields.CONTAINER_FIELD_SLOT_LAST)
                objectUpdate.UpdatedFields[(uint)field] = reader.ReadUInt32();
        }
    }
}
