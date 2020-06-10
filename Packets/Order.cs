using LiteNetLib.Utils;

namespace MultiplayerTest.Packets
{
    public class Order : INetSerializable
    {
        public int Player { get; set; }
        public double TargetX { get; set; }
        public double TargetY { get; set; }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(Player);
            writer.Put(TargetX);
            writer.Put(TargetY);
        }

        public void Deserialize(NetDataReader reader)
        {
            Player = reader.GetInt();
            TargetX = reader.GetDouble();
            TargetY = reader.GetDouble();
        }
    }
}
