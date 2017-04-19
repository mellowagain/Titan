using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Eto.Forms;
using log4net;
using Newtonsoft.Json;
using Titan.Core.Accounts;

namespace Titan.Core
{
    public class Hub
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(Hub));

        public static readonly List<Account> Accounts = new List<Account>();

        public static readonly FileInfo AccountFile = new FileInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "accounts.json");
        public static Json.Accounts JsonAccounts;

        public static void StartBotting(string target, string matchId)
        {
            var tar = Convert.ToUInt32(target);
            var mId = Convert.ToUInt64(matchId);

            foreach(var a in Accounts)
            {
                try
                {
                    if(!a.Report(tar, mId))
                    {
                        Log.WarnFormat("Report for {0} failed. Is the user in-game or the credicentials wrong?", a.Json.Username);
                    }
                }
                catch (Exception ex)
                {
                    Log.ErrorFormat("Report for {0} failed. A error occured.", a.Json.Username);
                    Log.Error(ex);
                }
            }
        }

        public static void ReadFile()
        {
            if(!AccountFile.Exists)
            {
                MessageBox.Show("Could not read account file.", "Titan - Error", MessageBoxType.Error);
                Log.Error("Can't read Account file. Does it exist?");
                return;
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
        }

    }
}