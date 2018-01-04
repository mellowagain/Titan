using Serilog.Core;
using SteamKit2;
using Titan.Account;

namespace Titan.Logging
{
    public class TitanListener : IDebugListener
    {

        private Logger _log = LogCreator.CreateDebugLogger("SteamKit Debug Listener");
        
        public void WriteLine(string category, string msg)
        {
            _log.Debug("{category}: {message}", category, msg);
        }
        
    }
}
