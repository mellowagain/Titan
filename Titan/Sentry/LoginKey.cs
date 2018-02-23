using System.IO;
using Serilog.Core;
using Titan.Account;
using Titan.Logging;

namespace Titan.Sentry
{
    public class LoginKey
    {

        private Logger _log;

        private FileInfo _file;

        public LoginKey(TitanAccount account)
        {
            _log = LogCreator.Create("Login Key - " + account.JsonAccount.Username);
            
            var dir = new DirectoryInfo(Path.Combine(Titan.Instance.Directory.ToString(), "loginkeys"));
            if (!dir.Exists)
            {
                dir.Create();
            }
            
            _file = new FileInfo(Path.Combine(dir.ToString(), account.JsonAccount.Username + ".key"));
        }

        public void Save(string key)
        {
            File.WriteAllText(_file.ToString(), key);
        }

        public string GetLastKey()
        {
            if (_file.Exists)
            {
                using (var reader = File.OpenText(_file.ToString()))
                {
                    var key = reader.ReadLine();

                    if (!Titan.Instance.Options.Secure)
                    {
                        _log.Debug("Received login key from key file: {key}", key);
                    }
                    
                    return key;
                }
            }
            else
            {
                _log.Warning("Tried to get login key from non-existant key file.");
            }
            
            return null;
        }

        public bool Exists()
        {
            return _file.Exists;
        }
        
    }
}