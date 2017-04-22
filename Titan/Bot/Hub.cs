using System;
using System.Collections.Generic;
using System.IO;
using Eto.Forms;
using Newtonsoft.Json;
using Serilog.Core;
using SteamKit2;
using Titan.Bot.Mode;
using Titan.Bot.Threads;
using Titan.Logging;

namespace Titan.Bot
{
    public class Hub
    {

        private static Logger _log = LogCreator.Create();

        public static readonly List<Account> Accounts = new List<Account>();

        public static readonly FileInfo AccountFile = new FileInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "accounts.json");
        public static Json.Accounts JsonAccounts;

        public static void StartBotting(string target, string matchId, BotMode mode)
        {
            var tar = new SteamID(target).AccountID;
            var mId = mode == BotMode.Report ? Convert.ToUInt64(matchId) : 0;

            foreach(var a in Accounts)
            {
                try
                {
                    ThreadManager.StartThread(a, tar, mId, mode);
                }
                catch (Exception ex)
                {
                    _log.Error(ex, "{0} for {1} failed. A error occured.", mode, a.Json.Username);
                }
            }

            MessageBox.Show("Successfully bombed " + target + " " + Accounts.Count + "x using method \"" + mode + "\".");
        }

        public static bool ReadFile()
        {
            if(!AccountFile.Exists)
            {
                MessageBox.Show("Could not read account.json file. \nPlease create it or check it using a Json Validator.",
                    "Titan - Error", MessageBoxType.Error);
                _log.Error("Can't read Account file. Does it exist?");
                return false;
            }

            using(var reader = File.OpenText(AccountFile.ToString()))
            {
                var serializer = new JsonSerializer();
                JsonAccounts = (Json.Accounts) serializer.Deserialize(reader, typeof(Json.Accounts));
            }

            foreach(var a in JsonAccounts.JsonAccounts)
            {
                Accounts.Add(new Account(a));
                _log.Debug("Account specified - U: " + a.Username + " P: " + a.Password);
            }

            if(Accounts.Count < 11)
            {
                MessageBox.Show("You have less than 11 accounts specified. " +
                                "There are atleast 11 reports needed to get a target into Overwatch.", MessageBoxType.Warning);
            }

            return true;
        }

    }
}