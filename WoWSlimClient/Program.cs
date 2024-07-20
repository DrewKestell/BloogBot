using WoWSlimClient;
using WoWSlimClient.Client;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

class Program
{
    static WoWClient WoWClient;
    static string accountName;
    static string password;
    static string ipAddress;
    static int port;
    static void Main(string[] args)
    {
        ObjectManager.Instance.Initialize();

        WoWEventHandler.Instance.OnLoginConnect += Instance_OnLoginConnect;
        WoWEventHandler.Instance.OnLoginSuccess += Instance_OnLoginSuccess;
        WoWEventHandler.Instance.OnLoginFailure += Instance_OnLoginFailure; 
        WoWEventHandler.Instance.OnCharacterListLoaded += Instance_OnCharacterListLoaded;

        try
        {
            accountName = "ORWR1";
            password = "password";
            ipAddress = "127.0.0.1";
            port = 3724;

            for (int i = 0; i < args.Length; i++)
            {
                // Handle args for the above variables
            }

            WoWClient = new WoWClient(ipAddress, port);
            WoWClient.ConnectToLogin();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }

        Console.ReadKey();
    }

    private static void Instance_OnLoginConnect(object? sender, EventArgs e)
    {
        WoWClient.Login(accountName, password);
    }

    private static void Instance_OnLoginSuccess(object? sender, EventArgs e)
    {
        List<Realm> realms = WoWClient.GetRealmList();

        if (realms.Count > 0)
        {
            WoWClient.SelectRealm(realms[0]);

            if (ObjectManager.Instance.HasRealmSelected)
            {
                WoWClient.RefreshCharacterSelects();
            }
            else
            {
                Console.WriteLine("No realm selected");
            }
        }
        else
        {
            Console.WriteLine("No realms listed");
        }
    }

    private static void Instance_OnLoginFailure(object? sender, EventArgs e)
    {
        Console.WriteLine("Failed to login to WoW server");
    }

    private static async void Instance_OnCharacterListLoaded(object? sender, EventArgs e)
    {
        await Task.Delay(100);

        if (ObjectManager.Instance.CharacterSelects.Count > 0)
            WoWClient.EnterWorld(ObjectManager.Instance.CharacterSelects[0].Guid);
    }
}
