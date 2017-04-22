using System.Collections.Generic;
using System.Threading;
using Serilog.Core;
using Titan.Bot.Mode;
using Titan.Logging;

namespace Titan.Bot.Threads
{
    public class ThreadManager
    {

        private static Logger _log = LogCreator.Create();

        public static Dictionary<Account, Thread> Dictionary = new Dictionary<Account, Thread>();

        public static void StartThread(Account acc, uint target, ulong matchId, BotMode mode)
        {
            acc.Target = target;
            acc.MatchID = matchId;
            acc.Mode = mode;

            _log.Debug("Starting reporting thread for {0}.", acc.Json.Username);
            var thread = new Thread(acc.Process);
            thread.Start();

            Dictionary.Add(acc, thread);
        }

        public static void Abort(Account acc)
        {
            Thread output;

            if(Dictionary.TryGetValue(acc, out output))
            {
                output.Abort();
                _log.Debug("The reporting thread for {0} has been aborted.", acc.Json.Username);
            }
            else
            {
                _log.Error("Could not find thread for {0}, but it has tried to be aborted!", acc.Json.Username);
            }
        }

    }
}