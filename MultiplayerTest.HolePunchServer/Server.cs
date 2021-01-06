using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using LiteNetLib;

namespace MultiplayerTest.HolePunchServer
{
    public class Server : INatPunchListener
    {
        private readonly IDictionary<string, Peer> peers = new Dictionary<string, Peer>();
        private readonly IList<string> peersToRemove = new List<string>();

        private readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

        private NetManager manager;

        public void Run(int port)
        {
            var listener = new EventBasedNetListener();

            manager = new NetManager(listener)
            {
                NatPunchEnabled = true
            };

            manager.Start(port);
            manager.NatPunchModule.Init(this);

            while (true)
            {
                DateTime now = DateTime.UtcNow;
                manager.NatPunchModule.PollEvents();

                foreach (var peer in peers)
                {
                    if (now - peer.Value.RefreshTime > Timeout)
                    {
                        peersToRemove.Add(peer.Key);
                    }
                }

                //while (peersToRemove.Count > 0)
                //{
                //    peers.Remove(peersToRemove[0]);
                //    peersToRemove.RemoveAt(0);
                //}

                Thread.Sleep(100);
            }
        }

        public void OnNatIntroductionRequest(IPEndPoint localEndPoint, IPEndPoint remoteEndPoint, string token)
        {
            Console.WriteLine($"Nat introduction request (local: {localEndPoint}, remote: {remoteEndPoint})");

            if (peers.TryGetValue(token, out var peer))
            {
                if (peer.InternalAddress.Equals(localEndPoint) &&
                    peer.ExternalAddress.Equals(remoteEndPoint))
                {
                    peer.Refresh();
                    return;
                }

                Console.WriteLine($"Peer with token {token} found, introducing...");

                manager.NatPunchModule.NatIntroduce(
                    peer.InternalAddress,
                    peer.ExternalAddress,
                    localEndPoint,
                    remoteEndPoint,
                    token);

                peers.Remove(token);
            }
            else
            {
                Console.WriteLine($"Did not find token {token}, waiting...");

                peers[token] = new Peer(localEndPoint, remoteEndPoint);
            }
        }

        public void OnNatIntroductionSuccess(IPEndPoint targetEndPoint, NatAddressType type, string token)
        {
            Console.WriteLine($"Nat introduction success for {targetEndPoint} ({type})");
        }
    }
}
