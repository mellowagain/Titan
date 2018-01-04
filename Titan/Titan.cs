using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using CommandLine;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Core;
using SteamKit2;
using Titan.Account;
using Titan.Bootstrap;
using Titan.Bootstrap.Verbs;
using Titan.Logging;
using Titan.Managers;
using Titan.Meta;
using Titan.Restrictions;
using Titan.UI;
using Titan.Util;
using Titan.Web;

#if __UNIX__
    using Mono.Unix.Native;
#else
    using System.Security.Principal;
#endif 

namespace Titan
{
    public sealed class Titan
    {

        public static Logger Logger; // Global logger
        public static Titan Instance;

        public Options Options;
        public bool IsAdmin;
        public bool EnableUI = true;
        public object ParsedObject;

        public AccountManager AccountManager;
        public ThreadManager ThreadManager;
        public VictimTracker VictimTracker;
        public UIManager UIManager;

        public JsonSerializer JsonSerializer;
        public SWAHandle WebHandle;

        public bool DummyMode = false;
        public IScheduler Scheduler;

        public DirectoryInfo Directory => new DirectoryInfo(
            Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? Environment.CurrentDirectory
        );
        public DirectoryInfo DebugDirectory;

        [STAThread]
        public static int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            Instance = new Titan
            {
                Options = new Options()
            };

            Logger = LogCreator.Create();
            
            #if __UNIX__
                Instance.IsAdmin = Syscall.getuid() == 0; // UID of root is always 0
            #else
                Instance.IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                                   .IsInRole(WindowsBuiltInRole.Administrator);
            #endif
            
            if (Instance.IsAdmin)
            {
                Logger.Error("Titan is running as administrator or root.");
                Logger.Error("This is not supported. Titan will refuse to start until you start it as normal user.");
                
                #if !__UNIX__
                    Console.Write("Press any key to exit Titan...");
                    Console.Read();
                #endif
                
                return -1;
            }
            
            Logger.Debug("Titan was called from: {dir}", Environment.CurrentDirectory);
            Logger.Debug("Working in directory: {dir}", Instance.Directory.ToString());
            
            // Workaround for Mono related issue regarding System.Net.Http.
            // More detail: https://github.com/dotnet/corefx/issues/19914

            var systemNetHttpDll = new FileInfo(Path.Combine(Instance.Directory.ToString(), "System.Net.Http.dll"));
            
            if (systemNetHttpDll.Exists && !PlatformUtil.IsWindows())
            {
                systemNetHttpDll.Delete();
            }

            // Windows users run the program by double clicking Titan.exe (and then it opens a console window)
            // and in case of exception occurence, this window gets immediatly closed which is bad because
            // they're unable to attach the stacktrace then. Prevent it by waiting until the user presses a key.
            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
            {
                if (eventArgs.IsTerminating)
                {
                    #if !__UNIX__
                        Console.Write("Press any key to exit Titan...");
                        Console.Read();
                    #endif
                }
            };
            
            Logger.Debug("Startup: Loading Serilog <-> Common Logging Bridge.");
            
            // The bridge between Common Logging and Serilog uses the global Logger (Log.Logger).
            // As Quartz.NET is the only dependency using Common Logging (and because of our bridge the global logger)
            // we're creating the global logger as Quartz logger (which hides annoying debug messages).
            Log.Logger = LogCreator.CreateQuartzLogger();
            
            Logger.Debug("Startup: Loading Quartz.NET.");
            
            // Quartz.NET
            Instance.Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            Instance.Scheduler.Start();

            Logger.Debug("Startup: Parsing Command Line Arguments.");

