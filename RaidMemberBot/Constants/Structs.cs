using RaidMemberBot.ExtensionMethods;
using RaidMemberBot.Objects;
using System.Runtime.InteropServices;
using static RaidMemberBot.Constants.Enums;

namespace RaidMemberBot.Constants
{
    /// <summary>
    ///     Intersection struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct Intersection
    {
        internal float X;
        internal float Y;
        internal float Z;
        internal float R;

        public override string ToString()
        {
            return $"Intersection -> X: {X} Y: {Y} Z: {Z} R: {R}";
        }
    }

    /// <summary>
    /// Information about profession and skill level an object requires to be collected
    /// </summary>
    public struct GatherInfo
    {
        public GatherType Type { get; set; }
        public int RequiredSkill { get; set; }
    }

    /// <summary>
    ///     two coordinates (Location 1 and Location 2)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct _XYZXYZ
    {
        internal float X1;
        internal float Y1;
        internal float Z1;
        internal float X2;
        internal float Y2;
        internal float Z2;

        internal _XYZXYZ(float x1, float y1, float z1,
            float x2, float y2, float z2)
            : this()
        {
            X1 = x1;
            Y1 = y1;
            Z1 = z1;
            X2 = x2;
            Y2 = y2;
            Z2 = z2;
        }

        public override string ToString()
        {
            return $"Start -> X: {X1} Y: {Y1} Z: {Z1}\n" + $"End -> X: {X2} Y: {Y2} Z: {Z2}";
        }
    }

    /// <summary>
    ///     Coordinate struct
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct _XYZ
    {
        internal float X;
        internal float Y;
        internal float Z;

        internal _XYZ(float x, float y, float z)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    /// <summary>
    ///     Struct with an item to restock at the restock npc
    /// </summary>
    internal struct RestockItem
    {
        internal string Item;
        internal int RestockUpTo;

        internal RestockItem(string parItem, int parRestockUpTo)
        {
            Item = parItem;
            RestockUpTo = parRestockUpTo;
        }
    }

    internal class NPC
    {
        internal NPC(string parName, Location parPos, string parMapPos)
        {
            Name = parName;
            Coordinates = parPos;
            MapLocation = parMapPos;
        }

        internal string Name { get; private set; }
        internal Location Coordinates { get; private set; }
        internal string MapLocation { get; private set; }
    }

    /// <summary>
    ///     ItemCacheEntry fetched from the game
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct ItemCacheEntry
    {
        [FieldOffset(0x0)] public int Id;

        [FieldOffset(0x8)] private readonly int NamePtr;

        [FieldOffset(0x1C)] public int Quality;
        [FieldOffset(0x24)] public int BuyPrice;
        [FieldOffset(0x28)] public int SellPrice;
        [FieldOffset(0x2c)] public int InventoryType;
        [FieldOffset(0x38)] public int ItemLevel;
        [FieldOffset(0x3C)] public int RequiredLevel;
        [FieldOffset(0x5C)] public int MaxCount;
        [FieldOffset(0x60)] public int MaxStackCount;
        [FieldOffset(0x64)] public int ContainerSlots;

        [FieldOffset(0x68)][MarshalAs(UnmanagedType.Struct)] public _ItemStats ItemStats;

        [FieldOffset(0xB8)][MarshalAs(UnmanagedType.Struct)] public _Damage Damage;

        [FieldOffset(0xF4)] public int Armor;

        [FieldOffset(0x114)] public int AmmoType;

        [FieldOffset(0x1C4)] public int MaxDurability;

        [FieldOffset(0x1D0)] public int BagFamily;

        [StructLayout(LayoutKind.Sequential)]
        public struct _ItemStats
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public uint[] ItemStatType;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)] public int[] ItemStatValue;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct _Damage
        {
            public float DmgMin => BaseDmg + ExtraDmg;

            public float DmgMax => BaseDmgMax + ExtraDmgMax;

            private readonly float BaseDmg;
            private readonly float ExtraDmg;

            private readonly int unk0;
            private readonly int unk1;
            private readonly int unk2;

            private readonly float BaseDmgMax;
            private readonly float ExtraDmgMax;
        }

        [FieldOffset(0x0)] public ItemClass ItemClass;
        public string Name => NamePtr.ReadString();
    }
}
