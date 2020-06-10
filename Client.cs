using System;
using System.Threading;
using LiteNetLib;
using LiteNetLib.Utils;
using MultiplayerTest.Packets;

namespace MultiplayerTest
{
    public class Client
    {
        private NetManager manager;
        private Thread thread;
        private NetPeer server;
        private NetDataWriter writer;

        public event Action<int> GameStarted;
        public event Action<Order> OrderReceived;

        public void Connect(string endpoint, int port)
        {
            if (manager != null)
            {
                throw new InvalidOperationException();
            }

            var listener = new EventBasedNetListener();
            manager = new NetManager(listener);
            manager.Start();
            manager.Connect(endpoint, port, nameof(MultiplayerTest));

            listener.PeerConnectedEvent += peer => server = peer;

            var processor = new NetPacketProcessor();
            processor.RegisterNestedType<Order>(() => new Order());
            processor.SubscribeReusable<OrderPacket>(packet =>
            {
                foreach (var order in packet.Orders)
                {
                    OrderReceived?.Invoke(order);
                }
            });

            listener.NetworkReceiveEvent += (fromPeer, dataReader, deliveryMethod) =>
            {
                var type = (PacketType)dataReader.GetByte();
                switch (type)
                {
                    case PacketType.StartGame:
                        var players = dataReader.GetInt();
                        GameStarted?.Invoke(players);
                        break;
                    case PacketType.Orders:
                        processor.ReadAllPackets(dataReader);
                        break;
                }

                dataReader.Recycle();
            };

            writer = new NetDataWriter();

            thread = new Thread(Run)
            {
                IsBackground = true
            };
            
            thread.Start();
        }

        public void SendOrder(double targetX, double targetY)
        {
            writer.Reset();
            writer.Put(targetX);
            writer.Put(targetY);
            
            server.Send(writer, DeliveryMethod.ReliableOrdered);
        }

        public void Disconnect()
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

        private void Run()
        {
            while (true)
            {
                manager.PollEvents();
                Thread.Sleep(100);
            }
        }
    }
}
