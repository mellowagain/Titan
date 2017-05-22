using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Serilog;
using Serilog.Core;

namespace Titan.Logging
{
    public class LogCreator
    {

        private static DirectoryInfo _logDir = new DirectoryInfo(Environment.CurrentDirectory +
                                                                     Path.DirectorySeparatorChar + "logs");

        public static Logger Create(string name)
        {
            return new LoggerConfiguration()
                .WriteTo.LiterateConsole(outputTemplate:
                    "{Timestamp:HH:mm:ss} [{Thread}] {Level:u} {Name} - {Message}{NewLine}{Exception}")
                .WriteTo.Async(a => a.RollingFile(Path.Combine(_logDir.ToString(),
                        name + "-{Date}.log"), outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u}] {Name} @ {ThreadId} - {Message}{NewLine}{Exception}"))
                .MinimumLevel.Information() // TODO: Change this to "INFO" on release.
                .Enrich.WithProperty("Name", name)
                .Enrich.WithProperty("Thread", Thread.CurrentThread.Name)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();
        }

        public static Logger Create()
        {
            var reflectedType = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
            return Create(reflectedType != null ? reflectedType.Name : "Titan (unknown Parent)");
        }

    }
}