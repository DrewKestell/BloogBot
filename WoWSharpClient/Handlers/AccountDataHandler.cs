using BotRunner.Constants;
using WoWSharpClient.Manager;

namespace WoWSharpClient.Handlers
{
    public class AccountDataHandler(WoWSharpEventEmitter woWSharpEventEmitter, ObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = woWSharpEventEmitter;
        private readonly ObjectManager _objectManager = objectManager;
        public void HandleAccountData(Opcodes opcode, byte[] data)
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

        private void HandleAccountDataTimes(byte[] data)
        {
            // Parse and handle SMSG_ACCOUNT_DATA_TIMES packet data here
            //Console.WriteLine("Handling SMSG_ACCOUNT_DATA_TIMES");
        }

        private void HandleUpdateAccountData(byte[] data)
        {
            // Parse and handle SMSG_UPDATE_ACCOUNT_DATA packet data here
            //Console.WriteLine("Handling SMSG_UPDATE_ACCOUNT_DATA");
        }
    }
}
