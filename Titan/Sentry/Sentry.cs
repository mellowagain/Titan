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
        }

        public bool Save(int offset, byte[] data, int bytesToWrite, out byte[] hash, out int size)
        {
            using (var stream = File.Open(_file.ToString(), FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                stream.Seek(offset, SeekOrigin.Begin);
                stream.Write(data, 0, bytesToWrite);
                size = (int) stream.Length;

                stream.Seek(0, SeekOrigin.Begin);
                using (var sha = SHA1.Create())
                {
                    hash = sha.ComputeHash(stream);
                }
                
                _log.Debug("Wrote to sentry file with hash: {Hash}", Convert.ToBase64String(hash));
                return true;
            }
            
            hash = null;
            size = -1;
            return false;
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
                _log.Warning("Tried to hash non-existant sentry file.");
            }
            
            return null;
        }

        public bool Exists()
        {
            return _file.Exists;
        }
       
    }

}