using GameData.Core.Enums;
using Moq;
using WoWSharpClient.Client;
using WoWSharpClient.Handlers;
using WoWSharpClient.Models;
using WoWSharpClient.Tests.Util;

namespace WoWSharpClient.Tests.Handlers
{
    [Collection("Sequential ObjectManager tests")]
    public class SMSG_COMPRESSED_UPDATE_OBJECT_Transports_Tests(ObjectManagerFixture _) : IClassFixture<ObjectManagerFixture>
    {
        [Fact]
        public void ShouldDecompressAndParseTransports()
        {
            var opcode = Opcode.SMSG_COMPRESSED_UPDATE_OBJECT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\20240815_1.bin");

            // Call the HandleUpdateObject method on ObjectUpdateHandler
            ObjectUpdateHandler.HandleUpdateObject(opcode, data);

            WoWSharpObjectManager.Instance.ProcessUpdates();
            // Verify that objects with the expected GUIDs were added to the ObjectManager
            Assert.Equal(6, WoWSharpObjectManager.Instance.Objects.Count());

            // First object
            ulong expectedGuid1 = 2287828610704211975;
            var addedObject1 = (WoWGameObject)WoWSharpObjectManager.Instance.Objects.First(o => o.Guid == expectedGuid1);

            Assert.NotNull(addedObject1);
            Assert.Equal(WoWObjectType.GameObj, addedObject1.ObjectType);
            Assert.Equal((uint)175080, addedObject1.Entry);
            Assert.Equal(1.000f, addedObject1.ScaleX);
            Assert.Equal((uint)3031, addedObject1.DisplayId);
            Assert.Equal(GOState.Ready, addedObject1.GoState);
            Assert.Equal((uint)15, addedObject1.TypeId);
            Assert.Equal((uint)303309, addedObject1.Level);
            Assert.Equal((uint)255, addedObject1.AnimProgress);


            // Third object
            ulong expectedGuid2 = 2287828610704211973;
            var addedObject2 = (WoWGameObject)WoWSharpObjectManager.Instance.Objects.First(o => o.Guid == expectedGuid2);

            Assert.NotNull(addedObject2);
            Assert.Equal(WoWObjectType.GameObj, addedObject2.ObjectType);
            Assert.Equal((uint)177233, addedObject2.Entry);
            Assert.Equal(1.000f, addedObject2.ScaleX);
            Assert.Equal((uint)3015, addedObject2.DisplayId);
            Assert.Equal(GOState.Ready, addedObject2.GoState);
            Assert.Equal((uint)15, addedObject2.TypeId);
            Assert.Equal((uint)316916, addedObject2.Level);
            Assert.Equal((uint)255, addedObject2.AnimProgress);

            // Second object
            ulong expectedGuid3 = 2287828610704211972;
            var addedObject3 = (WoWGameObject)WoWSharpObjectManager.Instance.Objects.First(o => o.Guid == expectedGuid3);

            Assert.NotNull(addedObject2);
            Assert.Equal(WoWObjectType.GameObj, addedObject3.ObjectType);
            Assert.Equal((uint)176310, addedObject3.Entry);
            Assert.Equal(1.000f, addedObject3.ScaleX);
            Assert.Equal((uint)3015, addedObject3.DisplayId);
            Assert.Equal(GOState.Ready, addedObject3.GoState);
            Assert.Equal((uint)15, addedObject3.TypeId);
            Assert.Equal((uint)295491, addedObject3.Level);
            Assert.Equal((uint)255, addedObject3.AnimProgress);

            // Fourth object
            ulong expectedGuid4 = 2287828610704211971;
            var addedObject4 = (WoWGameObject)WoWSharpObjectManager.Instance.Objects.First(o => o.Guid == expectedGuid4);

            Assert.NotNull(addedObject4);
            Assert.Equal(WoWObjectType.GameObj, addedObject4.ObjectType);
            Assert.Equal((uint)176231, addedObject4.Entry);
            Assert.Equal(1.000f, addedObject4.ScaleX);
            Assert.Equal((uint)3015, addedObject4.DisplayId);
            Assert.Equal(GOState.Ready, addedObject4.GoState);
            Assert.Equal((uint)15, addedObject4.TypeId);
            Assert.Equal((uint)329159, addedObject4.Level);
            Assert.Equal((uint)255, addedObject4.AnimProgress);

            // Fifth object
            ulong expectedGuid5 = 2287828610704211969;
            var addedObject5 = (WoWGameObject)WoWSharpObjectManager.Instance.Objects.First(o => o.Guid == expectedGuid5);

            Assert.NotNull(addedObject5);
            Assert.Equal(WoWObjectType.GameObj, addedObject5.ObjectType);
            Assert.Equal((uint)20808, addedObject5.Entry);
            Assert.Equal(1.000f, addedObject5.ScaleX);
            Assert.Equal((uint)3015, addedObject5.DisplayId);
            Assert.Equal(GOState.Ready, addedObject5.GoState);
            Assert.Equal((uint)15, addedObject5.TypeId);
            Assert.Equal((uint)350660, addedObject5.Level);
            Assert.Equal((uint)255, addedObject5.AnimProgress);

            // Sixth object
            ulong expectedGuid6 = 2287828610704211970;
            var addedObject6 = (WoWGameObject)WoWSharpObjectManager.Instance.Objects.First(o => o.Guid == expectedGuid6);

            Assert.NotNull(addedObject6);
            Assert.Equal(WoWObjectType.GameObj, addedObject6.ObjectType);
            Assert.Equal((uint)176244, addedObject6.Entry);
            Assert.Equal(1.000f, addedObject6.ScaleX);
            Assert.Equal((uint)3015, addedObject6.DisplayId);
            Assert.Equal(GOState.Ready, addedObject6.GoState);
            Assert.Equal((uint)15, addedObject6.TypeId);
            Assert.Equal((uint)316182, addedObject6.Level);
            Assert.Equal((uint)255, addedObject6.AnimProgress);
        }
    }

    [Collection("Sequential ObjectManager tests")]
    public class SMSG_COMPRESSED_UPDATE_OBJECT_Player_Tests(ObjectManagerFixture _) : IClassFixture<ObjectManagerFixture>
    {
        private Mock<WoWClient> _woWClientMock = _._woWClient;

