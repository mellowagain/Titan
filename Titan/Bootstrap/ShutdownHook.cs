using System;
using Serilog;
using Serilog.Core;
using Titan.Bootstrap.Exit;
using Titan.Logging;

namespace Titan.Bootstrap
{
    public class ShutdownHook
    {

        private static Logger _log = LogCreator.Create();

        public static IExitSignal ExitSignal;
        public static bool IsUnix
        {
            get
            {
                var p = (int) Environment.OSVersion.Platform;
                return p == 4 || p == 6 || p == 128;
            }
        }

        public static void Hook()
        {
            _log.Debug("Detected operating system: {System} - Running Titan in {Mode} mode.",
                IsUnix ? "Linux" : "Windows", IsUnix ? "Mono" : "Native");

            ExitSignal = IsUnix ? (IExitSignal) new UnixExitSignal() : new WinExitSignal();
            ExitSignal.Exit += OnShutdown;
        }

        public static void OnShutdown(object sender, EventArgs args)
        {
            _log.Information("Thank you. Have a nice day.");

            Log.CloseAndFlush();
            // TODO: Shutdown handeling
        }
    }
}