using System;
using System.IO;
using Eto.Forms;
using log4net;
using Newtonsoft.Json;

namespace Titan.Core
{
    public class Hub
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(Hub));

        public static readonly FileInfo AccountFile = new FileInfo(Environment.CurrentDirectory + Path.DirectorySeparatorChar + "accounts.json");
        public static Json.Accounts JsonAccounts;

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
                Log.Debug("Account specified - U: " + a.Username + " P: " + a.Password);
            }
        }

    }
}