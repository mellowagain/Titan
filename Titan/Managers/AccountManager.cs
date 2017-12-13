using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Serilog.Core;
using Titan.Account;
using Titan.Account.Impl;
using Titan.Json;
using Titan.Logging;
using Titan.Meta;
using Titan.Util;

namespace Titan.Managers
{
    public class AccountManager
    {

        private Logger _log = LogCreator.Create();

        public Dictionary<int, List<TitanAccount>> Accounts = new Dictionary<int, List<TitanAccount>>();
        private List<TitanAccount> _allAccounts = new List<TitanAccount>();

        private Dictionary<int, long> _indexEntries = new Dictionary<int, long>();
        public int Index;

        private FileInfo _indexFile;
        private FileInfo _file;

        private JsonAccounts _parsedAccounts;
        private JsonIndex _parsedIndex;

        private int _lastIndex;

        public AccountManager(FileInfo file)
        {
            _file = file;
            _indexFile = new FileInfo(Path.Combine(Titan.Instance.Directory.ToString(), "index.json"));
            Index = 0;

            var dirInfo = new DirectoryInfo(Path.Combine(Titan.Instance.Directory.ToString(), "sentries"));
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            _log.Debug("Titan Account Manager initialized on {TimeString}. ({UnixTimestamp})",
                DateTime.Now.ToShortTimeString(), Math.Round((double) TimeUtil.GetCurrentTicks()));
        }

        public void ParseAccountFile()
        {
            if (!_file.Exists)
            {
                _log.Warning("The accounts file at {0} doesn't exist!", _file.ToString());
                _log.Warning("Titan will run in dummy mode and allow you to edit the accounts before using it.");
                
                var dummy = @"{
    ""indexes"": [
        {
            ""accounts"": []
        }
    ]
}";
                
                File.WriteAllText(_file.ToString(), dummy);
                Titan.Instance.DummyMode = true;
            }

            using (var reader = File.OpenText(_file.ToString()))
            {
                try
                {
                    _parsedAccounts = (JsonAccounts) Titan.Instance.JsonSerializer.Deserialize(
                        reader, typeof(JsonAccounts)
                    );
                }
                catch (JsonReaderException ex)
                {
                    _log.Error("Could not parse {accounts} file.", "accounts.json");
                    _log.Error("The provided JSON contains errors: {err}", ex.Message);
                    _log.Error("Please run it thought a JSON validator and try again.");
                    Environment.Exit(-1);
                }
            }

            if (_parsedAccounts != null)
            {
                foreach (var indexes in _parsedAccounts.Indexes)
                {
                    var accounts = new List<TitanAccount>();

                    foreach (var account in indexes.Accounts)
                    {
                        TitanAccount acc;

                        var sentry = account.Sentry ||
                                     account.SharedSecret != null &&
                                     !account.SharedSecret.Equals("Shared Secret for SteamGuard",
                                         StringComparison.InvariantCultureIgnoreCase); // Paster proofing

                        if (sentry)
                        {
                            acc = new ProtectedAccount(account);
                        }
                        else
                        {
                            acc = new UnprotectedAccount(account);
                        }

                        if (account.Enabled)
                        {
                            accounts.Add(acc);
                            _allAccounts.Add(acc);
                        }

                        if (!Titan.Instance.Options.Secure)
                        {
                            _log.Debug("Found account (specified in index #{Index}): Username: {Username} / " +
                                       "Password: {Password} / Sentry: {sentry} / Enabled: {Enabled}",
                                Index, account.Username, account.Password, account.Sentry, account.Enabled);
                        }
                    }

                    if (accounts.Count > 11 && !Titan.Instance.DummyMode)
                    {
                        _log.Warning("You have more than 11 account specified in index {Index}. " +
                                     "It is recommend to specify max. 11 accounts.", Index);
                    }

                    Accounts.Add(Index, accounts);

                    Index++;
                }
            }
            else
            {
                _log.Warning("The accounts.json could not be parsed. Enabling Dummy mode.");
                _file.MoveTo(Path.Combine(_file.DirectoryName, "accounts.broken.json"));
                ParseAccountFile(); // Recursion is a meme
                return;
            }

            if (_allAccounts.Count < 11 && !Titan.Instance.DummyMode && _allAccounts.Count >= 1)
            {
                _log.Warning("Less than 11 (only {Count}) accounts have been parsed. Atleast 11 accounts " +
                             "are required to get a target into Overwatch.", _allAccounts.Count);
                
                /* TODO: This calls UIManager even thought it wasn't created yet - causes crash of application
                Titan.Instance.UIManager.SendNotification(
                    "Titan - Error", "You have less than 11 accounts specified. " +
                                     "Atleast 11 bot accounts need to specified to get " +
                                     "a target into Overwatch."
                );*/
            }

            var list = new List<object>();

            foreach (var keyPair in Accounts)
            {
                list.Add(new { Index = keyPair.Key, keyPair.Value.Count });
            }

