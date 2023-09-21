using BloogBot.Game.Objects;
using System;
using System.Runtime.InteropServices;

namespace BloogBot
{
    public sealed class BotDomainManager : AppDomainManager//, IWoWInstance
    {
        public BotDomainManager() { }

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            base.InitializationFlags = AppDomainManagerInitializationOptions.RegisterWithHost;
        }

        //[return: MarshalAs(UnmanagedType.LPWStr)]
        //public string HelloWorld([MarshalAs(UnmanagedType.LPWStr)] string name)
        //{
        //    return "Hello " + name;
        //}
    }
}
