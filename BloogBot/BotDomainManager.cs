using System;

namespace BloogBot
{
    public sealed class BotDomainManager : AppDomainManager//, IWoWInstance
    {
        public BotDomainManager() { }

        public override void InitializeNewDomain(AppDomainSetup appDomainInfo)
        {
            InitializationFlags = AppDomainManagerInitializationOptions.RegisterWithHost;
        }

        //[return: MarshalAs(UnmanagedType.LPWStr)]
        //public string HelloWorld([MarshalAs(UnmanagedType.LPWStr)] string name)
        //{
        //    return "Hello " + name;
        //}
    }
}
