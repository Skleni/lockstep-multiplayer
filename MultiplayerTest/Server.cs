using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using MultiplayerTest.Packets;

namespace MultiplayerTest
{
    class Server
    {
        private NetManager manager;
        private Thread thread;
        private bool gameStarted;
        private List<Order> orders = new List<Order>();

        public int MaxConnections { get; set; } = 4;

        public event Action<int> PlayerConnected;
        public event Action<Order> OrderReceived;

        public void Start(int port)
        {
            if (manager != null)
            {
                throw new InvalidOperationException();
            }

            var natPunchListener = new EventBasedNatPunchListener();
            natPunchListener.NatIntroductionSuccess += (endpoint, addressType, token) =>
            {
                Debug.WriteLine($"Server: Introduced to {endpoint} ({addressType})");
                //manager.Connect(endpoint, nameof(MultiplayerTest));
                //if (addressType == NatAddressType.External)
                //{
                //    manager.Connect(endpoint, nameof(MultiplayerTest));
                //}
            };

            var listener = new EventBasedNetListener();

            manager = new NetManager(listener)
            {
                NatPunchEnabled = true
            };

            manager.NatPunchModule.Init(natPunchListener);
            manager.Start();

            manager.NatPunchModule.SendNatIntroduceRequest("multiplayer-test.westeurope.azurecontainer.io", 5463, "Test");

            listener.ConnectionRequestEvent += request =>
            {
                if (manager.GetPeersCount(ConnectionState.Any) < MaxConnections)
                {
                    request.AcceptIfKey(nameof(MultiplayerTest));
                }
                else
                {
                    request.Reject();
                }
            };

            listener.PeerConnectedEvent += peer =>
            {
                Debug.WriteLine($"Peer {peer.Id} connected: {peer.EndPoint}");
                PlayerConnected?.Invoke(peer.Id);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                orders.Add(new Order()
                {
                    Player = fromPeer.Id,
                    TargetX = dataReader.GetDouble(),
                    TargetY = dataReader.GetDouble(),
                });
            };

            thread = new Thread(Run)
            {
                IsBackground = true
            };

            thread.Start();
        }

        public void StartGame()
        {
            if (manager == null)
            {
                throw new InvalidOperationException();
            }

            var writer = new NetDataWriter();
            writer.Put((byte)PacketType.StartGame);
            writer.Put(manager.ConnectedPeersCount + 1);
            manager.SendToAll(writer, DeliveryMethod.ReliableOrdered);
            gameStarted = true;
        }

        public void SendOrder(double targetX, double targetY)
        {
            orders.Add(new Order()
            {
                Player = manager.ConnectedPeersCount,
                TargetX = targetX,
                TargetY = targetY,
            });
        }

        private void Run()
        {
            var turn = 1;
            var writer = new NetDataWriter();
            var processor = new NetPacketProcessor();
            processor.RegisterNestedType<Order>(() => new Order());

            while (manager != null)
            {
                manager.NatPunchModule.PollEvents();
                manager.PollEvents();
                Thread.Sleep(100);

                if (gameStarted)
                {
                    writer.Reset();
                    writer.Put((byte)PacketType.Orders);

                    var orderPacket = new OrderPacket()
                    {
                        Turn = turn,
                        Orders = orders.ToArray()
                    };

                    processor.Write(writer, orderPacket);
                    manager.SendToAll(writer, DeliveryMethod.ReliableOrdered);

                    foreach (var order in orders)
                    {
                        OrderReceived?.Invoke(order);
                    }

                    orders.Clear();

                    turn++;
                }
            }
        }

        public void Stop()
        {
            if (manager == null)
            {
                throw new InvalidOperationException();
            }

            manager.Stop();
            manager = null;
            thread.Join();
            thread = null;
        }
    }
}