            _log.Information("Account file has been successfully parsed. {Count} accounts " +
                             "have been parsed. Details: {@List}", _allAccounts.Count, list);
            
            Accounts.Add(-1, _allAccounts);

            _lastIndex = Index;
            Index = 0;

            if (_allAccounts.Count < 1 && !Titan.Instance.DummyMode)
            {
                _log.Warning("The {File} file has been created but doesn't have accounts specified.", _file.ToString());
                _log.Warning("Titan will run in dummy mode and allow you to edit the accounts before using it.");
                
                Titan.Instance.DummyMode = true;
            }
            
            ParseIndexFile();
        }

        public void SaveAccountsFile()
        {
            var indexes = (from keyPair in Accounts
                where keyPair.Key != -1
                select new JsonAccounts.JsonIndex
                {
                    Accounts = keyPair.Value.Select(account => account.JsonAccount).ToArray()
                }).ToList();

            var jsonAccount = new JsonAccounts
            {
                Indexes = indexes.ToArray()
            };
            
            using (var writer = File.CreateText(_file.ToString()))
            {
                var textToWrite = JsonConvert.SerializeObject(jsonAccount, Formatting.Indented);
                writer.Write(textToWrite);
            }
        }

        private void ParseIndexFile()
        {
            if (_indexFile.Exists) // check if file exists
            {
                using (var reader = File.OpenText(_indexFile.ToString()))
                {
                    _parsedIndex = (JsonIndex) Titan.Instance.JsonSerializer.Deserialize(reader, typeof(JsonIndex));
                    // read it and deserialize it into a Index object
                }

                var lowest = _parsedIndex.AvailableIndex; // find the available index and set it to lowest

                if (lowest == -1)
                {
                    lowest++;
                }

                foreach (var expireEntry in _parsedIndex.Entries)
                {
                    // check if a entry that is marked for expiration is already expired and ready to bot
                    if (expireEntry.ExpireTimestamp <= TimeUtil.GetCurrentTicks())
                    {
                        _log.Debug("Index {Index} has expired. It is now available for botting.",
                            expireEntry.TargetedIndex);

                        if (lowest > expireEntry.TargetedIndex) // if thats the case, check if its lower than the available one
                        {
                            lowest = expireEntry.TargetedIndex;
                        }

                        _parsedIndex.Entries = _parsedIndex.Entries.Where(val => val != expireEntry)
                            .ToArray(); // and remove it from the expiration list
                    }
                    else
                    {
                        if (!_indexEntries.ContainsKey(expireEntry.TargetedIndex))
                        {
                            _indexEntries.Add(expireEntry.TargetedIndex, expireEntry.ExpireTimestamp);
                        }

                        _log.Debug("Index #{Index} hasn't expired yet. It will expire on {Time}.",
                            expireEntry.TargetedIndex,
                            TimeUtil.TicksToDateTime(expireEntry.ExpireTimestamp).ToShortTimeString());

                        if (expireEntry.TargetedIndex == lowest)
                        {
                            lowest++;
                        }
                    }
                }

                Index = lowest;
            }
            else
            {
                Index = 0;
                SaveIndexFile(); // it doesn't exist, we're gonna' create it!
            }

            var valid = false;
            foreach (var keyVal in Accounts)
            {
                if (keyVal.Key == Index && Index != -1)
                {
                    _log.Debug("Using index #{Index} for botting.", Index);
                    valid = true;
                }
            }

            if (!valid)
            {
                _log.Warning("Index #{index} doesn't exist. The Bot will use index " +
                             "#{ForcedIndex}. Please keep in mind that it may already " +
                             "been used.", Index, Index = 0);
            }

        }

        public void SaveIndexFile()
        {
            if(_indexFile.Exists)
            {
                _indexFile.Delete();
            }

            var jsonIndex = new JsonIndex
            {
                AvailableIndex = Index,
                Entries = _indexEntries.Select(keyVal => new JsonIndex.JsonEntry
                    {
                        TargetedIndex = keyVal.Key,
                        ExpireTimestamp = keyVal.Value
                    })
                    .ToArray()
            };

            using (var writer = File.CreateText(_indexFile.ToString()))
            {
                var str = JsonConvert.SerializeObject(jsonIndex, Formatting.Indented);
                writer.Write(str);
            }

            _log.Debug("Successfully wrote index file.");
        }

        public void StartReporting(int index, ReportInfo info)
        {
            _log.Debug("Starting botting using index {Index}.", index);

            if (Accounts.TryGetValue(index, out var accounts))
            {
                accounts.Last().IsLast = true;
                
                foreach (var acc in accounts)
                {
                    try
                    {
                        Titan.Instance.ThreadManager.StartReport(acc, info);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Could not start botting for account {Account}: {Message}",
                            acc.JsonAccount.Username, ex.Message);
                    }
                }
            }
            else
            {
                _log.Error("Could not export accounts for current index {Index}. " +
                           "Does it exist?", Index);
                return;
            }
                
