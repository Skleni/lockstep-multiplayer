using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace MultiplayerTest.HolePunchServer
{
    public class Peer
    {
        public IPEndPoint InternalAddress { get; }
        public IPEndPoint ExternalAddress { get; }
        public DateTime RefreshTime { get; private set; }

        public void Refresh()
        {
            RefreshTime = DateTime.UtcNow;
        }

        public Peer(IPEndPoint internalAddress, IPEndPoint externalAddress)
        {
            Refresh();
            InternalAddress = internalAddress;
            ExternalAddress = externalAddress;
        }
    }
}
