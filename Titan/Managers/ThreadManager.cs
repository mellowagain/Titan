using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Quartz.Util;
using Serilog.Core;
using Titan.Account;
using Titan.Logging;
using Titan.Meta;
using Titan.Util;

namespace Titan.Managers
{
    public class ThreadManager
    {
        private Logger _log = LogCreator.Create();

        private Dictionary<TitanAccount, Task> _taskDic = new Dictionary<TitanAccount, Task>();

        private int _successCount; // Amount of accounts that successfully reported or commended
        private int _failCount; // Amount of accounts that failed to report or commend
        private int _count; // Amount of accounts that started reporting / commending

        public void StartReport(TitanAccount account, ReportInfo info)
        {
            if (_taskDic.ContainsKey(account))
            {
                _log.Warning("Account is already reporting / commending / idling. Aborting forcefully!");

                FinishBotting(account);
            }

            account.FeedReportInfo(info);

            _successCount = 0;
            _failCount = 0;
            _count = 0;

            _log.Debug("Starting reporting thread for {Target} in {Match} using account {Account}.",
                info.SteamID, info.MatchID, account.JsonAccount.Username);

            _taskDic.Add(account, Task.Run(() =>
            {
                var timedOut = false;

                try
                {
                    account.StartEpoch = DateTime.Now.ToEpochTime();

                    // Timeout on Sentry Account: 3min (so the user has enough time to input the 2FA code), else 60sec.
                    var origin = Task.Run(() => account.Start());
                    var result = origin.RunUntil(account.JsonAccount.Sentry
                        ? TimeSpan.FromMinutes(3)
                        : TimeSpan.FromSeconds(60));
                    switch (result.Result)
                    {
                        case Result.Success:
                            _successCount++;
                            break;
                        case Result.AlreadyLoggedInSomewhereElse:
                            _log.Error("Could not report with account {Account}. The account is " +
                                       "already logged in somewhere else.", account.JsonAccount.Username);
                            _failCount++;
                            break;
                        case Result.AccountBanned:
                            _log.Warning("Account {Account} has VAC or game bans on record. The report may " +
                                         "have not been submitted.", account.JsonAccount.Username);
                            _failCount++;
                            break;
                        case Result.NoMatches:
                            _log.Error("Could not receive match information for {Account}: User is not in live match.",
                                account._liveGameInfo.SteamID.ConvertToUInt64());
                            _failCount++;
                            break;
                        case Result.TimedOut:
                            _log.Error("Processing thread for {Account} has timed out.");
                            _failCount++;
                            break;
                        case Result.SentryRequired:
                            _log.Error("The account has 2FA enabled. Please set {sentry} to {true} " +
                                       "in the accounts.json file.", "sentry", true);
                            _failCount++;
                            break;
                        case Result.RateLimit:
                            _log.Error("The Steam Rate Limit has been reached. Please try again in a " +
                                       "few minutes.");
                            _failCount++;
                            break;
                        case Result.Code2FAWrong:
                            _log.Error("The provided SteamGuard code was wrong. Please retry.");
                            _failCount++;
                            break;
                        default:
                            _failCount++;
                            break;
                    }

                    if (_count - _successCount - _failCount == 0)
                    {
                        if (_successCount == 0)
                        {
                            _log.Information("FAIL! Titan was not able to report target {Target}.",
                                info.SteamID.ConvertToUInt64());

                            Titan.Instance.UIManager.SendNotification(
                                "Titan", " was not able to report your target."
                            );
                        }
                        else
                        {
                            _log.Information(
                                "SUCCESS! Titan has successfully sent {Amount} out of {Fail} reports to target {Target}.",
                                _successCount, _count, info.SteamID.ConvertToUInt64());

                            Titan.Instance.UIManager.SendNotification(
                                "Titan", _successCount + " reports have been successfully sent!"
                            );
                        }
                        if (Titan.Instance.ParsedObject != null)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
                catch (TimeoutException)
                {
                    var timeSpent = DateTime.Now.Subtract(account.StartEpoch.ToDateTime());

                    _log.Error("Connection to account {Account} timed out. It was not possible to " +
                               "report the target after {Timespan} seconds.", account.JsonAccount.Username,
                        timeSpent.Seconds);
                    timedOut = true;
                }
                finally
                {
                    if (timedOut)
                    {
                        account.Stop();
                    }

                    _taskDic.Remove(account);
                }
            }));
        }

        public void StartCommend(TitanAccount account, CommendInfo info)
        {
            if (_taskDic.ContainsKey(account))
            {
                _log.Warning("Account is already reporting / commending / idling. Aborting forcefully!");

                FinishBotting(account);
            }

            account.FeedCommendInfo(info);

            _successCount = 0;
            _failCount = 0;
            _count = 0;

            _log.Debug("Starting commending thread for {Target} using account {Account}.",
                info.SteamID, account.JsonAccount.Username);

            _taskDic.Add(account, Task.Run(() =>
            {
                var timedOut = false;

                try
                {
                    account.StartEpoch = DateTime.Now.ToEpochTime();

                    // Timeout on Sentry Account: 3min (so the user has enough time to input the 2FA code), else 60sec.
                    var origin = Task.Run(() => account.Start());
                    var result = origin.RunUntil(account.JsonAccount.Sentry
                        ? TimeSpan.FromMinutes(3)
                        : TimeSpan.FromSeconds(60));
                    _count++;

                    switch (result.Result)
                    {
                        case Result.Success:
                            _successCount++;
                            break;
                        case Result.AlreadyLoggedInSomewhereElse:
                            _log.Error("Could not commend with account {Account}. The account is " +
                                       "already logged in somewhere else.", account.JsonAccount.Username);
                            _failCount++;
                            break;
                        case Result.AccountBanned:
                            _log.Warning("Account {Account} has VAC or game bans on record. The report may " +
                                         "have not been submitted.", account.JsonAccount.Username);
                            _failCount++;
                            break;
                        case Result.TimedOut:
                            _log.Error("Processing thread for {Account} has timed out.");
                            _failCount++;
                            break;
                        case Result.SentryRequired:
                            _log.Error("The account has 2FA enabled. Please set {sentry} to {true} " +
                                       "in the accounts.json file.", "sentry", true);
                            _failCount++;
                            break;
                        case Result.RateLimit:
                            _log.Error("The Steam Rate Limit has been reached. Please try again in a " +
                                       "few minutes.");
                            _failCount++;
                            break;
                        case Result.Code2FAWrong:
                            _log.Error("The provided SteamGuard code was wrong. Please retry.");
                            _failCount++;
                            break;
                        default:
                            _failCount++;
                            break;
                    }

                    if (_count - _successCount - _failCount == 0)
                    {
                        if (_successCount == 0)
                        {
                            _log.Information("FAIL! Titan was not able to commend target {Target}.",
                                info.SteamID.ConvertToUInt64());

                            Titan.Instance.UIManager.SendNotification(
                                "Titan", " was not able to commend your target."
                            );
                        }
                        else
                        {
                            _log.Information(
                                "SUCCESS! Titan has successfully sent {Amount} out of {Fail} commends to target {Target}.",
                                _successCount, _count, info.SteamID.ConvertToUInt64());

                            Titan.Instance.UIManager.SendNotification(
                                "Titan", _successCount + " commends have been successfully sent!"
                            );
                        }
                        if (Titan.Instance.ParsedObject != null)
                        {
                            Environment.Exit(0);
                        }
                    }
                }
                catch (TimeoutException)
                {
                    var timeSpent = DateTime.Now.Subtract(account.StartEpoch.ToDateTime());

                    _log.Error("Connection to account {Account} timed out. It was not possible to " +
                               "commend the target after {Timespan} seconds.", account.JsonAccount.Username,
                        timeSpent.Seconds);
                    timedOut = true;
                }
                finally
                {
                    if (timedOut)
                    {
                        account.Stop();
                    }

                    _taskDic.Remove(account);
                }
            }));
        }

        public void StartMatchResolving(TitanAccount account, LiveGameInfo info)
        {
            if (_taskDic.ContainsKey(account))
            {
                _log.Warning("Account is already reporting / commending / idling. Aborting forcefully!");

                FinishBotting(account);
            }

            account.FeedLiveGameInfo(info);

            _log.Debug("Starting Match ID resolving thread for {Target} using account {Account}.",
                info.SteamID, account.JsonAccount.Username);

            _taskDic.Add(account, Task.Run(() =>
            {
                var timedOut = false;

                try
                {
                    account.StartEpoch = DateTime.Now.ToEpochTime();

                    // Timeout on Sentry Account: 3min (so the user has enough time to input the 2FA code), else 60sec.
                    var origin = Task.Run(() => account.Start());
                    var result = origin.RunUntil(account.JsonAccount.Sentry
                        ? TimeSpan.FromMinutes(3)
                        : TimeSpan.FromSeconds(60));

                    switch (result.Result)
                    {
                        case Result.Success:
                            _successCount++;
                            break;
                        case Result.AlreadyLoggedInSomewhereElse:
                            _log.Error("Could not resolve Match ID with account {Account}. The account is " +
                                       "already logged in somewhere else.", account.JsonAccount.Username);
                            break;
                        case Result.TimedOut:
                            _log.Error("Processing thread for {Account} has timed out.");
                            break;
                        case Result.SentryRequired:
                            _log.Error("The account has 2FA enabled. Please set {sentry} to {true} " +
                                       "in the accounts.json file.", "sentry", true);
                            break;
                        case Result.RateLimit:
                            _log.Error("The Steam Rate Limit has been reached. Please try again in a " +
                                       "few minutes.");
                            break;
                        case Result.Code2FAWrong:
                            _log.Error("The provided SteamGuard code was wrong. Please retry.");
                            break;
                    }
                }
                catch (TimeoutException)
                {
                    var timeSpent = DateTime.Now.Subtract(account.StartEpoch.ToDateTime());

                    _log.Error("Connection to account {Account} timed out. It was not possible to resolve the Match " +
                               "ID for the target after {Timespan} seconds.", account.JsonAccount.Username,
                        timeSpent.Seconds);
                    timedOut = true;
                }
                finally
                {
                    if (timedOut)
                    {
                        account.Stop();
                    }

                    _taskDic.Remove(account);
                }
            }));

            _successCount = 0;
        }

        public void FinishBotting(TitanAccount acc = null)
        {
            if (acc != null)
            {
                if (acc.IsRunning)
                {
                    acc.Stop();
                }

                if (_taskDic.ContainsKey(acc))
                {
                    _taskDic.Remove(acc);
                }
            }
            else
            {
                foreach (var pair in _taskDic)
                {
                    if (pair.Key.IsRunning || !pair.Value.IsCompleted)
                    {
                        pair.Key.Stop();

                        _log.Warning("Forcefully finished botting of account {account}.",
                            pair.Key.JsonAccount.Username);
                    }

                    _taskDic.Remove(pair.Key);
                }
            }
        }
    }
}