            // Default
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    Instance.Options = options;
                });
            
            // Verbs
            Parser.Default.ParseArguments<ReportOptions, CommendOptions>(args)
                .WithParsed<ReportOptions>(options =>
                {
                    Instance.EnableUI = false;
                    Instance.ParsedObject = options;
                })
                .WithParsed<CommendOptions>(options =>
                {
                    Instance.EnableUI = false;
                    Instance.ParsedObject = options;
                })
                .WithNotParsed(error =>
                {
                    Instance.EnableUI = true;
                    Logger.Information("No valid verb has been provided while parsing. Opening UI...");
                });
            
            // Reinitialize logger with new parsed debug option
            Logger = LogCreator.Create();

            if (Instance.Options.Debug)
            {
                Instance.DebugDirectory = new DirectoryInfo(Path.Combine(Instance.Directory.ToString(), "debug"));
                
                if (!Instance.DebugDirectory.Exists)
                {
                    Instance.DebugDirectory.Create();
                }

                DebugLog.AddListener(new TitanListener());
                DebugLog.Enabled = true;
            }

            if (Instance.Options.Secure)
            {
                Logger.Debug("Secure mode has been enabled. Titan will output no sensitive data.");
            }
            
            if (Instance.Options.DisableBlacklist)
            {
                Logger.Debug("Blacklist has been disabled by passing the --noblacklist option.");
            }

            Logger.Debug("Startup: Loading UI Manager, Victim Tracker, Account Manager and Ban Manager.");

            Instance.JsonSerializer = new JsonSerializer();
            
            try
            {
                Instance.UIManager = new UIManager();
            }
            catch (InvalidOperationException ex)
            {
                if (!string.IsNullOrEmpty(ex.Message) && ex.Message.ToLower().Contains("could not detect platform"))
                {
                    Logger.Error("---------------------------------------");
                    Logger.Error("A fatal error has been detected!");
                    Logger.Error("Eto.Forms could not detect your current operating system.");
                    if (Type.GetType("Mono.Runtime") != null)
                    {
                        Logger.Error("Please install {0}, {1}, {2}, {3} and {4} before submitting a bug report.",
                                     "Mono (\u22655.4)", 
                                     "Gtk 3",
                                     "Gtk# 3 (GTK Sharp)",
                                     "libNotify",
                                     "libAppindicator3");
                    }
                    else
                    {
                        Logger.Error("Please install {0} before submitting a bug report.", 
                                     ".NET Framework (\u22654.6.1)");
                    }
                    Logger.Error("Contact {Marc} on Discord if the issue still persists after installing " +
                                 "the dependencies listed above.", "Marc3842h#7312");
                    Logger.Error("---------------------------------------");
                    Logger.Debug(ex, "Include the error below if you\'re contacting Marc on Discord.");

                    #if !__UNIX__
                        Console.Write("Press any key to exit Titan...");
                        Console.Read();
                    #endif
                    
                    Instance.Scheduler.Shutdown();
                    return -1;
                }
                
                Logger.Error(ex, "A error occured while loading UI.");
                throw;
            }

            Instance.VictimTracker = new VictimTracker();
            
            Instance.Scheduler.ScheduleJob(Instance.VictimTracker.Job, Instance.VictimTracker.Trigger);

            Instance.AccountManager = new AccountManager(new FileInfo(
                Path.Combine(Instance.Directory.ToString(), Instance.Options.AccountsFile))
            );

            Instance.ThreadManager = new ThreadManager();

            Instance.WebHandle = new SWAHandle();
            
            Logger.Debug("Startup: Registering Shutdown Hook.");

            AppDomain.CurrentDomain.ProcessExit += OnShutdown;

            Logger.Debug("Startup: Parsing accounts.json file.");

            Instance.AccountManager.ParseAccountFile(); 
            
            Logger.Debug("Startup: Initializing Forms...");

            Instance.UIManager.InitializeForms();
            
            // Load after Forms were initialized
            Instance.WebHandle.Load();

            Logger.Information("Hello and welcome to Titan v1.6.0-EAP.");

            if (Instance.EnableUI || Instance.ParsedObject == null || Instance.DummyMode)
            {
                Instance.UIManager.ShowForm(UIType.General);
            }
            else
            {
                if (Instance.ParsedObject.GetType() == typeof(ReportOptions))
                {
                    var opt = (ReportOptions) Instance.ParsedObject;

                    var steamID = SteamUtil.Parse(opt.Target);
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
                        Instance.AccountManager.StartReporting(Instance.AccountManager.Index,
                            new ReportInfo
                            {
                                SteamID = SteamUtil.Parse(opt.Target),
                                MatchID = SharecodeUtil.Parse(opt.Match),
                                AppID = TitanAccount.CSGO_APPID,

                                AbusiveText = opt.AbusiveTextChat,
                                AbusiveVoice = opt.AbusiveVoiceChat,
                                Griefing = opt.Griefing,
                                AimHacking = opt.AimHacking,
                                WallHacking = opt.WallHacking,
                                OtherHacking = opt.OtherHacking
                            });
                    }
                }
                else if (Instance.ParsedObject.GetType() == typeof(CommendOptions))
                {
                    var opt = (CommendOptions) Instance.ParsedObject;

                    Instance.AccountManager.StartCommending(Instance.AccountManager.Index,
                        new CommendInfo
                        {
                            SteamID = SteamUtil.Parse(opt.Target),
                            AppID = TitanAccount.CSGO_APPID,

                            Friendly = opt.Friendly,
                            Leader = opt.Leader,
                            Teacher = opt.Teacher
                        });
                }
                else
                {
                    Instance.UIManager.ShowForm(UIType.General);
                }
            }

            Instance.UIManager.StartMainLoop();
            
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
            if (!Instance.Scheduler.IsShutdown)
            {
                Instance.Scheduler.Shutdown();
            }
            
            Instance.UIManager.Destroy();
            Instance.ThreadManager.FinishBotting();
            Instance.AccountManager.SaveAccountsFile();
            Instance.VictimTracker.SaveVictimsFile();
            Instance.WebHandle.Save();
            Instance.AccountManager.SaveIndexFile();

            Logger.Information("Thank you and have a nice day.");

            Log.CloseAndFlush();
        }

    }
}