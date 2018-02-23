using System;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.Util;

namespace Titan.Account
{
    public class TitanHandler : ClientMsgHandler
    {

        private Logger _log = LogCreator.CreateDebugFileLogger("Titan Handler");
        public DateTime LastPacket;
        
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg != null)
            {
                LastPacket = DateTime.Now;

                switch (packetMsg.MsgType)
                {
                    case EMsg.ClientSharedLibraryLockStatus:
                        // TODO: Handle CS:GO shared over family sharing (It doesn't work for report botting!)
                        break;
                }
                
                _log.Debug("Recv'd msg: {msg} Content: {@content}", packetMsg, packetMsg);
                return;
            }
            
            _log.Error("Received null package: {package}", nameof(packetMsg));
        }
        
    }
}
