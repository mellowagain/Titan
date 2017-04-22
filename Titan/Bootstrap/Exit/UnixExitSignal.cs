using System;
using System.Threading.Tasks;
using Mono.Unix;

namespace Titan.Bootstrap.Exit
{
    public class UnixExitSignal : IExitSignal
    {

        public event EventHandler Exit;

        private UnixSignal[] _signals = {
            new UnixSignal(Mono.Unix.Native.Signum.SIGTERM),
            new UnixSignal(Mono.Unix.Native.Signum.SIGINT),
            new UnixSignal(Mono.Unix.Native.Signum.SIGUSR1)
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