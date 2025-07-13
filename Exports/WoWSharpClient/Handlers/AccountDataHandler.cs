using GameData.Core.Enums;
using System.Text;

namespace WoWSharpClient.Handlers
{
    public static class AccountDataHandler
    {
        private static readonly uint[] _accountDataTimestamps = new uint[8];

        public static void HandleAccountData(Opcode opcode, byte[] data)
        {
            switch (opcode)
            {
                case Opcode.SMSG_ACCOUNT_DATA_TIMES:
                    HandleAccountDataTimes(data);
                    break;
                case Opcode.SMSG_UPDATE_ACCOUNT_DATA:
                    HandleUpdateAccountData(data);
                    break;
                default:
                    Console.WriteLine($"Unhandled AccountData opcode: {opcode}");
                    break;
            }
        }

        private static void HandleAccountDataTimes(byte[] data)
        {
            if (data.Length < 36)
            {
                Console.WriteLine("Invalid SMSG_ACCOUNT_DATA_TIMES packet size.");
                return;
            }

            uint serverTime = BitConverter.ToUInt32(data, 0);
            for (int i = 0; i < 8; i++)
            {
                _accountDataTimestamps[i] = BitConverter.ToUInt32(data, 4 + i * 4);
            }

            for (int i = 0; i < 8; i++)
            {
                //Console.WriteLine($"[AccountDataTimes] Type {i}: Timestamp = {_accountDataTimestamps[i]}");
            }

            //_woWSharpEventEmitter.Emit("AccountDataTimesReceived", _accountDataTimestamps);
        }

        private static void HandleUpdateAccountData(byte[] data)
        {
            if (data.Length < 16)
            {
                Console.WriteLine("Invalid SMSG_UPDATE_ACCOUNT_DATA packet size.");
                return;
            }

            uint guid = BitConverter.ToUInt32(data, 0); // Often unused in 1.12.1
            uint type = BitConverter.ToUInt32(data, 4);
            uint timestamp = BitConverter.ToUInt32(data, 8);
            uint size = BitConverter.ToUInt32(data, 12);

            if (data.Length < 16 + size)
            {
                Console.WriteLine($"SMSG_UPDATE_ACCOUNT_DATA: Declared size {size} exceeds actual packet size.");
                return;
            }

            byte[] payload = new byte[size];
            Array.Copy(data, 16, payload, 0, size);

            // For debugging: try to render as UTF-8 if possible
            string debugData = Encoding.UTF8.GetString(payload).Trim('\0');

            //_woWSharpEventEmitter.FireOnAccountDataUpdated("AccountDataUpdated", new
            //{
            //    Type = type,
            //    Timestamp = timestamp,
            //    Data = payload
            //});
        }
    }
}
