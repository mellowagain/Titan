using System;
using System.Collections.Generic;
using System.IO;
using Eto.Forms;
using log4net;
using Newtonsoft.Json;
using SteamKit2;
using Titan.Core.Accounts;

namespace Titan.Core
{
    public class Hub
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(Hub));

        public static readonly List<Account> Accounts = new List<Account>();

        public static readonly FileInfo AccountFile = new FileInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "accounts.json");
        public static Json.Accounts JsonAccounts;

        public static void StartBotting(string target, string matchId, BotMode mode)
        {
            var tar = new SteamID(target).AccountID;
            var mId = mode == BotMode.Report ? Convert.ToUInt64(matchId) : 0;

            Log.Info("== STARTING BOTTING ==");

            foreach(var a in Accounts)
            {
                try
                {
                    ThreadManager.StartThread(a, tar, mId, mode);
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Report for {0} failed. A error occured.", a.Json.Username);
                    Log.Error(ex);
                }
            }

            Log.Info("== FINISHED BOTTING ==");

            MessageBox.Show("Successfully bombed " + target + " " + Accounts.Count + "x using method \"" + mode + "\".");
        }

        public static bool ReadFile()
        {
            if(!AccountFile.Exists)
            {
                MessageBox.Show("Could not read account.json file. \nPlease create it or check it using a Json Validator.",
                    "Titan - Error", MessageBoxType.Error);
                Log.Error("Can't read Account file. Does it exist?");
                return false;
            }

            using(var reader = File.OpenText(AccountFile.ToString()))
            {
                var serializer = new JsonSerializer();
                JsonAccounts = (Json.Accounts) serializer.Deserialize(reader, typeof(Json.Accounts));
            }

            foreach(var a in JsonAccounts.JsonAccounts)
            {
                Accounts.Add(new Account(a.Username, a.Password, a));
                Log.Debug("Account specified - U: " + a.Username + " P: " + a.Password);
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