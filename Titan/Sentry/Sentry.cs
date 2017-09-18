using System;
using System.IO;
using System.Security.Cryptography;
using Serilog.Core;
using SteamKit2;
using Titan.Account;
using Titan.Logging;

namespace Titan.Sentry
{
    public class Sentry
    {

        private Logger _log;

        private string _username;

        private FileInfo _file;
        
        public Sentry(TitanAccount account)
        {
            _log = LogCreator.Create("Sentry - " + account.JsonAccount.Username);
            
            var dir = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "sentries"));
            if (!dir.Exists)
            {
                dir.Create();
            }
            
            _file = new FileInfo(Path.Combine(dir.ToString(), account.JsonAccount.Username + ".sentry.bin"));

            _username = account.JsonAccount.Username;
        }

        public byte[] Save(int offset, byte[] data, int count, out int size)
        {
            byte[] hash;

            using (var stream = File.Open(_file.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                //stream.Write(data, offset, count);
                stream.Write(data, 0, count);
                size = Convert.ToInt32(stream.Length);
                stream.Seek(0, SeekOrigin.Begin);

                using (var sha = SHA1.Create())
                {
                    hash = sha.ComputeHash(stream);
                }
            }

            if (hash.Length > 0 && size > 0)
            {
                _log.Debug("Hash for saved sentry file found: {Hash}", Convert.ToBase64String(hash));

                return hash;
            }

            _log.Error("(Sentry::Save): Failed to save sentry file.");
            return new byte[0];
        }

        public byte[] Hash()
        {
            if (!_file.Exists)
            {
                var fileBytes = File.ReadAllBytes(_file.ToString());

                if (fileBytes.Length > 0)
                {
                    var hash = CryptoHelper.SHAHash(fileBytes);

                    if (hash != null && hash.Length > 0)
                    {
                        _log.Debug("Hash for sentry file found: {Hash}", Convert.ToBase64String(hash));

                        return hash;
                    }
                }
            }
            else
            {
                _log.Warning("(Sentry::Hash): Tried to hash non-existant sentry file.");
            }
            
            _log.Error("(Sentry::Hash): Failed to hash sentry file.");
            return new byte[0];
        }
        
       
    }


}