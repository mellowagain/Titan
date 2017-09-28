using System;
using System.IO;
using System.Threading;
using CommandLine;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Core;
using Titan.Bootstrap;
using Titan.Logging;
using Titan.Managers;
using Titan.Meta;
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
        public object ParseMode;

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
            Instance.Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            Instance.Scheduler.Start();

            Logger.Debug("Startup: Parsing Command Line Arguments.");

            Parser.Default.ParseArguments<Options.ReportOptions, Options.CommendOptions, Options.IdleOptions>(args)
                .WithParsed<Options.ReportOptions>(options =>
                {
                    Instance.EnableUI = false;
                    Instance.ParseMode = options;
                })
                .WithParsed<Options.CommendOptions>(options =>
                {
                    Instance.EnableUI = false;
                    Instance.ParseMode = options;
                })
                .WithParsed<Options.IdleOptions>(options =>
                {
                    Instance.EnableUI = false;
                    Instance.ParseMode = options;
                })
                .WithNotParsed(error =>
                {
                    Instance.EnableUI = true;
                    Logger.Information("No valid verb has been provided while parsing. Opening UI...");
                });
            
            // Reinitialize logger with new parsed debug option
            Logger = LogCreator.Create();

            if(Instance.Options.Debug)
            {
                if(!Instance.DebugDirectory.Exists)
                {
                    Instance.DebugDirectory.Create();
                }
            }

            Logger.Debug("Startup: Loading UI Manager, Victim Tracker, Account Manager and Ban Manager.");

            try
            {
                Instance.UIManager = new UIManager();
            }
            catch (Exception ex)
            {
                /*if (ex.GetType() == typeof(InvalidOperationException))
                {
                    var osEx = (InvalidOperationException) ex;

                    if (osEx.Message.ToLower().Contains("could not detect platform"))
                    {
                        Logger.Error("---------------------------------------");
                        Logger.Error("A fatal error has been detected!");
                        Logger.Error("You are missing a Eto.Forms platform assembly. Please install dependencies.");
                        Logger.Error("Either {0} or {1} Titan. Titan will now shutdown.", "redownload", "rebuild");
                        Logger.Error("Contact {Marc} on Discord for more information.", "Marc3842h#7312");
                        Logger.Error("---------------------------------------");

                        Environment.Exit(-1);
                    }
                }*/
                
                Log.Error(ex, "A error occured while loading UI.");
                throw;
            }

            Instance.VictimTracker = new VictimTracker();
            
            Instance.Scheduler.ScheduleJob(Instance.VictimTracker.Job, Instance.VictimTracker.Trigger);

            Instance.AccountManager = new AccountManager(new FileInfo(
                Path.Combine(Environment.CurrentDirectory, Instance.Options.AccountsFile))
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

                if(Instance.EnableUI || Instance.ParseMode == null)
                {
                    Instance.UIManager.ShowForm(UIType.General);
                }
                else
                {
                    if (Instance.ParseMode.GetType() == typeof(Options.ReportOptions))
                    {
                        var opt = (Options.ReportOptions) Instance.ParseMode;
                        
                        Instance.AccountManager.StartReporting(Instance.AccountManager.Index,
                            new ReportInfo
                            {
                                SteamID = SteamUtil.Parse(opt.Target),
                                MatchID = SharecodeUtil.Parse(opt.MatchID),
                                    
                                AbusiveText = opt.AbusiveTextChat,
                                AbusiveVoice = opt.AbusiveVoiceChat,
                                Griefing = opt.Griefing,
                                AimHacking = opt.AimHacking,
                                WallHacking = opt.WallHacking,
                                OtherHacking = opt.OtherHacking
                            });
                    } 
                    else if (Instance.ParseMode.GetType() == typeof(Options.CommendOptions))
                    {
                        var opt = (Options.CommendOptions) Instance.ParseMode;
                        
                        Instance.AccountManager.StartCommending(Instance.AccountManager.Index,
                            new CommendInfo
                            {
                                SteamID = SteamUtil.Parse(opt.Target),
                                    
                                Friendly = opt.Friendly,
                                Leader = opt.Leader,
                                Teacher = opt.Teacher
                            });
                    }
                    else if (Instance.ParseMode.GetType() == typeof(Options.IdleOptions))
                    {
                        var opt = (Options.IdleOptions) Instance.ParseMode;
                        // TODO
                    }
                    else
                    {
                        Logger.Error("Could not parse {@ParseMode} to valid ParseMode.", Instance.ParseMode.GetType());
                            
                        Instance.UIManager.ShowForm(UIType.General);
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