using System.Collections.Generic;
using System.Text.RegularExpressions;
using Titan.Account;
using Titan.Account.Impl;
using Titan.Json;

namespace Titan.Import.Impl
{
    public class GenericAccountListImporter : AccountImporter
    {
        
        public GenericAccountListImporter(string file) : base(file)
        {
            // Passed to base
        }

        public override List<TitanAccount> ParseAccounts()
        {
            List<TitanAccount> result = new List<TitanAccount>();
            var lines = System.IO.File.ReadLines(File);

            foreach (var line in lines)
            {
                string[] parts = line.Split(':');

                if (parts.Length < 2)
                {
                    continue;
                }
                
                var jsonAccount = new JsonAccounts.JsonAccount
                {
                    Username = Regex.Replace(parts[0].Trim(), @"\t|\n|\r", ""),
                    Password = Regex.Replace(parts[1].Trim(), @"\t|\n|\r", ""),
                    Sentry = false, // Generic username:password lists don't have SteamGuard enabled in most cases.
                    Enabled = true,
                    SharedSecret = null
                };
                
                result.Add(new UnprotectedAccount(jsonAccount));
            }

            return result;
        }
        
    }
}