using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using Serilog;
using Serilog.Core;

namespace Titan.Logging
{
    public static class LogCreator
    {

        private static DirectoryInfo _logDir;

        static LogCreator()
        {
            _logDir = new DirectoryInfo(Path.Combine(
                Titan.Instance != null ? Titan.Instance.Directory.ToString() : Environment.CurrentDirectory, "logs"
            ));
        }

        public static Logger Create(string name)
        {
            if (Titan.Instance != null && Titan.Instance.Options.Debug)
            {
                return CreateDebugLogger(name);
            }

            return CreateInfoLogger(name);
        }

        public static Logger CreateInfoLogger(string name)
        {
            return new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Thread}] {Level:u} {Name} - {Message}{NewLine}{Exception}"
                )
                .WriteTo.Async(c => c.RollingFile(
                    Path.Combine(_logDir.ToString(), name + "-{Date}.log"), 
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u}] {Name} {ThreadId} - {Message}{NewLine}{Exception}"
                ))
                .MinimumLevel.Information()
                .Enrich.WithProperty("Name", name)
                .Enrich.WithProperty("Thread", Thread.CurrentThread.Name)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();
        }

        public static Logger CreateDebugLogger(string name)
        {
            return new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Thread}] {Level:u} {Name} - {Message}{NewLine}{Exception}"
                )
                .WriteTo.Async(c => c.RollingFile(
                    Path.Combine(_logDir.ToString(), name + "-{Date}.log"), 
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u}] {Name} {ThreadId} - {Message}{NewLine}{Exception}"
                ))
                .MinimumLevel.Debug()
                .Enrich.WithProperty("Name", name)
                .Enrich.WithProperty("Thread", Thread.CurrentThread.Name)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();
        }

        public static Logger CreateDebugFileLogger(string name)
        {
            return new LoggerConfiguration()
                .WriteTo.Async(c => c.RollingFile(
                    Path.Combine(_logDir.ToString(), name + "-{Date}.log"), 
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u}] {Name} {ThreadId} - {Message}{NewLine}{Exception}"
                ))
                .MinimumLevel.Debug()
                .Enrich.WithProperty("Name", name)
                .Enrich.WithProperty("Thread", Thread.CurrentThread.Name)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .CreateLogger();
        }

        public static Logger CreateQuartzLogger()
        {
            return new LoggerConfiguration()
                .WriteTo.Console(
                    outputTemplate: "{Timestamp:HH:mm:ss} [{Thread}] {Level:u} {Name} - {Message}{NewLine}{Exception}"
                )
                .WriteTo.Async(c => c.RollingFile(
                    Path.Combine(_logDir.ToString(), "Quartz.Net-Scheduler-{Date}.log"),
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u}] {Name} {ThreadId} - {Message}{NewLine}{Exception}"
                ))
                .MinimumLevel.Debug()
                .Enrich.WithProperty("Name", "Quartz.NET Scheduler")
                .Enrich.WithProperty("Thread", Thread.CurrentThread.Name)
                .Enrich.FromLogContext()
                .Enrich.WithThreadId()
                .Filter.ByExcluding(logEvent => 
                    logEvent.RenderMessage().Contains("Batch acquisition") && 
                    logEvent.RenderMessage().Contains("triggers"))
                .CreateLogger();
        }
 
        public static Logger Create()
        {
            var reflectedType = new StackTrace().GetFrame(1).GetMethod().ReflectedType;
            return Create(reflectedType != null ? reflectedType.Name : "Titan (unknown Parent)");
        }

    }
}