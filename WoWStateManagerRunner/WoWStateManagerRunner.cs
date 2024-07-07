using System.Diagnostics;
using WoWStateManager;
using static WinProcessImports.WinImports;

public class WoWStateManagerRunner
{
    public static void Main(string[] args)
    {
        WorldStateManagerServer woWStateManager = new();
        woWStateManager.Start();

        //MaNGOSCommandClient maNGOS = new(IPAddress.Loopback);
        //string response = maNGOS.SendCommand("account create lrhodes404 Rockydog11!!..");
        //Console.WriteLine(response);

        Console.ReadKey();
    }
}