        [Fact]
        public void ShouldDecompressAndParsePlayerCharacter()
        {
            var opcode = Opcode.SMSG_COMPRESSED_UPDATE_OBJECT;
            byte[] data = FileReader.ReadBinaryFile($"{Path.Combine(Directory.GetCurrentDirectory(), "Resources", opcode.ToString())}\\20240815_2.bin");

            // Call the HandleUpdateObject method on ObjectUpdateHandler
            ObjectUpdateHandler.HandleUpdateObject(opcode, data);

            _woWClientMock.Setup(expression => expression.SendNameQuery(150));

            WoWSharpObjectManager.Instance.ProcessUpdates();

            // First object (WoWItem)
            var item1 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402422, item1.Guid);
            Assert.Equal(WoWObjectType.Item, item1.ObjectType);
            Assert.Equal(0xB6, item1.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item1.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16963, item1.Entry);
            Assert.Equal(1.0f, item1.ScaleX);
            Assert.Equal(0, item1.Position.X);
            Assert.Equal(0, item1.Position.Y);
            Assert.Equal(0, item1.Position.Z);
            Assert.Equal(0f, item1.Facing);
            Assert.Equal(0u, item1.LastUpdated);
            Assert.Equal((uint)0, item1.ItemId);
            Assert.Equal((ulong)150, item1.Owner.FullGuid);
            Assert.Equal((ulong)150, item1.Contained.FullGuid);
            Assert.Equal((ulong)0, item1.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item1.StackCount);
            Assert.Equal((uint)0, item1.Duration);
            Assert.Equal(new uint[5], item1.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item1.ItemDynamicFlags);
            Assert.Equal((uint)2545, item1.Enchantments[0]);
            Assert.Equal((uint)0, item1.PropertySeed);
            Assert.Equal((uint)0, item1.RandomPropertiesId);
            Assert.Equal((uint)0, item1.ItemTextId);
            Assert.Equal((uint)100, item1.Durability);
            Assert.Equal((uint)100, item1.MaxDurability);
            Assert.Equal((uint)0, item1.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item1.Quality);

