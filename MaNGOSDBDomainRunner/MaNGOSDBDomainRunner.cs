using MaNGOSDBDomain;

namespace MaNGOSDBDomainApp
{
    public class MaNGOSDBDomainRunner
    {
        public static void Main(string[] args)
        {
            MaNGOSDBSocketServer maNGOSDBSocketServer = new();
            maNGOSDBSocketServer.Start();

            Console.ReadKey();
        }
    }
}