            Titan.Instance.VictimTracker.AddVictim(info.SteamID);

            if (!_indexEntries.ContainsKey(index) && index != -1)
            {
                _indexEntries.Add(index, TimeUtil.DateTimeToTicks(DateTime.Now.AddHours(12)));
                SaveIndexFile();
            }

            _log.Debug("Index #{Index} has been used. It will be unlocked on {Unlock}. " +
                       "Suggesting index #{NextIndex} for next botting session.",
                index, DateTime.Now.AddHours(12).ToLongTimeString(), ++index);
        }
        
        public void StartCommending(int index, CommendInfo info)
        {
            _log.Debug("Starting botting using index {Index}.", index);

            if (Accounts.TryGetValue(index, out var accounts))
            {
                accounts.Last().IsLast = true;
                
                foreach (var acc in accounts)
                {
                    try
                    {
                        Titan.Instance.ThreadManager.StartCommend(acc, info);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Could not start botting for account {Account}: {Message}",
                            acc.JsonAccount.Username, ex.Message);
                    }
                }
            }
            else
            {
                _log.Error("Could not export accounts for current index {Index}. " +
                           "Does it exist?", Index);
                return;
            }

            if (!_indexEntries.ContainsKey(index))
            {
                _indexEntries.Add(index, TimeUtil.DateTimeToTicks(DateTime.Now.AddHours(12)));
                SaveIndexFile();
            }

            _log.Debug("Index #{Index} has been used. It will be unlocked on {Unlock}. " +
                       "Suggesting index #{NextIndex} for next botting session.",
                index, DateTime.Now.AddHours(12).ToLongTimeString(), ++index);
        }
        
        public void StartMatchIDResolving(int index, LiveGameInfo info)
        {
            if (Accounts.TryGetValue(index, out var accounts))
            {
                try
                {
                    Titan.Instance.ThreadManager.StartMatchResolving(accounts[0], info);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "Could not start resolving for account {Account}: {Message}",
                               accounts[0].JsonAccount.Username, ex.Message);
                }
            }
            else
            {
                _log.Error("Could not export accounts for current index {Index}. " +
                           "Does it exist?", Index);
            }
        }

        public void StartIdleing(int index, IdleInfo info)
        {
            if (Accounts.TryGetValue(index, out var accounts))
            {
                accounts.Last().IsLast = true;
                
                foreach (var acc in accounts)
                {
                    try
                    {
                        Titan.Instance.ThreadManager.StartIdling(acc, info);
                    }
                    catch (Exception ex)
                    {
                        _log.Error(ex, "Could not start botting for account {Account}: {Message}",
                            acc.JsonAccount.Username, ex.Message);
                    }
                }
            }
            else
            {
                _log.Error("Could not export accounts for current index {Index}. " +
                           "Does it exist?", Index);
            }
        }

        public void AddAccount(TitanAccount account)
        {
            if (!Accounts.ContainsKey(_lastIndex))
            {
                _lastIndex -= 1;
            }
          
            foreach (var keyPair in Accounts)
            {
                if (keyPair.Key == _lastIndex)
                {
                    if (keyPair.Value.Count < 11 && account.JsonAccount.Enabled)
                    {
                        keyPair.Value.Add(account);
                    }
                    else
                    {
                        _lastIndex += 1;
                    }
                }
            }
            
            if (!Accounts.ContainsKey(_lastIndex) && account.JsonAccount.Enabled)
            {
                var list = new List<TitanAccount>();
                
                Accounts.Add(_lastIndex, list);
            }

            if (account.JsonAccount.Enabled)
            {
                _allAccounts.Add(account);
                Accounts[-1] = _allAccounts;
            }
            
            if (!Titan.Instance.Options.Secure)
            {
                _log.Debug("Added account in index #{Index}: Username: {Username} / " +
                           "Password: {Password} / Sentry: {sentry} / Enabled: {Enabled}",
                    _lastIndex, account.JsonAccount.Username, account.JsonAccount.Password, 
                    account.JsonAccount.Sentry, account.JsonAccount.Enabled);
            }
        }

        public bool TryGetAccount(string username, out TitanAccount output)
        {
            foreach (var keyPair in Accounts)
            {
                foreach (var account in keyPair.Value)
                {
                    if (account.JsonAccount.Username.Equals(username))
                    {
                        output = account;
                        return true;
                    }
                }
            }
            
            output = default(TitanAccount);
            return false;
        }

        public bool RemoveAccount(TitanAccount target)
        {
            _log.Information("Removing account {account}.", target.JsonAccount.Username);

            var success = false;

            foreach (var keyPair in Accounts)
            {
                if (keyPair.Value.Contains(target))
                {
                    keyPair.Value.Remove(target);
                    success = true;
                }
            }

            if (success)
            {
                _allAccounts.Remove(target);
            }
            
            return success;
        }

        public int Count()
        {
            return _allAccounts.Count;
        }

    }
}