            // Second object (WoWItem)
            var item2 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402423, item2.Guid);
            Assert.Equal(WoWObjectType.Item, item2.ObjectType);
            Assert.Equal(0xB7, item2.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item2.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)18404, item2.Entry);
            Assert.Equal(1.0f, item2.ScaleX);
            Assert.Equal(0, item2.Position.X);
            Assert.Equal(0, item2.Position.Y);
            Assert.Equal(0, item2.Position.Z);
            Assert.Equal(0f, item2.Facing);
            Assert.Equal(0u, item2.LastUpdated);
            Assert.Equal((uint)0, item2.ItemId);
            Assert.Equal((ulong)150, item2.Owner.FullGuid);
            Assert.Equal((ulong)150, item2.Contained.FullGuid);
            Assert.Equal((ulong)0, item2.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item2.StackCount);
            Assert.Equal((uint)0, item2.Duration);
            Assert.Equal(new uint[5], item2.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item2.ItemDynamicFlags);
            Assert.Equal(new uint[21], item2.Enchantments);
            Assert.Equal((uint)0, item2.PropertySeed);
            Assert.Equal((uint)0, item2.RandomPropertiesId);
            Assert.Equal((uint)0, item2.ItemTextId);
            Assert.Equal((uint)0, item2.Durability);
            Assert.Equal((uint)0, item2.MaxDurability);
            Assert.Equal((uint)0, item2.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item2.Quality);

            // Third object (WoWItem)
            var item3 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402424, item3.Guid);
            Assert.Equal(WoWObjectType.Item, item3.ObjectType);
            Assert.Equal(0xB8, item3.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item3.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16961, item3.Entry);
            Assert.Equal(1.0f, item3.ScaleX);
            Assert.Equal(0, item3.Position.X);
            Assert.Equal(0, item3.Position.Y);
            Assert.Equal(0, item3.Position.Z);
            Assert.Equal(0f, item3.Facing);
            Assert.Equal(0u, item3.LastUpdated);
            Assert.Equal((uint)0, item3.ItemId);
            Assert.Equal((ulong)150, item3.Owner.FullGuid);
            Assert.Equal((ulong)150, item3.Contained.FullGuid);
            Assert.Equal((ulong)0, item3.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item3.StackCount);
            Assert.Equal((uint)0, item3.Duration);
            Assert.Equal(new uint[5], item3.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item3.ItemDynamicFlags);
            Assert.Equal((uint)2606, item3.Enchantments[0]);
            Assert.Equal((uint)0, item3.PropertySeed);
            Assert.Equal((uint)0, item3.RandomPropertiesId);
            Assert.Equal((uint)0, item3.ItemTextId);
            Assert.Equal((uint)100, item3.Durability);
            Assert.Equal((uint)100, item3.MaxDurability);
            Assert.Equal((uint)0, item3.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item3.Quality);

            // Fourth object (WoWItem)
            var item4 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402469, item4.Guid);
            Assert.Equal(WoWObjectType.Item, item4.ObjectType);
            Assert.Equal(0xE5, item4.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item4.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)4335, item4.Entry);
            Assert.Equal(1.0f, item4.ScaleX);
            Assert.Equal(0, item4.Position.X);
            Assert.Equal(0, item4.Position.Y);
            Assert.Equal(0, item4.Position.Z);
            Assert.Equal(0f, item4.Facing);
            Assert.Equal(0u, item4.LastUpdated);
            Assert.Equal((uint)0, item4.ItemId);
            Assert.Equal((ulong)150, item4.Owner.FullGuid);
            Assert.Equal((ulong)150, item4.Contained.FullGuid);
            Assert.Equal((ulong)0, item4.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item4.StackCount);
            Assert.Equal((uint)0, item4.Duration);
            Assert.Equal(new uint[5], item4.SpellCharges);
            Assert.Equal((ItemDynFlags)0, item4.ItemDynamicFlags);
            Assert.Equal(new uint[21], item4.Enchantments);
            Assert.Equal((uint)0, item4.PropertySeed);
            Assert.Equal((uint)0, item4.RandomPropertiesId);
            Assert.Equal((uint)0, item4.ItemTextId);
            Assert.Equal((uint)0, item4.Durability);
            Assert.Equal((uint)0, item4.MaxDurability);
            Assert.Equal((uint)0, item4.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item4.Quality);

            // Fifth object (WoWItem)
            var item5 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402426, item5.Guid);
            Assert.Equal(WoWObjectType.Item, item5.ObjectType);
            Assert.Equal(0xBA, item5.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item5.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16966, item5.Entry);
            Assert.Equal(1.0f, item5.ScaleX);
            Assert.Equal(0, item5.Position.X);
            Assert.Equal(0, item5.Position.Y);
            Assert.Equal(0, item5.Position.Z);
            Assert.Equal(0f, item5.Facing);
            Assert.Equal(0u, item5.LastUpdated);
            Assert.Equal((uint)0, item5.ItemId);
            Assert.Equal((ulong)150, item5.Owner.FullGuid);
            Assert.Equal((ulong)150, item5.Contained.FullGuid);
            Assert.Equal((ulong)0, item5.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item5.StackCount);
            Assert.Equal((uint)0, item5.Duration);
            Assert.Equal(new uint[5], item5.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item5.ItemDynamicFlags);
            Assert.Equal((uint)1892, item5.Enchantments[0]);
            Assert.Equal((uint)0, item5.PropertySeed);
            Assert.Equal((uint)0, item5.RandomPropertiesId);
            Assert.Equal((uint)0, item5.ItemTextId);
            Assert.Equal((uint)165, item5.Durability);
            Assert.Equal((uint)165, item5.MaxDurability);
            Assert.Equal((uint)0, item5.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item5.Quality);

            // Sixth object (WoWItem)
            var item6 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402429, item6.Guid);
            Assert.Equal(WoWObjectType.Item, item6.ObjectType);
            Assert.Equal(0xBD, item6.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item6.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16960, item6.Entry);
            Assert.Equal(1.0f, item6.ScaleX);
            Assert.Equal(0, item6.Position.X);
            Assert.Equal(0, item6.Position.Y);
            Assert.Equal(0, item6.Position.Z);
            Assert.Equal(0f, item6.Facing);
            Assert.Equal(0u, item6.LastUpdated);
            Assert.Equal((uint)0, item6.ItemId);
            Assert.Equal((ulong)150, item6.Owner.FullGuid);
            Assert.Equal((ulong)150, item6.Contained.FullGuid);
            Assert.Equal((ulong)0, item6.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item6.StackCount);
            Assert.Equal((uint)0, item6.Duration);
            Assert.Equal(new uint[5], item6.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item6.ItemDynamicFlags);
            Assert.Equal(new uint[21], item6.Enchantments);
            Assert.Equal((uint)0, item6.PropertySeed);
            Assert.Equal((uint)0, item6.RandomPropertiesId);
            Assert.Equal((uint)0, item6.ItemTextId);
            Assert.Equal((uint)55, item6.Durability);
            Assert.Equal((uint)55, item6.MaxDurability);
            Assert.Equal((uint)0, item6.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item6.Quality);

            // Seventh object (WoWItem)
            var item7 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402430, item7.Guid);
            Assert.Equal(WoWObjectType.Item, item7.ObjectType);
            Assert.Equal(0xBE, item7.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item7.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16962, item7.Entry);
            Assert.Equal(1.0f, item7.ScaleX);
            Assert.Equal(0, item7.Position.X);
            Assert.Equal(0, item7.Position.Y);
            Assert.Equal(0, item7.Position.Z);
            Assert.Equal(0f, item7.Facing);
            Assert.Equal(0u, item7.LastUpdated);
            Assert.Equal((uint)0, item7.ItemId);
            Assert.Equal((ulong)150, item7.Owner.FullGuid);
            Assert.Equal((ulong)150, item7.Contained.FullGuid);
            Assert.Equal((ulong)0, item7.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item7.StackCount);
            Assert.Equal((uint)0, item7.Duration);
            Assert.Equal(new uint[5], item7.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item7.ItemDynamicFlags);
            Assert.Equal((uint)2583, item7.Enchantments[0]);
            Assert.Equal((uint)0, item7.PropertySeed);
            Assert.Equal((uint)0, item7.RandomPropertiesId);
            Assert.Equal((uint)0, item7.ItemTextId);
            Assert.Equal((uint)120, item7.Durability);
            Assert.Equal((uint)120, item7.MaxDurability);
            Assert.Equal((uint)0, item7.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item7.Quality);

            // Eighth object (WoWItem)
            var item8 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402431, item8.Guid);
            Assert.Equal(WoWObjectType.Item, item8.ObjectType);
            Assert.Equal(0xBF, item8.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item8.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16965, item8.Entry);
            Assert.Equal(1.0f, item8.ScaleX);
            Assert.Equal(0, item8.Position.X);
            Assert.Equal(0, item8.Position.Y);
            Assert.Equal(0, item8.Position.Z);
            Assert.Equal(0f, item8.Facing);
            Assert.Equal(0u, item8.LastUpdated);
            Assert.Equal((uint)0, item8.ItemId);
            Assert.Equal((ulong)150, item8.Owner.FullGuid);
            Assert.Equal((ulong)150, item8.Contained.FullGuid);
            Assert.Equal((ulong)0, item8.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item8.StackCount);
            Assert.Equal((uint)0, item8.Duration);
            Assert.Equal(new uint[5], item8.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item8.ItemDynamicFlags);
            Assert.Equal((uint)929, item8.Enchantments[0]);
            Assert.Equal((uint)0, item8.PropertySeed);
            Assert.Equal((uint)0, item8.RandomPropertiesId);
            Assert.Equal((uint)0, item8.ItemTextId);
            Assert.Equal((uint)75, item8.Durability);
            Assert.Equal((uint)75, item8.MaxDurability);
            Assert.Equal((uint)0, item8.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item8.Quality);

            // Ninth object (WoWItem)
            var item9 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402427, item9.Guid);
            Assert.Equal(WoWObjectType.Item, item9.ObjectType);
            Assert.Equal(0xBB, item9.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item9.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16959, item9.Entry);
            Assert.Equal(1.0f, item9.ScaleX);
            Assert.Equal(0, item9.Position.X);
            Assert.Equal(0, item9.Position.Y);
            Assert.Equal(0, item9.Position.Z);
            Assert.Equal(0f, item9.Facing);
            Assert.Equal(0u, item9.LastUpdated);
            Assert.Equal((uint)0, item9.ItemId);
            Assert.Equal((ulong)150, item9.Owner.FullGuid);
            Assert.Equal((ulong)150, item9.Contained.FullGuid);
            Assert.Equal((ulong)0, item9.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item9.StackCount);
            Assert.Equal((uint)0, item9.Duration);
            Assert.Equal(new uint[5], item9.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item9.ItemDynamicFlags);
            Assert.Equal((uint)1886, item9.Enchantments[0]);
            Assert.Equal((uint)0, item9.PropertySeed);
            Assert.Equal((uint)0, item9.RandomPropertiesId);
            Assert.Equal((uint)0, item9.ItemTextId);
            Assert.Equal((uint)55, item9.Durability);
            Assert.Equal((uint)55, item9.MaxDurability);
            Assert.Equal((uint)0, item9.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item9.Quality);

            // Tenth object (WoWItem)
            var item10 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402428, item10.Guid);
            Assert.Equal(WoWObjectType.Item, item10.ObjectType);
            Assert.Equal(0xBC, item10.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item10.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)16964, item10.Entry);
            Assert.Equal(1.0f, item10.ScaleX);
            Assert.Equal(0, item10.Position.X);
            Assert.Equal(0, item10.Position.Y);
            Assert.Equal(0, item10.Position.Z);
            Assert.Equal(0f, item10.Facing);
            Assert.Equal(0u, item10.LastUpdated);
            Assert.Equal((uint)0, item10.ItemId);
            Assert.Equal((ulong)150, item10.Owner.FullGuid);
            Assert.Equal((ulong)150, item10.Contained.FullGuid);
            Assert.Equal((ulong)0, item10.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item10.StackCount);
            Assert.Equal((uint)0, item10.Duration);
            Assert.Equal(new uint[5], item10.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item10.ItemDynamicFlags);
            Assert.Equal((uint)2613, item10.Enchantments[0]);
            Assert.Equal((uint)0, item10.PropertySeed);
            Assert.Equal((uint)0, item10.RandomPropertiesId);
            Assert.Equal((uint)0, item10.ItemTextId);
            Assert.Equal((uint)55, item10.Durability);
            Assert.Equal((uint)55, item10.MaxDurability);
            Assert.Equal((uint)0, item10.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item10.Quality);

            // Eleventh object (WoWItem)
            var item11 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402433, item11.Guid);
            Assert.Equal(WoWObjectType.Item, item11.ObjectType);
            Assert.Equal(0xC1, item11.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item11.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)18821, item11.Entry);
            Assert.Equal(1.0f, item11.ScaleX);
            Assert.Equal(0, item11.Position.X);
            Assert.Equal(0, item11.Position.Y);
            Assert.Equal(0, item11.Position.Z);
            Assert.Equal(0f, item11.Facing);
            Assert.Equal(0u, item11.LastUpdated);
            Assert.Equal((uint)0, item11.ItemId);
            Assert.Equal((ulong)150, item11.Owner.FullGuid);
            Assert.Equal((ulong)150, item11.Contained.FullGuid);
            Assert.Equal((ulong)0, item11.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item11.StackCount);
            Assert.Equal((uint)0, item11.Duration);
            Assert.Equal(new uint[5], item11.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item11.ItemDynamicFlags);
            Assert.Equal(new uint[21], item11.Enchantments);
            Assert.Equal((uint)0, item11.PropertySeed);
            Assert.Equal((uint)0, item11.RandomPropertiesId);
            Assert.Equal((uint)0, item11.ItemTextId);
            Assert.Equal((uint)0, item11.Durability);
            Assert.Equal((uint)0, item11.MaxDurability);
            Assert.Equal((uint)0, item11.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item11.Quality);

            // Twelfth object (WoWItem)
            var item12 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402432, item12.Guid);
            Assert.Equal(WoWObjectType.Item, item12.ObjectType);
            Assert.Equal(0xC0, item12.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item12.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)17063, item12.Entry);
            Assert.Equal(1.0f, item12.ScaleX);
            Assert.Equal(0, item12.Position.X);
            Assert.Equal(0, item12.Position.Y);
            Assert.Equal(0, item12.Position.Z);
            Assert.Equal(0f, item12.Facing);
            Assert.Equal(0u, item12.LastUpdated);
            Assert.Equal((uint)0, item12.ItemId);
            Assert.Equal((ulong)150, item12.Owner.FullGuid);
            Assert.Equal((ulong)150, item12.Contained.FullGuid);
            Assert.Equal((ulong)0, item12.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item12.StackCount);
            Assert.Equal((uint)0, item12.Duration);
            Assert.Equal(new uint[5], item12.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item12.ItemDynamicFlags);
            Assert.Equal(new uint[21], item12.Enchantments);
            Assert.Equal((uint)0, item12.PropertySeed);
            Assert.Equal((uint)0, item12.RandomPropertiesId);
            Assert.Equal((uint)0, item12.ItemTextId);
            Assert.Equal((uint)0, item12.Durability);
            Assert.Equal((uint)0, item12.MaxDurability);
            Assert.Equal((uint)0, item12.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item12.Quality);

            // Thirteenth object (WoWItem)
            var item13 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402435, item13.Guid);
            Assert.Equal(WoWObjectType.Item, item13.ObjectType);
            Assert.Equal(0xC3, item13.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item13.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)13965, item13.Entry);
            Assert.Equal(1.0f, item13.ScaleX);
            Assert.Equal(0, item13.Position.X);
            Assert.Equal(0, item13.Position.Y);
            Assert.Equal(0, item13.Position.Z);
            Assert.Equal(0f, item13.Facing);
            Assert.Equal(0u, item13.LastUpdated);
            Assert.Equal((uint)0, item13.ItemId);
            Assert.Equal((ulong)150, item13.Owner.FullGuid);
            Assert.Equal((ulong)150, item13.Contained.FullGuid);
            Assert.Equal((ulong)0, item13.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item13.StackCount);
            Assert.Equal((uint)0, item13.Duration);
            Assert.Equal(new uint[5], item13.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item13.ItemDynamicFlags);
            Assert.Equal(new uint[21], item13.Enchantments);
            Assert.Equal((uint)0, item13.PropertySeed);
            Assert.Equal((uint)0, item13.RandomPropertiesId);
            Assert.Equal((uint)0, item13.ItemTextId);
            Assert.Equal((uint)0, item13.Durability);
            Assert.Equal((uint)0, item13.MaxDurability);
            Assert.Equal((uint)0, item13.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item13.Quality);

            // Fourteenth object (WoWItem)
            var item14 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402434, item14.Guid);
            Assert.Equal(WoWObjectType.Item, item14.ObjectType);
            Assert.Equal(0xC2, item14.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item14.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)11815, item14.Entry);
            Assert.Equal(1.0f, item14.ScaleX);
            Assert.Equal(0, item14.Position.X);
            Assert.Equal(0, item14.Position.Y);
            Assert.Equal(0, item14.Position.Z);
            Assert.Equal(0f, item14.Facing);
            Assert.Equal(0u, item14.LastUpdated);
            Assert.Equal((uint)0, item14.ItemId);
            Assert.Equal((ulong)150, item14.Owner.FullGuid);
            Assert.Equal((ulong)150, item14.Contained.FullGuid);
            Assert.Equal((ulong)0, item14.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item14.StackCount);
            Assert.Equal((uint)0, item14.Duration);
            Assert.Equal(new uint[5], item14.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item14.ItemDynamicFlags);
            Assert.Equal(new uint[21], item14.Enchantments);
            Assert.Equal((uint)0, item14.PropertySeed);
            Assert.Equal((uint)0, item14.RandomPropertiesId);
            Assert.Equal((uint)0, item14.ItemTextId);
            Assert.Equal((uint)0, item14.Durability);
            Assert.Equal((uint)0, item14.MaxDurability);
            Assert.Equal((uint)0, item14.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item14.Quality);

            // Fifteenth object (WoWItem)
            var item15 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402425, item15.Guid);
            Assert.Equal(WoWObjectType.Item, item15.ObjectType);
            Assert.Equal(0xB9, item15.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item15.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)17107, item15.Entry);
            Assert.Equal(1.0f, item15.ScaleX);
            Assert.Equal(0, item15.Position.X);
            Assert.Equal(0, item15.Position.Y);
            Assert.Equal(0, item15.Position.Z);
            Assert.Equal(0f, item15.Facing);
            Assert.Equal(0u, item15.LastUpdated);
            Assert.Equal((uint)0, item15.ItemId);
            Assert.Equal((ulong)150, item15.Owner.FullGuid);
            Assert.Equal((ulong)150, item15.Contained.FullGuid);
            Assert.Equal((ulong)0, item15.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item15.StackCount);
            Assert.Equal((uint)0, item15.Duration);
            Assert.Equal(new uint[5], item15.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item15.ItemDynamicFlags);
            Assert.Equal((uint)1889, item15.Enchantments[0]);
            Assert.Equal((uint)0, item15.PropertySeed);
            Assert.Equal((uint)0, item15.RandomPropertiesId);
            Assert.Equal((uint)0, item15.ItemTextId);
            Assert.Equal((uint)0, item15.Durability);
            Assert.Equal((uint)0, item15.MaxDurability);
            Assert.Equal((uint)0, item15.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item15.Quality);

            // Sixteenth object (WoWItem)
            var item16 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402420, item16.Guid);
            Assert.Equal(WoWObjectType.Item, item16.ObjectType);
            Assert.Equal(0xB4, item16.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item16.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)19351, item16.Entry);
            Assert.Equal(1.0f, item16.ScaleX);
            Assert.Equal(0, item16.Position.X);
            Assert.Equal(0, item16.Position.Y);
            Assert.Equal(0, item16.Position.Z);
            Assert.Equal(0f, item16.Facing);
            Assert.Equal(0u, item16.LastUpdated);
            Assert.Equal((uint)0, item16.ItemId);
            Assert.Equal((ulong)150, item16.Owner.FullGuid);
            Assert.Equal((ulong)150, item16.Contained.FullGuid);
            Assert.Equal((ulong)0, item16.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item16.StackCount);
            Assert.Equal((uint)0, item16.Duration);
            Assert.Equal(new uint[5], item16.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item16.ItemDynamicFlags);
            Assert.Equal((uint)1900, item16.Enchantments[0]);
            Assert.Equal((uint)0, item16.PropertySeed);
            Assert.Equal((uint)0, item16.RandomPropertiesId);
            Assert.Equal((uint)0, item16.ItemTextId);
            Assert.Equal((uint)105, item16.Durability);
            Assert.Equal((uint)105, item16.MaxDurability);
            Assert.Equal((uint)0, item16.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item16.Quality);

            // Seventeenth object (WoWItem)
            var item17 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402441, item17.Guid);
            Assert.Equal(WoWObjectType.Item, item17.ObjectType);
            Assert.Equal(0xC9, item17.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item17.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)17066, item17.Entry);
            Assert.Equal(1.0f, item17.ScaleX);
            Assert.Equal(0, item17.Position.X);
            Assert.Equal(0, item17.Position.Y);
            Assert.Equal(0, item17.Position.Z);
            Assert.Equal(0f, item17.Facing);
            Assert.Equal(0u, item17.LastUpdated);
            Assert.Equal((uint)0, item17.ItemId);
            Assert.Equal((ulong)150, item17.Owner.FullGuid);
            Assert.Equal((ulong)150, item17.Contained.FullGuid);
            Assert.Equal((ulong)0, item17.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item17.StackCount);
            Assert.Equal((uint)0, item17.Duration);
            Assert.Equal(new uint[5], item17.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item17.ItemDynamicFlags);
            Assert.Equal((uint)929, item17.Enchantments[0]);
            Assert.Equal((uint)0, item17.PropertySeed);
            Assert.Equal((uint)0, item17.RandomPropertiesId);
            Assert.Equal((uint)0, item17.ItemTextId);
            Assert.Equal((uint)120, item17.Durability);
            Assert.Equal((uint)120, item17.MaxDurability);
            Assert.Equal((uint)0, item17.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item17.Quality);

            // Eighteenth object (WoWItem)
            var item18 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402442, item18.Guid);
            Assert.Equal(WoWObjectType.Item, item18.ObjectType);
            Assert.Equal(0xCA, item18.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item18.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)22656, item18.Entry);
            Assert.Equal(1.0f, item18.ScaleX);
            Assert.Equal(0, item18.Position.X);
            Assert.Equal(0, item18.Position.Y);
            Assert.Equal(0, item18.Position.Z);
            Assert.Equal(0f, item18.Facing);
            Assert.Equal(0u, item18.LastUpdated);
            Assert.Equal((uint)0, item18.ItemId);
            Assert.Equal((ulong)150, item18.Owner.FullGuid);
            Assert.Equal((ulong)150, item18.Contained.FullGuid);
            Assert.Equal((ulong)0, item18.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item18.StackCount);
            Assert.Equal((uint)0, item18.Duration);
            Assert.Equal(new uint[5], item18.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item18.ItemDynamicFlags);
            Assert.Equal((uint)663, item18.Enchantments[0]);
            Assert.Equal((uint)0, item18.PropertySeed);
            Assert.Equal((uint)0, item18.RandomPropertiesId);
            Assert.Equal((uint)0, item18.ItemTextId);
            Assert.Equal((uint)90, item18.Durability);
            Assert.Equal((uint)90, item18.MaxDurability);
            Assert.Equal((uint)0, item18.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item18.Quality);

            // Nineteenth object (WoWItem)
            var item19 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402436, item19.Guid);
            Assert.Equal(WoWObjectType.Item, item19.ObjectType);
            Assert.Equal(0xC4, item19.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item19.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)23192, item19.Entry);
            Assert.Equal(1.0f, item19.ScaleX);
            Assert.Equal(0, item19.Position.X);
            Assert.Equal(0, item19.Position.Y);
            Assert.Equal(0, item19.Position.Z);
            Assert.Equal(0f, item19.Facing);
            Assert.Equal(0u, item19.LastUpdated);
            Assert.Equal((uint)0, item19.ItemId);
            Assert.Equal((ulong)150, item19.Owner.FullGuid);
            Assert.Equal((ulong)150, item19.Contained.FullGuid);
            Assert.Equal((ulong)0, item19.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item19.StackCount);
            Assert.Equal((uint)0, item19.Duration);
            Assert.Equal(new uint[5], item19.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item19.ItemDynamicFlags);
            Assert.Equal(new uint[21], item19.Enchantments);
            Assert.Equal((uint)0, item19.PropertySeed);
            Assert.Equal((uint)0, item19.RandomPropertiesId);
            Assert.Equal((uint)0, item19.ItemTextId);
            Assert.Equal((uint)0, item19.Durability);
            Assert.Equal((uint)0, item19.MaxDurability);
            Assert.Equal((uint)0, item19.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item19.Quality);

            // Twentieth object (Container)
            var container20 = (WoWContainer)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427396374, container20.Guid);
            Assert.Equal(WoWObjectType.Container, container20.ObjectType);
            Assert.Equal(0x16, container20.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x21, container20.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)17966, container20.Entry);
            Assert.Equal(1.0f, container20.ScaleX);
            Assert.Equal(0, container20.Position.X);
            Assert.Equal(0, container20.Position.Y);
            Assert.Equal(0, container20.Position.Z);
            Assert.Equal(0f, container20.Facing);
            Assert.Equal(0u, container20.LastUpdated);
            Assert.Equal((uint)0, container20.ItemId);
            Assert.Equal((ulong)150, container20.Owner.FullGuid);
            Assert.Equal((ulong)150, container20.Contained.FullGuid);
            Assert.Equal((ulong)0, container20.GiftCreator.FullGuid);
            Assert.Equal((uint)1, container20.StackCount);
            Assert.Equal((uint)0, container20.Duration);
            Assert.Equal(new uint[5], container20.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, container20.ItemDynamicFlags);
            Assert.Equal(new uint[21], container20.Enchantments);
            Assert.Equal((uint)0, container20.PropertySeed);
            Assert.Equal((uint)0, container20.RandomPropertiesId);
            Assert.Equal((uint)0, container20.ItemTextId);
            Assert.Equal((uint)0, container20.Durability);
            Assert.Equal((uint)0, container20.MaxDurability);
            Assert.Equal((uint)0, container20.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, container20.Quality);
            Assert.Equal(18, container20.NumOfSlots);

            // Twenty-first object (WoWItem)
            var item21 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402419, item21.Guid);
            Assert.Equal(WoWObjectType.Item, item21.ObjectType);
            Assert.Equal(0xB3, item21.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item21.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)19364, item21.Entry);
            Assert.Equal(1.0f, item21.ScaleX);
            Assert.Equal(0, item21.Position.X);
            Assert.Equal(0, item21.Position.Y);
            Assert.Equal(0, item21.Position.Z);
            Assert.Equal(0f, item21.Facing);
            Assert.Equal(0u, item21.LastUpdated);
            Assert.Equal((uint)0, item21.ItemId);
            Assert.Equal((ulong)150, item21.Owner.FullGuid);
            Assert.Equal((ulong)4611686018427396374, item21.Contained.FullGuid);
            Assert.Equal((ulong)0, item21.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item21.StackCount);
            Assert.Equal((uint)0, item21.Duration);
            Assert.Equal(new uint[5], item21.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item21.ItemDynamicFlags);
            Assert.Equal(new uint[21], item21.Enchantments);
            Assert.Equal((uint)0, item21.PropertySeed);
            Assert.Equal((uint)0, item21.RandomPropertiesId);
            Assert.Equal((uint)0, item21.ItemTextId);
            Assert.Equal((uint)120, item21.Durability);
            Assert.Equal((uint)120, item21.MaxDurability);
            Assert.Equal((uint)0, item21.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item21.Quality);

            // Twenty-second object (Container)
            var container22 = (WoWContainer)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402438, container22.Guid);
            Assert.Equal(WoWObjectType.Container, container22.ObjectType);
            Assert.Equal(0xC6, container22.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, container22.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)14155, container22.Entry);
            Assert.Equal(1.0f, container22.ScaleX);
            Assert.Equal(0, container22.Position.X);
            Assert.Equal(0, container22.Position.Y);
            Assert.Equal(0, container22.Position.Z);
            Assert.Equal(0f, container22.Facing);
            Assert.Equal(0u, container22.LastUpdated);
            Assert.Equal((uint)0, container22.ItemId);
            Assert.Equal((ulong)150, container22.Owner.FullGuid);
            Assert.Equal((ulong)150, container22.Contained.FullGuid);
            Assert.Equal((ulong)0, container22.GiftCreator.FullGuid);
            Assert.Equal((uint)1, container22.StackCount);
            Assert.Equal((uint)0, container22.Duration);
            Assert.Equal(new uint[5], container22.SpellCharges);
            Assert.Equal((ItemDynFlags)0, container22.ItemDynamicFlags);
            Assert.Equal(new uint[21], container22.Enchantments);
            Assert.Equal((uint)0, container22.PropertySeed);
            Assert.Equal((uint)0, container22.RandomPropertiesId);
            Assert.Equal((uint)0, container22.ItemTextId);
            Assert.Equal((uint)0, container22.Durability);
            Assert.Equal((uint)0, container22.MaxDurability);
            Assert.Equal((uint)0, container22.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, container22.Quality);
            Assert.Equal(16, container22.NumOfSlots);

            // Twenty-third object (WoWItem)
            var item23 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402443, item23.Guid);
            Assert.Equal(WoWObjectType.Item, item23.ObjectType);
            Assert.Equal(0xCB, item23.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item23.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)19874, item23.Entry);
            Assert.Equal(1.0f, item23.ScaleX);
            Assert.Equal(0, item23.Position.X);
            Assert.Equal(0, item23.Position.Y);
            Assert.Equal(0, item23.Position.Z);
            Assert.Equal(0f, item23.Facing);
            Assert.Equal(0u, item23.LastUpdated);
            Assert.Equal((uint)0, item23.ItemId);
            Assert.Equal((ulong)150, item23.Owner.FullGuid);
            Assert.Equal((ulong)4611686018427402438, item23.Contained.FullGuid);
            Assert.Equal((ulong)0, item23.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item23.StackCount);
            Assert.Equal((uint)0, item23.Duration);
            Assert.Equal(new uint[5], item23.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item23.ItemDynamicFlags);
            Assert.Equal(new uint[21], item23.Enchantments);
            Assert.Equal((uint)0, item23.PropertySeed);
            Assert.Equal((uint)0, item23.RandomPropertiesId);
            Assert.Equal((uint)0, item23.ItemTextId);
            Assert.Equal((uint)120, item23.Durability);
            Assert.Equal((uint)120, item23.MaxDurability);
            Assert.Equal((uint)0, item23.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item23.Quality);

            // Twenty-fourth object (Container)
            var container24 = (WoWContainer)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402439, container24.Guid);
            Assert.Equal(WoWObjectType.Container, container24.ObjectType);
            Assert.Equal(0xC7, container24.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, container24.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)14046, container24.Entry);
            Assert.Equal(1.0f, container24.ScaleX);
            Assert.Equal(0, container24.Position.X);
            Assert.Equal(0, container24.Position.Y);
            Assert.Equal(0, container24.Position.Z);
            Assert.Equal(0f, container24.Facing);
            Assert.Equal(0u, container24.LastUpdated);
            Assert.Equal((uint)0, container24.ItemId);
            Assert.Equal((ulong)150, container24.Owner.FullGuid);
            Assert.Equal((ulong)150, container24.Contained.FullGuid);
            Assert.Equal((ulong)0, container24.GiftCreator.FullGuid);
            Assert.Equal((uint)1, container24.StackCount);
            Assert.Equal((uint)0, container24.Duration);
            Assert.Equal(new uint[5], container24.SpellCharges);
            Assert.Equal((ItemDynFlags)0, container24.ItemDynamicFlags);
            Assert.Equal(new uint[21], container24.Enchantments);
            Assert.Equal((uint)0, container24.PropertySeed);
            Assert.Equal((uint)0, container24.RandomPropertiesId);
            Assert.Equal((uint)0, container24.ItemTextId);
            Assert.Equal((uint)0, container24.Durability);
            Assert.Equal((uint)0, container24.MaxDurability);
            Assert.Equal((uint)0, container24.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, container24.Quality);
            Assert.Equal(14, container24.NumOfSlots);

            // Twenty-fifth object (WoWItem)
            var item25 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402437, item25.Guid);
            Assert.Equal(WoWObjectType.Item, item25.ObjectType);
            Assert.Equal(0xC5, item25.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item25.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)15997, item25.Entry);
            Assert.Equal(1.0f, item25.ScaleX);
            Assert.Equal(0, item25.Position.X);
            Assert.Equal(0, item25.Position.Y);
            Assert.Equal(0, item25.Position.Z);
            Assert.Equal(0f, item25.Facing);
            Assert.Equal(0u, item25.LastUpdated);
            Assert.Equal((uint)0, item25.ItemId);
            Assert.Equal((ulong)150, item25.Owner.FullGuid);
            Assert.Equal((ulong)4611686018427402439, item25.Contained.FullGuid);
            Assert.Equal((ulong)0, item25.GiftCreator.FullGuid);
            Assert.Equal((uint)200, item25.StackCount);
            Assert.Equal((uint)0, item25.Duration);
            Assert.Equal(new uint[5], item25.SpellCharges);
            Assert.Equal((ItemDynFlags)0, item25.ItemDynamicFlags);
            Assert.Equal(new uint[21], item25.Enchantments);
            Assert.Equal((uint)0, item25.PropertySeed);
            Assert.Equal((uint)0, item25.RandomPropertiesId);
            Assert.Equal((uint)0, item25.ItemTextId);
            Assert.Equal((uint)0, item25.Durability);
            Assert.Equal((uint)0, item25.MaxDurability);
            Assert.Equal((uint)0, item25.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item25.Quality);

            // Twenty-sixth object (Container)
            var container26 = (WoWContainer)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402440, container26.Guid);
            Assert.Equal(WoWObjectType.Container, container26.ObjectType);
            Assert.Equal(0xC8, container26.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, container26.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)22679, container26.Entry);
            Assert.Equal(1.0f, container26.ScaleX);
            Assert.Equal(0, container26.Position.X);
            Assert.Equal(0, container26.Position.Y);
            Assert.Equal(0, container26.Position.Z);
            Assert.Equal(0f, container26.Facing);
            Assert.Equal(0u, container26.LastUpdated);
            Assert.Equal((uint)0, container26.ItemId);
            Assert.Equal((ulong)150, container26.Owner.FullGuid);
            Assert.Equal((ulong)150, container26.Contained.FullGuid);
            Assert.Equal((ulong)0, container26.GiftCreator.FullGuid);
            Assert.Equal((uint)1, container26.StackCount);
            Assert.Equal((uint)0, container26.Duration);
            Assert.Equal(new uint[5], container26.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, container26.ItemDynamicFlags);
            Assert.Equal(new uint[21], container26.Enchantments);
            Assert.Equal((uint)0, container26.PropertySeed);
            Assert.Equal((uint)0, container26.RandomPropertiesId);
            Assert.Equal((uint)0, container26.ItemTextId);
            Assert.Equal((uint)0, container26.Durability);
            Assert.Equal((uint)0, container26.MaxDurability);
            Assert.Equal((uint)0, container26.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, container26.Quality);
            Assert.Equal(18, container26.NumOfSlots);

            // Twenty-seventh object (WoWItem)
            var item27 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402444, item27.Guid);
            Assert.Equal(WoWObjectType.Item, item27.ObjectType);
            Assert.Equal(0xCC, item27.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item27.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)18502, item27.Entry);
            Assert.Equal(1.0f, item27.ScaleX);
            Assert.Equal(0, item27.Position.X);
            Assert.Equal(0, item27.Position.Y);
            Assert.Equal(0, item27.Position.Z);
            Assert.Equal(0f, item27.Facing);
            Assert.Equal(0u, item27.LastUpdated);
            Assert.Equal((uint)0, item27.ItemId);
            Assert.Equal((ulong)150, item27.Owner.FullGuid);
            Assert.Equal((ulong)4611686018427402440, item27.Contained.FullGuid);
            Assert.Equal((ulong)0, item27.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item27.StackCount);
            Assert.Equal((uint)0, item27.Duration);
            Assert.Equal(new uint[5], item27.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item27.ItemDynamicFlags);
            Assert.Equal(new uint[21], item27.Enchantments);
            Assert.Equal((uint)0, item27.PropertySeed);
            Assert.Equal((uint)0, item27.RandomPropertiesId);
            Assert.Equal((uint)0, item27.ItemTextId);
            Assert.Equal((uint)100, item27.Durability);
            Assert.Equal((uint)100, item27.MaxDurability);
            Assert.Equal((uint)0, item27.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item27.Quality);

            // Twenty-eighth object (WoWItem)
            var item28 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427396365, item28.Guid);
            Assert.Equal(WoWObjectType.Item, item28.ObjectType);
            Assert.Equal(0x0D, item28.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x21, item28.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)6948, item28.Entry);
            Assert.Equal(1.0f, item28.ScaleX);
            Assert.Equal(0, item28.Position.X);
            Assert.Equal(0, item28.Position.Y);
            Assert.Equal(0, item28.Position.Z);
            Assert.Equal(0f, item28.Facing);
            Assert.Equal(0u, item28.LastUpdated);
            Assert.Equal((uint)0, item28.ItemId);
            Assert.Equal((ulong)150, item28.Owner.FullGuid);
            Assert.Equal((ulong)150, item28.Contained.FullGuid);
            Assert.Equal((ulong)0, item28.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item28.StackCount);
            Assert.Equal((uint)0, item28.Duration);
            Assert.Equal(new uint[5], item28.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item28.ItemDynamicFlags);
            Assert.Equal(new uint[21], item28.Enchantments);
            Assert.Equal((uint)0, item28.PropertySeed);
            Assert.Equal((uint)0, item28.RandomPropertiesId);
            Assert.Equal((uint)0, item28.ItemTextId);
            Assert.Equal((uint)0, item28.Durability);
            Assert.Equal((uint)0, item28.MaxDurability);
            Assert.Equal((uint)0, item28.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item28.Quality);

            // Twenty-ninth object (WoWItem)
            var item29 = (WoWItem)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)4611686018427402451, item29.Guid);
            Assert.Equal(WoWObjectType.Item, item29.ObjectType);
            Assert.Equal(0xD3, item29.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x38, item29.HighGuid.LowGuidValue[1]);
            Assert.Equal((uint)11145, item29.Entry);
            Assert.Equal(1.0f, item29.ScaleX);
            Assert.Equal(0, item29.Position.X);
            Assert.Equal(0, item29.Position.Y);
            Assert.Equal(0, item29.Position.Z);
            Assert.Equal(0f, item29.Facing);
            Assert.Equal(0u, item29.LastUpdated);
            Assert.Equal((uint)0, item29.ItemId);
            Assert.Equal((ulong)150, item29.Owner.FullGuid);
            Assert.Equal((ulong)150, item29.Contained.FullGuid);
            Assert.Equal((ulong)0, item29.GiftCreator.FullGuid);
            Assert.Equal((uint)1, item29.StackCount);
            Assert.Equal((uint)0, item29.Duration);
            Assert.Equal(new uint[5], item29.SpellCharges);
            Assert.Equal(ItemDynFlags.ITEM_DYNFLAG_BINDED, item29.ItemDynamicFlags);
            Assert.Equal(new uint[21], item29.Enchantments);
            Assert.Equal((uint)0, item29.PropertySeed);
            Assert.Equal((uint)0, item29.RandomPropertiesId);
            Assert.Equal((uint)0, item29.ItemTextId);
            Assert.Equal((uint)0, item29.Durability);
            Assert.Equal((uint)0, item29.MaxDurability);
            Assert.Equal((uint)0, item29.RequiredLevel);
            Assert.Equal(ItemQuality.Poor, item29.Quality);

            // Thirtieth object (Player)
            var player = (WoWPlayer)WoWSharpObjectManager.Instance.Objects.ElementAt(0);
            Assert.Equal((ulong)150, player.Guid);
            Assert.Equal(WoWObjectType.Player, player.ObjectType);
            Assert.Equal(0x96, player.HighGuid.LowGuidValue[0]);
            Assert.Equal(0x00, player.HighGuid.LowGuidValue[1]);
            Assert.Equal(0u, player.Entry);
            Assert.Equal(1.0f, player.ScaleX);
            Assert.Equal(1984.08997f, player.Position.X);
            Assert.Equal(-4792.2299f, player.Position.Y);
            Assert.Equal(55.8203f, player.Position.Z);
            Assert.Equal(2.68104005f, player.Facing);
            Assert.Equal(7114328u, player.LastUpdated);
            Assert.Equal(DynamicFlags.None, player.DynamicFlags);
        }
    }
}
