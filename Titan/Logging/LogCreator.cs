using System;
using System.Diagnostics;
using System.IO;
using Serilog;
using Serilog.Core;

namespace Titan.Logging
{
    public class LogCreator
    {

        public static DirectoryInfo LogDirectory = new DirectoryInfo(Environment.CurrentDirectory +
                                                                     Path.DirectorySeparatorChar + "logs");

        public static Logger Create(string name)
        {
            return new LoggerConfiguration()
                .WriteTo.LiterateConsole(outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u}] {Message}{NewLine}{Exception}")
                .WriteTo.RollingFile(Path.Combine(LogDirectory.ToString(),
                        name + "-{Date}.log"), outputTemplate:
                    "[{Timestamp:HH:mm:ss} {Level:u}] {Message}{NewLine}{Exception}")
                .MinimumLevel.Debug() // TODO: Change this to "INFO" on release.
                .CreateLogger();
        }

        public static Logger Create()
        {
            var reflectedType = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
            return Create(reflectedType != null ? reflectedType.Name : "Titan (unknown Parent)");
        }
    }
}