using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Core;
using Titan.Bot.Account;
using Titan.Bot.Mode;
using Titan.Logging;
using Titan.Util;

namespace Titan.Bot.Threads
{
    public class ThreadManager
    {

        private Logger _log = LogCreator.Create();

        private Dictionary<TitanAccount, Task> _taskDic = new Dictionary<TitanAccount, Task>();

        private int _count; // Amount of accounts that successfully reported or commended

        public void Start(BotMode mode, TitanAccount acc, uint target, ulong matchId)
        {
            acc.Feed(new Info
            {
                Mode = mode,
                Target = target,
                MatchID = matchId
            });

            _count = 0;

            _log.Debug("Starting {Mode} thread for {Target} in match {MatchId} " +
                       "using account {Account}.", mode.ToString().ToLower() + "ing", target, matchId, acc.JsonAccount.Username);

            _taskDic.Add(acc, Task.Run(() =>
            {
                var timedOut = false;

                try
                {
                    acc.StartTick = DateTime.Now.Ticks;
                    var result = WaitFor<Result>.Run(TimeSpan.FromSeconds(60), acc.Start);

                    switch(result)
                    {
                        case Result.Success:
                            _log.Information("Successfully sent a {Mode} to {Target} with account {Account}.",
                                mode.ToString().ToLower(), target, acc.JsonAccount.Username);
                            _count++;
                            break;
                        case Result.AlreadyLoggedInSomewhereElse:
                            _log.Error("Could not report with account {Account}. The account is " +
                                         "already logged in somewhere else.", acc.JsonAccount.Username);
                            break;
                        case Result.AccountBanned:
                            _log.Warning("Account {Account} has VAC or game bans on record. The report may " +
                                         "have not been submitted.");
                            _count++;
                            break;
                        case Result.TimedOut:
                            _log.Error("Processing thread for {Account} has timed out.");
                            break;
                    }
                }
                catch (TimeoutException ex)
                {
                    var timeSpent = new DateTime(DateTime.Now.Ticks - acc.StartTick);

                    if(timeSpent.Second > 60)
                    {
                        _log.Error("Connection to account {Account} timed out. It was not possible to " +
                                   "report the target after {Timespan} seconds.", acc.JsonAccount.Username, timeSpent.Second);
                        timedOut = true;
                    }
                    else
                    {
                        _log.Error("The {Mode} thread for {Account} already timed out after {Time} seconds.",
                            mode.ToString().ToLower(), acc.JsonAccount.Username, timeSpent.Second);
                    }
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
                Info info = null;

                foreach(var pair in _taskDic)
                {
                    while(!pair.Value.IsCompleted)
                    {
                        Thread.Sleep(TimeSpan.FromSeconds(5));
                    }

                    if(info == null) info = pair.Key._info;
                }

                if(info != null)
                {
                    _log.Information("Successfully {mode} {Target} {Count}x.",
                        info.Mode.ToString().ToLower() + "ed", info.Target, _count);
                }
            });
        }

    }
}