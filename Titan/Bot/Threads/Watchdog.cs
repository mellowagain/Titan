using System.Collections.Generic;
using System.Threading;
using Serilog.Core;
using Titan.Logging;

namespace Titan.Bot.Threads
{
    public class Watchdog
    {

        private static Logger _log = LogCreator.Create();

        public static void OverwatchThreads()
        {
            foreach(var pair in ThreadManager.Dictionary)
            {
                var thread = pair.Value;

                while(thread.IsAlive)
                {
                    _log.Debug("Thread {Thread} is still alive...", thread.Name);
                }

                // TODO: Implement a better Watchdog
            }
        }

    }
}
