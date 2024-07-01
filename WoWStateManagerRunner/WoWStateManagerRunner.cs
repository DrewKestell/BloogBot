using MaNGOSDBDomain.Client;
using System.Net;
using WoWStateManager;

public class WoWStateManagerRunner
{
    public static void Main(string[] args)
    {
        WorldStateManagerServer woWStateManager = new();
        woWStateManager.Start();

        //MaNGOSCommandClient maNGOS = new(IPAddress.Parse("127.0.0.1"));
        //string response = maNGOS.SendCommand("account create lrhodes404 Rockydog11!!..");
        //Console.WriteLine(response);

        Console.ReadKey();
    }
}