using System;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.Util;

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
                _lastReceivedPacket = DateTime.Now.ToEpochTime();
                
                // TODO: Implement handler
            }
            else
            {
                _log.Error("Received packet payload from Steam network: {null}", null);
            }
        }
        
    }
}
