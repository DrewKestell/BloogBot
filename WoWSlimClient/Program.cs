using Newtonsoft.Json;
using System;
using WoWSlimClient;
using WoWSlimClient.Client;
using WoWSlimClient.Manager;
using WoWSlimClient.Models;

class Program
{
    static WoWClient WoWClient;
    static string accountName = string.Empty;
    static string password = "password";
    static string ipAddress = "127.0.0.1";
    static int port = 3724;
    static void Main(string[] args)
    {
        ObjectManager.Instance.Initialize();

        WoWEventHandler.Instance.OnLoginConnect += Instance_OnLoginConnect;
        WoWEventHandler.Instance.OnLoginSuccess += Instance_OnLoginSuccess;
        WoWEventHandler.Instance.OnLoginFailure += Instance_OnLoginFailure;
        WoWEventHandler.Instance.OnCharacterListLoaded += Instance_OnCharacterListLoaded;

        try
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (i % 2 == 0)
                {
                    switch (args[i])
                    {
                        case "-a":
                            accountName = args[i + 1];
                            break;
                        case "-p":
                            password = args[i + 1];
                            break;
                        case "-ip":
                            ipAddress = args[i + 1];
                            break;
                        case "-port":
                            port = int.Parse(args[i + 1]);
                            break;
                    }
                }
            }

            if (string.IsNullOrEmpty(accountName))
            {
                throw new Exception("No Username provided");
            }

            WoWClient = new WoWClient(ipAddress, port);
            WoWClient.ConnectToLogin();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex}");
        }

        Console.ReadKey();
        try
        {
            Console.WriteLine(JsonConvert.SerializeObject(ObjectManager.Instance.Objects));
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
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[Main]No realm selected");
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("[Main]No realms listed");
        }
    }

    private static void Instance_OnLoginFailure(object? sender, EventArgs e)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("[Main]Failed to login to WoW server");
    }

    private static async void Instance_OnCharacterListLoaded(object? sender, EventArgs e)
    {
        await Task.Delay(100);

        if (ObjectManager.Instance.CharacterSelects.Count > 0)
            WoWClient.EnterWorld(ObjectManager.Instance.CharacterSelects[0].Guid);
    }
}
