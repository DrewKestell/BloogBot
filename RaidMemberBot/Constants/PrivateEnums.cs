
namespace RaidMemberBot.Constants
{
    internal static class PrivateEnums
    {
        internal enum ItemCacheLookupType
        {
            None = 0,
            Vendor = 1,
            Quest = 2
        }

        internal static class DynamicFlags
        {
            private static readonly string _nostalrius = "elysium";
            internal static int Untouched = 0x0;

            internal static int IsMarked
            {
                get
                {
                    return 0x2;
                }
            }

            internal static int CanBeLooted
            {
                get
                {
                    return 0x1;
                }
            }

            internal static int TappedByMe
            {
                get
                {
                    return 0x0;
                }
            }

            internal static int TappedByOther => 0x4;
        }

        internal enum PlayerAction
        {
            GuildInvite = 0,
            PartyInvite = 1,
            TradeRequest = 2,
            DuelRequest = 3
        }

        internal enum ControlBitsMouse
        {
            Rightclick = 0x00000001,
            Leftclick = 0x00000002
        }

        internal enum ChatType
        {
            Say = 0,
            Yell = 5,
            Channel = 14,
            Group = 1,
            Guild = 3,
            Whisper = 6
        }

        internal enum LoginState
        {
            login,
            charselect
        }

        internal enum MovementOpCodes
        {
            stopTurn = 0xBE,
            turnLeft = 0xBC,
            turnRight = 0xBD,

            moveStop = 0xB7,
            moveFront = 0xB5,
            moveBack = 0xB6,

            setFacing = 0xDA,

            heartbeat = 0xEE,

            strafeLeft = 0xB8,
            strafeRightStart = 0xB9,
            strafeStop = 0xBA
        }

        internal enum CtmType
        {
            FaceTarget = 0x1,
            Face = 0x2,

            /// <summary>
            ///     Will throw a UI error. Have not figured out how to avoid it!
            /// </summary>
            // ReSharper disable InconsistentNaming
            Stop_ThrowsException = 0x3,
            // ReSharper restore InconsistentNaming
            Move = 0x4,
            NpcInteract = 0x5,
            Loot = 0x6,
            ObjInteract = 0x7,
            FaceOther = 0x8,
            Skin = 0x9,
            AttackLocation = 0xA,
            AttackGuid = 0xB,
            ConstantFace = 0xC,
            None = 0xD,
            Attack = 0x10,
            Idle = 0xC
        }

        internal enum LocationType
        {
            Hotspot,
            Waypoint
        }
    }
}
