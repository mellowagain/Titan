using System.Collections.Generic;
using System.Threading;
using log4net;
using Titan.Core.Accounts;

namespace Titan.Core
{
    public class ThreadManager
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(ThreadManager));

        public static Dictionary<Account, Thread> Dictionary = new Dictionary<Account, Thread>();

        public static void StartThread(Account acc, uint target, ulong matchId)
        {
            acc.Target = target;
            acc.MatchID = matchId;

            Log.DebugFormat("Starting reporting thread for {0}.", acc.Json.Username);
            var thread = new Thread(acc.Report);
            thread.Start();

            Dictionary.Add(acc, thread);
        }

        public static void Abort(Account acc)
        {
            Thread output;

            if(Dictionary.TryGetValue(acc, out output))
            {
                output.Abort();
                Log.DebugFormat("The reporting thread for {0} has been aborted.", acc.Json.Username);
            }
            else
            {
                Log.ErrorFormat("Could not find thread for {0}, but it has tried to be aborted!", acc.Json.Username);
            }
        }

    }
}