using WoWSlimClient.Client;
using WoWSlimClient.Models;

class Program
{
    static WoWClient WoWClient;
    static void Main(string[] args)
    {
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

            if (WoWClient.IsLoggedIn)
            {
                List<Realm> realms = WoWClient.GetRealmList();

                if (realms.Count > 0)
                {
                    WoWClient.SelectRealm(realms[0]);

                    if (WoWClient.HasRealmSelected)
                    {
                        List<CharacterSelect> characterSelects = WoWClient.GetCharacterListFromRealm();

                        if (characterSelects.Count > 0)
                            WoWClient.EnterWorld(characterSelects[0].Guid);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }

        Console.ReadKey();
    }
}
