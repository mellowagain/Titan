using System;
using System.IO;
using System.Text.RegularExpressions;
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
using Titan.Meta;
using Titan.Mode;
using Titan.UI;
using Titan.UI.General;
using Titan.Util;

namespace Titan
{
    public sealed class Titan
    {

        public static Logger Logger; // Global logger
        public static Titan Instance;

        public Options Options;
        public bool EnableUI = true;

        public AccountManager AccountManager;
        public ThreadManager ThreadManager;
        public VictimTracker VictimTracker;
        public BanManager BanManager;
        public UIManager UIManager;

        public IScheduler Scheduler;

        // May be null if Options#Debug is false
        public DirectoryInfo DebugDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "debug"));

        [STAThread]
        public static int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            // Initialize Titan Singleton
            Instance = new Titan
            {
                Options = new Options()
            };

            Logger = LogCreator.Create();
            
            Logger.Debug("Startup: Loading Titan Bootstrapper.");
            
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
                Logger.Information("Skipping UI and going directly to botting - Target: {Target} - Match ID: {ID}", Instance.Options.Target, Instance.Options.MatchID);
                Instance.EnableUI = false;
            }
            else
            {
                Logger.Information("The arguments --target and --mode were omitted - opening the UI.");
                Instance.EnableUI = true;
            }
            
            // Reinitialize logger with new parsed debug option
            Logger = LogCreator.Create();

            // Initialize the Debug directory if Debug mode is enabled
            if(Instance.Options.Debug)
            {
                if(!Instance.DebugDirectory.Exists)
                {
                    Instance.DebugDirectory.Create();
                }
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

            SteamUtil.WebAPIKey = Instance.BanManager.APIKey;
            
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
                    //Instance.UIManager.ShowForm(UIType.Main, new General());
                }
                else
                {
                    switch(Regex.Replace(Instance.Options.Mode.ToLowerInvariant(), @"\s+", ""))
                    {
                        case "report":
                            Instance.AccountManager.StartReporting(Instance.AccountManager.Index,
                                new ReportInfo
                                {
                                    SteamID = SteamUtil.Parse(Instance.Options.Target),
                                    MatchID = SharecodeUtil.Parse(Instance.Options.MatchID),
                                    
                                    // TODO: Maybe make this customizeable
                                    AbusiveText = true,
                                    AbusiveVoice = true,
                                    Griefing = true,
                                    AimHacking = true,
                                    WallHacking = true,
                                    OtherHacking = true
                                });
                            break;
                        case "commend":
                            Instance.AccountManager.StartCommending(Instance.AccountManager.Index,
                                new CommendInfo
                                {
                                    SteamID = SteamUtil.Parse(Instance.Options.Target),
                                    
                                    Friendly = true,
                                    Leader = true,
                                    Teacher = true
                                });
                            break;
                        default:
                            Log.Error("Could not parse {Mode} to Mode.", Instance.Options.Mode);
                            
                            Instance.UIManager.ShowForm(UIType.Main);
                            break;
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