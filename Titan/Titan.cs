using System;
using System.IO;
using System.Threading;
using CommandLine;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Core;
using SteamKit2;
using Titan.Bans;
using Titan.Bootstrap;
using Titan.Logging;
using Titan.Managers;
using Titan.Mode;
using Titan.UI;
using Titan.Util;

namespace Titan
{
    public sealed class Titan
    {

        public static Logger Logger = LogCreator.Create(); // Global logger
        public static Titan Instance;

        public Options Options;
        public bool EnableUI = true;

        public AccountManager AccountManager;
        public ThreadManager ThreadManager;
        public VictimTracker VictimTracker;
        public BanManager BanManager;
        public UIManager UIManager;

        public IScheduler Scheduler;

        [STAThread]
        public static int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            Logger.Debug("Startup: Loading Titan Bootstrapper.");

            // Initialize Titan Singleton
            Instance = new Titan
            {
                Options = new Options()
            };
            
            Logger.Debug("Startup: Loading Serilog <-> Common Logging Bridge.");
            
            // Common Logging <-> Serilog bridge
            Log.Logger = LogCreator.Create("Quartz.NET Scheduler");
            
            Logger.Debug("Startup: Loading Quartz.NET.");
            
            // Quartz.NET
            Instance.Scheduler = StdSchedulerFactory.GetDefaultScheduler();
            Instance.Scheduler.Start();
            
            // SteamKit
            Logger.Debug("Startup: Refreshing Steam Universe list.");
            
            SteamDirectory.Initialize().Wait();

            Logger.Debug("Startup: Parsing Command Line Arguments.");

            /* Parse arguments provided with the starting of this */
            if(Parser.Default.ParseArguments(args, Instance.Options))
            {
                Logger.Information("Skipping UI and going directly to botting - Target: {Target} - Match ID: {Id}", Instance.Options.Target, Instance.Options.MatchId);
                Instance.EnableUI = false;
            }
            else
            {
                Logger.Information("The arguments --target and --mode were omitted - opening the UI.");
                Instance.EnableUI = true;
            }

            Logger.Debug("Startup: Initializing Gui Manager, Victim Tracker, Account Manager and Ban Manager.");
            
            Instance.UIManager = new UIManager();
            
            Instance.VictimTracker = new VictimTracker();
            
            // Schedule Victim Tracking
            Instance.Scheduler.ScheduleJob(Instance.VictimTracker.Job, Instance.VictimTracker.Trigger);

            var file = string.IsNullOrEmpty(Instance.Options.File) ? "accounts.json" : Instance.Options.File;
            Instance.AccountManager = new AccountManager(new FileInfo(Path.Combine(Environment.CurrentDirectory, file)));

            Instance.ThreadManager = new ThreadManager();

            Instance.BanManager = new BanManager();
            Instance.BanManager.ParseApiKeyFile();
            
            Logger.Debug("Startup: Registering Shutdown Hook.");

            // Register hook
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;

            Logger.Debug("Startup: Parsing accounts.json file.");
            
            if(Instance.AccountManager.ParseAccountFile())
            {
                Logger.Information("Hello and welcome to Titan v1.4.0-Dev.");

                if(Instance.EnableUI)
                {
                    Instance.UIManager.ShowForm(UIType.Main);
                }
                else
                {
                    var mode = BotModeParser.Parse(Instance.Options.Mode);

                    if(mode == BotMode.Report)
                    {
                        Instance.AccountManager.StartBotting(mode, SteamUtil.Parse(Instance.Options.Target),
                            Instance.Options.MatchId != null ? Convert.ToUInt64(Instance.Options.MatchId) : 8);
                    }
                }

                Instance.UIManager.StartMainLoop();
            }
            
            // The Shutdown handler gets only called after the last thread finished.
            // Quartz runs a Watchdog until Scheduler#Shutdown is called, so we're calling it
            // before Titan will be calling the Shutdown Hook.
            Logger.Debug("Shutdown: Shutting down Quartz.NET Scheduler.");
            
            Instance.Scheduler.Shutdown();
            
            return 0; // OK.
        }

        public static void OnShutdown(object sender, EventArgs args)
        {
            // Cleanup a few things before shutting down
            
            Instance.VictimTracker.SaveVictimsFile();
            Instance.BanManager.SaveAPIKeyFile();
            Instance.AccountManager.SaveIndexFile();

            Logger.Information("Thank you and have a nice day.");

            Log.CloseAndFlush();
        }

    }
}