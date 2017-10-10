using System;
using System.Diagnostics;
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
using Titan.Restrictions;
using Titan.UI;
using Titan.Util;
using Titan.Web;

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

        public WebAPIKeyResolver APIKeyResolver;

        public IScheduler Scheduler;

        public DirectoryInfo DebugDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "debug"));

        [STAThread]
        public static int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            Instance = new Titan
            {
                Options = new Options()
            };

            Logger = LogCreator.Create();
            
            Logger.Debug("Startup: Loading Serilog <-> Common Logging Bridge.");
            
            // Common Logging <-> Serilog bridge
            Log.Logger = LogCreator.Create("Quartz.NET Scheduler");
            
            Logger.Debug("Startup: Loading Quartz.NET.");
            
            // Quartz.NET
            Instance.Scheduler = StdSchedulerFactory.GetDefaultScheduler();
            Instance.Scheduler.Start();
            
            Logger.Debug("Startup: Refreshing Steam Universe list.");
            
            SteamDirectory.Initialize().Wait();

            Logger.Debug("Startup: Parsing Command Line Arguments.");

            if(Parser.Default.ParseArguments(args, Instance.Options))
            {
                Logger.Information("Skipping UI and going directly to botting - Target: {Target} - Match ID: {ID}", 
                                    Instance.Options.Target, Instance.Options.MatchID);
                Instance.EnableUI = false;
            }
            else
            {
                Logger.Information("The arguments --target and --mode were omitted - opening the UI.");
                Instance.EnableUI = true;
            }
            
            // Reinitialize logger with new parsed debug option
            Logger = LogCreator.Create();

            if(Instance.Options.Debug)
            {
                if(!Instance.DebugDirectory.Exists)
                {
                    Instance.DebugDirectory.Create();
                }
            }

            if (Instance.Options.DisableBlacklist)
            {
                Logger.Debug("Blacklist has been disabled by passing the --noblacklist option.");
            }

            Logger.Debug("Startup: Loading UI Manager, Victim Tracker, Account Manager and Ban Manager.");

            try
            {
                Instance.UIManager = new UIManager();
            }
            catch (Exception ex)
            {
                if (ex.GetType() == typeof(InvalidOperationException))
                {
                    var osEx = (InvalidOperationException) ex;

                    if (osEx.Message.ToLower().Contains("could not detect platform"))
                    {
                        Log.Error("---------------------------------------");
                        Log.Error("A fatal error has been detected!");
                        Log.Error("You are missing a Eto.Forms platform assembly.");
                        if (Type.GetType("Mono.Runtime") != null)
                        {
                            Log.Error("Please read the README.md file and install all required dependencies.");
                        }
                        Log.Error("Either {0} or {1} Titan. Titan will now shutdown.", "redownload", "rebuild");
                        Log.Error("Contact {Marc} on Discord for more information.", "Marc3842h#7312");
                        Log.Error("---------------------------------------");

                        Environment.Exit(-1);
                    }
                }
                
                Log.Error(ex, "A error occured while loading UI.");
                throw;
            }

            Instance.VictimTracker = new VictimTracker();
            
            Instance.Scheduler.ScheduleJob(Instance.VictimTracker.Job, Instance.VictimTracker.Trigger);

            Instance.AccountManager = new AccountManager(new FileInfo(
                Path.Combine(Environment.CurrentDirectory, Instance.Options.File))
            );

            Instance.ThreadManager = new ThreadManager();

            Instance.BanManager = new BanManager();
            
            Logger.Debug("Startup: Registering Shutdown Hook.");

            AppDomain.CurrentDomain.ProcessExit += OnShutdown;

            Logger.Debug("Startup: Parsing accounts.json file.");
            
            if(Instance.AccountManager.ParseAccountFile())
            {
                Logger.Debug("Initializing Forms...");
                
                Instance.UIManager.InitializeForms();
                
                
                Logger.Debug("Startup: Loading Web API Key");
                
                // Resolve API Key File
                Instance.APIKeyResolver = new WebAPIKeyResolver();
                Instance.APIKeyResolver.ParseKeyFile();
                
                Logger.Information("Hello and welcome to Titan v1.5.0-Dev.");

                if(Instance.EnableUI)
                {
                    Instance.UIManager.ShowForm(UIType.General);
                }
                else
                {
                    var steamID = SteamUtil.Parse(Instance.Options.Target);
                    
                    if (Blacklist.IsBlacklisted(steamID))
                    {
                        Instance.UIManager.SendNotification(
                            "Restriction applied",
                            "The target you are trying to report is blacklisted from botting " +
                            "in Titan.",
                            delegate { Process.Start("https://github.com/Marc3842h/Titan/wiki/Blacklist"); }
                        );
                    }
                    else
                    {
                        switch (Regex.Replace(Instance.Options.Mode.ToLowerInvariant(), @"\s+", ""))
                        {
                            case "report":
                                Instance.AccountManager.StartReporting(Instance.AccountManager.Index,
                                    new ReportInfo
                                    {
                                        SteamID = steamID,
                                        MatchID = SharecodeUtil.Parse(Instance.Options.MatchID),

                                        AbusiveText = Instance.Options.AbusiveTextChat,
                                        AbusiveVoice = Instance.Options.AbusiveVoiceChat,
                                        Griefing = Instance.Options.Griefing,
                                        AimHacking = Instance.Options.AimHacking,
                                        WallHacking = Instance.Options.WallHacking,
                                        OtherHacking = Instance.Options.OtherHacking
                                    });
                                break;
                            case "commend":
                                Instance.AccountManager.StartCommending(Instance.AccountManager.Index,
                                    new CommendInfo
                                    {
                                        SteamID = steamID,

                                        Friendly = Instance.Options.Friendly,
                                        Leader = Instance.Options.Leader,
                                        Teacher = Instance.Options.Teacher
                                    });
                                break;
                            default:
                                Logger.Error("Could not parse {Mode} to Mode.", Instance.Options.Mode);

                                Instance.UIManager.ShowForm(UIType.General);
                                break;
                        }
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
            // Check if Titan got closed via Process Manager or by the TrayIcon
            if(!Instance.Scheduler.IsShutdown)
            {
                Instance.Scheduler.Shutdown();
            }
            
            Instance.VictimTracker.SaveVictimsFile();
            Instance.APIKeyResolver.SaveKeyFile();
            Instance.AccountManager.SaveIndexFile();

            Logger.Information("Thank you and have a nice day.");

            Log.CloseAndFlush();
        }

    }
}