using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StreamJsonRpc;

namespace BloogBot.UI
{
    internal class BotService
    {
        BotService(MainViewModel mainViewModel)
        {
            this.mainViewModel = mainViewModel;
        }

        public Task<string> Start()
        {
            mainViewModel.RpcLogin();
            return Task.FromResult("ok");
        }

        public async Task<string> Ping()
        {
            // Run something on the main thread to make sure that our message loop is working.
            return await Task.Run(() => ThreadSynchronizer.RunOnMainThread(() =>
                {
                    return "ok";
                }));
        }

        public Task<bool> IsRunning()
        {
            return Task.FromResult(mainViewModel.CurrentBot.Running());
        }

        MainViewModel mainViewModel;

        public static async Task Run(MainViewModel mainViewModel)
        {
            while (true)
            {
                using (var serverStream = new NamedPipeServerStream(
                    "BloogBotPipe",
                    PipeDirection.InOut,
                    1,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous))
                {
                    await serverStream.WaitForConnectionAsync();

                    var botService = new BotService(mainViewModel);
                    var jsonRpc = JsonRpc.Attach(serverStream, botService);

                    await jsonRpc.Completion;
                }
            }
        }
    }
}
