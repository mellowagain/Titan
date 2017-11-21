using System;
using Serilog.Core;
using SteamKit2;
using SteamKit2.GC.CSGO.Internal;
using Titan.Logging;

namespace Titan.Account
{
    public class TitanHandler : ClientMsgHandler
    {

        private Logger _log = LogCreator.Create();

        private long _lastReceivedPacket;
        
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg != null)
            {
                _lastReceivedPacket = DateTime.Now.Ticks;
                
                // TODO: Implement handler
            }
            else
            {
                _log.Error("Received packet payload from Steam network: {null}", null);
            }
        }
        
    }
}
