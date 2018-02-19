using System;
using System.Threading;
using System.Threading.Tasks;
using Serilog.Core;
using SteamAuth;
using Titan.Account;
using Titan.Logging;

namespace Titan.Sentry
{
    public class SharedSecret
    {

        private Logger _log;

        private static bool _timeAligned;

        private SteamGuardAccount _steamGuardAccount;
        
        public SharedSecret(TitanAccount account)
        {
            _log = LogCreator.Create("Shared Secret - " + account.JsonAccount.Username);

            _steamGuardAccount = new SteamGuardAccount
            {
                SharedSecret = account.JsonAccount.SharedSecret
            };
        }

        public string GenerateCode()
        {
            // Align time with the Steam servers before generating code so we don't generate a outdated code.
            if (!_timeAligned)
            {
                TimeAligner.AlignTime();
                
                _timeAligned = true;
                Task.Run(() =>
                {
                    // Time alignment expires in 5 minutes
                    Thread.Sleep(TimeSpan.FromMinutes(5));
                    _timeAligned = false;
                });
            }

            var code = _steamGuardAccount.GenerateSteamGuardCode();

            if (!Titan.Instance.Options.Secure)
            {
                _log.Debug("Generated Steam Guard code from Shared Secret: {code}", code);
            }
            
            return code;
        }
        
    }
}