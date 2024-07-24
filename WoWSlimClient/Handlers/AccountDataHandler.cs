using WoWSlimClient.Models;

namespace WoWSlimClient.Handlers
{
    public static class AccountDataHandler
    {
        public static void HandleAccountData(Opcodes opcode, byte[] data)
        {
            switch (opcode)
            {
                case Opcodes.SMSG_ACCOUNT_DATA_TIMES:
                    HandleAccountDataTimes(data);
                    break;
                case Opcodes.SMSG_UPDATE_ACCOUNT_DATA:
                    HandleUpdateAccountData(data);
                    break;
                default:
                    Console.WriteLine($"Unhandled AccountData opcode: {opcode}");
                    break;
            }
        }

        private static void HandleAccountDataTimes(byte[] data)
        {
            // Parse and handle SMSG_ACCOUNT_DATA_TIMES packet data here
            //Console.WriteLine("Handling SMSG_ACCOUNT_DATA_TIMES");
        }

        private static void HandleUpdateAccountData(byte[] data)
        {
            // Parse and handle SMSG_UPDATE_ACCOUNT_DATA packet data here
            //Console.WriteLine("Handling SMSG_UPDATE_ACCOUNT_DATA");
        }
    }
}
