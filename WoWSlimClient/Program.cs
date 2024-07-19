using WoWSlimClient.Client;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

class Program
{
    static WoWClient WoWClient;
    static void Main(string[] args)
    {
        OpCodeDispatcher.Instance.OnCharacterListLoaded += Instance_OnCharacterListLoaded;
        OpCodeDispatcher.Instance.OnPlayerInit   += Instance_OnPlayerInit;

        try
        {
            string accountName = "ORWR1";
            string password = "password";
            string ipAddress = "127.0.0.1";
            int port = 3724;

            for (int i = 0; i < args.Length; i++)
            {
                // Handle args for the above variables
            }

            WoWClient = new WoWClient(ipAddress, port);
            WoWClient.Login(accountName, password);

            if (ObjectManager.Instance.IsLoggedIn)
            {
                List<Realm> realms = WoWClient.GetRealmList();

                if (realms.Count > 0)
                {
                    WoWClient.SelectRealm(realms[0]);

                    if (ObjectManager.Instance.HasRealmSelected)
                    {
                        WoWClient.RefreshCharacterSelects();
                    } else
                    {
                        Console.WriteLine("No realm selected");
                    }
                }
                else
                {
                    Console.WriteLine("No realms listed");
                }
            } else
            {
                Console.WriteLine("Failed to login to WoW server");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }

        Console.ReadKey();
    }

    private static void Instance_OnPlayerInit(object? sender, EventArgs e)
    {
        WoWClient.StartServerPing();
    }

    private static async void Instance_OnCharacterListLoaded(object? sender, EventArgs e)
    {
        await Task.Delay(100);

        if (ObjectManager.Instance.CharacterSelects.Count > 0)
            WoWClient.EnterWorld(ObjectManager.Instance.CharacterSelects[0].Guid);
    }
}
