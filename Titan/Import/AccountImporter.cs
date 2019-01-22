using System.Collections.Generic;
using Titan.Account;

namespace Titan.Import
{
    public abstract class AccountImporter
    {
        protected string File;

        protected AccountImporter(string file)
        {
            File = file;
        }

        public abstract List<TitanAccount> ParseAccounts();

    }
}