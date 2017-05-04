using System;
using System.Threading.Tasks;
using Mono.Unix;
using Mono.Unix.Native;

namespace Titan.Bootstrap.Exit
{
    public class UnixExitSignal : IExitSignal
    {

        public event EventHandler Exit;

        private UnixSignal[] _signals = {
            new UnixSignal(Signum.SIGTERM),
            new UnixSignal(Signum.SIGINT),
            new UnixSignal(Signum.SIGUSR1)
        };

        public UnixExitSignal()
        {
            Task.Factory.StartNew(() =>
            {
                UnixSignal.WaitAny(_signals, -1);

                Exit?.Invoke(null, EventArgs.Empty);
            });
        }

    }
}