
using GameData.Core.Enums;

namespace WoWSharpClient.Handlers
{
    public class AccountDataHandler(WoWSharpObjectManager objectManager)
    {
        private readonly WoWSharpEventEmitter _woWSharpEventEmitter = objectManager.EventEmitter;
        private readonly WoWSharpObjectManager _objectManager = objectManager;
        public void HandleAccountData(Opcode opcode, byte[] data)
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
