namespace WoWSlimClient.Models
{
    enum OpType : byte
    {
        UPDATE_PARTIAL = 0,
        UPDATE_MOVEMENT = 1,
        UPDATE_FULL = 2,
        UPDATE_OUT_OF_RANGE = 3,
        UPDATE_IN_RANGE = 4
    }

    [Flags]
    enum UpdateFlags
    {
        UPDATEFLAGS_TRANSPORT = 0x2000000,
        UPDATEFLAGS_4000000 = 0x4000000,
        UPDATEFLAGS_400000 = 0x400000,
        UPDATEFLAGS_200000 = 0x200000,
        UPDATEFLAGS_2000 = 0x2000,
    }

    enum ObjectTypeMask
    {
        TYPE_OBJECT = 0x1,
        TYPE_ITEM = 0x2,
        TYPE_CONTAINER = 0x4,
        TYPE_UNIT = 0x8,
        TYPE_PLAYER = 0x10,
        TYPE_GAMEOBJECT = 0x20,
        TYPE_DYNAMICOBJECT = 0x40,
        TYPE_CORPSE = 0x80,
        TYPE_AIGROUP = 0x100,
        TYPE_AREATRIGGER = 0x200,
    }

    class Operation
    {
        public OpType OType { get; set; }
        public ulong ObjGUID { get; set; }
        public ulong FobjGUID { get; set; }
        public byte ObjType { get; set; }
        public UpdateFlags UfFlags { get; set; }
        public ulong MoveGUID { get; set; }
        public ulong TransportGUID { get; set; }
        public float PY { get; set; }
        public float PX { get; set; }
        public float PZ { get; set; }
        public float PTurn { get; set; }
        public float TPY { get; set; }
        public float TPX { get; set; }
        public float TPZ { get; set; }
        public float TPTurn { get; set; }
        public float Unk2 { get; set; }
        public uint Unk3 { get; set; }
        public float Unkf1 { get; set; }
        public float Unkf2 { get; set; }
        public float Unkf3 { get; set; }
        public float Unkf4 { get; set; }
        public float Unkf5 { get; set; }
        public float Unkff1 { get; set; }
        public float Unkff2 { get; set; }
        public float Unkff3 { get; set; }
        public float Unkff4 { get; set; }
        public float Unkff5 { get; set; }
        public float Unkff6 { get; set; }
        public uint Uf2Flags { get; set; }
        public float Unkfff1 { get; set; }
        public float Unkfff2 { get; set; }
        public float Unkfff3 { get; set; }
        public ulong UnkGUID { get; set; }
        public float Unkfff4 { get; set; }
        public uint Unkdw1 { get; set; }
        public uint Unkdw2 { get; set; }
        public uint BufSize { get; set; }
        public byte[] Splines { get; set; }
        public ulong UnkPGUID { get; set; }
        public uint SizeFlags1 { get; set; }
        public byte[] Buf1 { get; set; }
        public byte[] Buf2 { get; set; } = new byte[0x18];
        public uint SizeFlags2 { get; set; }
        public byte[] Buf3 { get; set; }
        public uint Buf4Size { get; set; }
        public byte[] Buf4 { get; set; }
        public ulong UpdateGUID { get; set; }
        public byte NumUpdateMaskBlocks { get; set; }
        public uint[] MaskBlocks { get; set; }
        public uint[][] Masks { get; set; }
        public uint NumGUIDs { get; set; }
        public ulong[] GUIDs { get; set; }
        public ulong[] GUIDspecial { get; set; }
    }
}
