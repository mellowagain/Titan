using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Core;
using Titan.Bot.Mode;
using Titan.Logging;
using Titan.Util;

namespace Titan.Bot.Threads
{
    public class ThreadManager
    {

        private Logger _log = LogCreator.Create();

        private Dictionary<Account, Task> _taskDic = new Dictionary<Account, Task>();

        public void Start(BotMode mode, Account acc, uint target, ulong matchId)
        {
            SetArguments(acc, mode, target, matchId);

            _log.Debug("Starting reporting thread for {Target} in {MatchId} " +
                       "using account {Account}.", target, matchId, acc.Json.Username);

            _taskDic.Add(acc, Task.Run(() =>
            {
                var timedOut = false;

                try
                {
                    var result = WaitFor<bool>.Run(TimeSpan.FromSeconds(30), acc.Process);

                    if(!result)
                    {
                        _log.Error("Could not report {Target} with account {Account}.",
                            target, acc.Json.Username);
                    }
                }
                catch (TimeoutException ex)
                {
                    _log.Error("Connection to account {Account} timed out. It was not possible " +
                               "to report the target after {Timespan} seconds. Please check your " +
                               "internet connection and if steam is online.", acc.Json.Username, ex.Message);
                    timedOut = true;
                }
                finally
                {
                    if(timedOut)
                    {
                        acc.Stop();
                    }

                    _taskDic.Remove(acc);
                }
            }));
        }

        /// <todo>
        /// This method requires reworking as it doesn't work.
        /// FIXME: 01.04.17 19:09
        /// </todo>
        public void StartWatchdog()
        {
            Task.Run(() =>
            {
                foreach(var pair in _taskDic)
                {
                    while(!pair.Value.IsCompleted)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }
                }

                _log.Information("Successfully finished botting.");
            });
        }

        private void SetArguments(Account acc, BotMode mode, uint target, ulong matchId)
        {
            acc.Mode = mode;
            acc.Target = target;
            acc.MatchID = matchId;
        }

    }
}