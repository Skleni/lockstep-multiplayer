using System;

namespace MultiplayerTest.HolePunchServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var port = int.Parse(args[0]);

            Console.WriteLine("Local IPs:");
            foreach (var endpoint in LiteNetLib.NetUtils.GetLocalIpList(LiteNetLib.LocalAddrType.IPv4))
            {
                Console.WriteLine(endpoint);
            }

            Console.WriteLine($"Listening on port {port}...");
            new Server().Run(port);
        }
    }
}
