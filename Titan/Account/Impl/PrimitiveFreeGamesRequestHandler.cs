using System;
using SteamKit2;
using SteamKit2.Internal;

namespace Titan.Account.Impl
{
    public class PrimitiveFreeGamesRequestHandler : ClientMsgHandler
    {

        private Action<ClientMsgProtobuf<CMsgClientRequestFreeLicenseResponse>> _action;
        
        public PrimitiveFreeGamesRequestHandler(Action<ClientMsgProtobuf<CMsgClientRequestFreeLicenseResponse>> action)
        {
            _action = action;
        }
        
        public override void HandleMsg(IPacketMsg packetMsg)
        {
            if (packetMsg.MsgType == EMsg.ClientRequestFreeLicenseResponse)
            {
                var payload = new ClientMsgProtobuf<CMsgClientRequestFreeLicenseResponse>(packetMsg.MsgType);

                _action(payload);
            }
        }
        
    }
}
