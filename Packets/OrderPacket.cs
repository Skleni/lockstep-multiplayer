using System;
using System.Collections.Generic;
using System.Text;

namespace MultiplayerTest.Packets
{
    public class OrderPacket
    {
        public int Turn { get; set; }
        public Order[] Orders { get; set; }
    }
}
