using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Eto.Forms;
using Newtonsoft.Json;
using Serilog.Core;
using SteamKit2;
using Titan.Bot.Mode;
using Titan.Bot.Threads;
using Titan.Json;
using Titan.Logging;

namespace Titan.Bot
{
    public class AccountManager
    {

        private Logger _log = LogCreator.Create();

        private Dictionary<int, List<Account>> _accounts = new Dictionary<int, List<Account>>();
        private List<Account> _allAccounts = new List<Account>();

        private Dictionary<int, long> _indexEntries = new Dictionary<int, long>();
        private int _index;

        private FileInfo _indexFile;
        private FileInfo _file;

        private Accounts _parsedAccounts;
        private Index _parsedIndex;

        public AccountManager(FileInfo file)
        {
            _file = file;
            _indexFile = new FileInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar +
                                      "index.json");
            _index = 0;
        }

        public bool ParseAccountFile()
        {
            if(!_file.Exists)
            {
                _log.Error("The accounts file at {0} doesn't exist! It is required and needs to " +
                           "atleast one account specified.", _file.ToString());
                MessageBox.Show("The account file at " + _file + "doesn't exist. \nPlease create " +
                                "it and specify atleast one account.", "<!> Titan <!>", MessageBoxType.Error);
                return false;
            }

            using(var reader = File.OpenText(_file.ToString()))
            {
                _parsedAccounts = (Accounts) new JsonSerializer().Deserialize(reader, typeof(Accounts));
            }

            var list = new List<Account>();

            foreach(var account in _parsedAccounts.JsonAccounts)
            {
                var a = new Account(account);

                _allAccounts.Add(a);
                list.Add(a);

                _log.Debug("Found account (stored in index #{Index}): Username: {Username} / " +
                           "Password: {Password} / Sentry: {sentry}",
                    _index, a.Json.Username, a.Json.Password, a.Json.Sentry);

                if(list.Count >= 11 || account.Equals(_parsedAccounts.JsonAccounts.Last()))
                {
                    _accounts.Add(_index, list);
                    _index++;
                    list.Clear();
                }
            }

            if(_allAccounts.Count < 11)
            {
                _log.Warning("Less than 11 (only {Count}) accounts have been parsed. Atleast 11 accounts " +
                             "are required to get a target into Overwatch.", _allAccounts.Count);
                MessageBox.Show("You have less than 11 accounts specified. There are atleast 11 " +
                                "reports needed to get a target into Overwatch.", MessageBoxType.Warning);
            }
            else
            {
                _log.Information("Account file successfully parsed. {Count} accounts have been parsed.", _allAccounts.Count);
            }

            ParseIndexFile();

            return true;
        }

        private void ParseIndexFile()
        {
            if(_indexFile.Exists) // check if file exists
            {
                using(var reader = File.OpenText(_indexFile.ToString()))
                {
                    _parsedIndex = (Index) new JsonSerializer().Deserialize(reader, typeof(Index));
                    // read it and deserialize it into a Index object
                }

                var lowest = _parsedIndex.AvailableIndex; // find the available index and set it to lowest

                foreach(var expireEntry in _parsedIndex.ExpireEntries)
                {
                    // check if a entry that is marked for expiration is already expired and ready to bot
                    if(expireEntry.Expires <= GetCurrentUnixTime())
                    {
                        if(lowest > expireEntry.Index) // if thats the case, check if its lower than the available one
                        {
                            lowest = expireEntry.Index;
                        }

                        _parsedIndex.ExpireEntries = _parsedIndex.ExpireEntries.Where(val => val != expireEntry)
                            .ToArray(); // and remove it from the expiration list
                    }
                    else
                    {
                        _indexEntries.Add(expireEntry.Index, expireEntry.Expires);
                    }
                }

                _index = lowest;
            }
            else
            {
                SaveIndexFile(); // it doesn't exist, we're gonna' create it!
            }

        }

        public void SaveIndexFile()
        {
            var entries = _indexEntries.Select(e => new Index.IndexExpireEntry {Index = e.Key, Expires = e.Value}).ToList();

            var obj = new Index
            {
                AvailableIndex = _index,
                ExpireEntries = entries.ToArray()
            };

            if(_indexFile.Exists)
            {
                File.WriteAllText(_indexFile.ToString(), string.Empty);
            }

            using(var writer = File.CreateText(_indexFile.ToString()))
            {
                new JsonSerializer().Serialize(writer, obj);
            }
        }

        public void StartBotting(BotMode mode, string target, string matchId)
        {
            var convTarget = new SteamID(target).AccountID;
            var convMatchId = mode == BotMode.Report ? Convert.ToUInt64(matchId) : 0;

            List<Account> accounts;
            if(_accounts.TryGetValue(_index, out accounts))
            {
                try
                {
                    ThreadManager.StartWatchdogThread();
                }
                catch (Exception ex)
                {
                    _log.Warning(ex, "Could not start Watchdog thread, starting to report without one!");
                }

                foreach(var acc in accounts)
                {
                    try
                    {
                        ThreadManager.StartThread(acc, convTarget, convMatchId, mode);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Could not start reporting for account {Account}: {Message}",
                            acc.Json.Username, ex.Message);
                    }
                }
            }
            else
            {
                _log.Error("Could not export accounts for current index {Index}.", _index);
                return;
            }

            var thread = new Thread(() =>
            {
                while(ThreadManager.WatchdogThread.IsAlive) {}

                _log.Information("Successfully bombed {Target} {Count}x using method {Method}.",
                    target, accounts.Count, mode);

                MessageBox.Show("Success!", "<!> Titan <!>");
            });
            thread.Start();
        }

        // =====================================================
        // UTILITY METHODS
        // =====================================================

        private int GetCurrentUnixTime()
        {
            return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        }

    }
}