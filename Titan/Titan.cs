using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Serilog;
using Serilog.Core;
using SteamAuth;
using SteamKit2;
using Titan.Account;
using Titan.Bootstrap;
using Titan.Bootstrap.Verbs;
using Titan.Logging;
using Titan.Managers;
using Titan.Meta;
using Titan.Proof;
using Titan.Restrictions;
using Titan.UI;
using Titan.Util;
using Titan.Web;

#if __UNIX__
    using Titan.Native;
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
        public bool IsBotting = true;

        public AccountManager AccountManager;
        public ThreadManager ThreadManager;
        public VictimTracker VictimTracker;
        public UIManager UIManager;

        public JsonSerializer JsonSerializer;
        public HttpClient HttpClient;
        
        public SWAHandle WebHandle;
        public ProfileSaver ProfileSaver;

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
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    Logger.Fatal("Titan has been compiled to run under Linux but is running on " +
                                 "Windows. Please use the correct Titan version for your operating system. Exiting.");
                    return (int) ExitCodes.WrongOS;
                }
            #else
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Logger.Fatal("Titan has been compiled to run under Windows but is running on " +
                                 "Linux. Please use the correct Titan version for your operating system. Exiting.");
                    return (int) ExitCodes.WrongOS;
                }
            #endif

            if (Environment.CurrentDirectory != Instance.Directory.ToString())
            {
                Logger.Debug("Run from {currentDir}, switching to work directory in {workingDir}.",
                             Environment.CurrentDirectory, Instance.Directory.ToString());
            }

            // Windows users run the program by double clicking Titan.exe (and then it opens a console window)
            // and in case of exception occurence, this window gets immediatly closed which is bad because
            // they're unable to attach the stacktrace then. Prevent it by waiting until the user presses a key.
            #if !__UNIX__
                AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                {
                    if (eventArgs.IsTerminating)
                    {
                        Logger.Error((Exception) eventArgs.ExceptionObject, "An error occured.");
                        
                        // Don't use logging object here incase the exception was thrown by a logger
                        Console.Write("Press any key to exit Titan...");
                        Console.Read();
                    }
                };
            #endif
            
            // The bridge between Common Logging and Serilog uses the global Logger (Log.Logger).
            // As Quartz.NET is the only dependency using Common Logging (and because of our bridge the global logger)
            // we're creating the global logger as Quartz logger (which hides annoying debug messages).
            Log.Logger = LogCreator.CreateQuartzLogger();
            
            // Quartz.NET
            Instance.Scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
            Instance.Scheduler.Start();
            
            Instance.JsonSerializer = new JsonSerializer();
            
            Instance.HttpClient = new HttpClient();
            Instance.HttpClient.DefaultRequestHeaders.Add(
                "User-Agent", "Titan Report & Commend Bot (https://github.com/Marc3842h/Titan)"
            );

            var parser = new Parser(config =>
            {
                config.IgnoreUnknownArguments = true;
                config.EnableDashDash = true;
                config.HelpWriter = TextWriter.Null;
            });
            
            // Default
            parser.ParseArguments<Options>(args)
                .WithParsed(options =>
                {
                    Instance.Options = options;
                });
            
            // Verbs
            parser.ParseArguments<ReportOptions, CommendOptions>(args)
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
                    if (Instance.ParsedObject == null)
                    {
                        Instance.EnableUI = true;
                        Logger.Information("No valid verb has been provided while parsing. Opening UI...");
                    }
                });
            
            new Config.Config().Load();
            
            // Reinitialize logger with new parsed debug option
            Logger = LogCreator.Create();
            
            #if __UNIX__
                Instance.IsAdmin = Linux.getuid() == 0; // UID of root is always 0
            #else
                Instance.IsAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                                   .IsInRole(WindowsBuiltInRole.Administrator);
            #endif
            
            if (Instance.IsAdmin)
            {
                if (!Instance.Options.AllowAdmin)
                {
                    Logger.Error("Titan is running as administrator or root.");
                    Logger.Error("This is not supported. Titan will refuse to start until you start it as normal " +
                                 "user. If you are unable to do this for any reason, start Titan with the --admin " +
                                 "option to force the usage of administrator rights.");

                    #if !__UNIX__
                        Console.Write("Press any key to exit Titan...");
                        Console.Read();
                    #endif

                    Instance.Scheduler.Shutdown();
                    return (int) ExitCodes.RunningAsAdmin;
                }

                Logger.Warning("Titan has been started as Administrator but will continue to run as the " +
                               "--admin option has been passed. Please note that Steam also doesn't allow to be " +
                               "run from root and that it may be insecure.");
            }

            if (Instance.Options.Debug)
            {
                Instance.DebugDirectory = new DirectoryInfo(Path.Combine(Instance.Directory.ToString(), "debug"));
                
                if (!Instance.DebugDirectory.Exists)
                {
                    Instance.DebugDirectory.Create();
                }

                if (Instance.Options.SteamKitDebug)
                {
                    DebugLog.AddListener(new TitanListener());
                    DebugLog.Enabled = true;
                }
            }

            if (Instance.Options.Secure)
            {
                Logger.Debug("Secure mode has been enabled. Titan will output no sensitive data.");
            }
            
            if (Instance.Options.DisableBlacklist)
            {
                Logger.Debug("Blacklist has been disabled by passing the --noblacklist option.");
            }

            Instance.ProfileSaver = new ProfileSaver();

            if (Instance.EnableUI)
            {
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

                        #if __UNIX__
                            Logger.Error("Please install {0}, {1}, {2} and {3} before submitting a bug report.",
                                "Mono (\u22655.4)",
                                "Gtk 3",
                                "libNotify",
                                "libAppindicator3");
                        #else
                            Logger.Error("Please install {0} before submitting a bug report.", 
                                         ".NET Framework (\u22654.6.1)");
                        #endif

                        Logger.Error("Contact {Marc} on Discord if the issue still persists after installing " +
                                     "the dependencies listed above.", "Marc3842h#7312");
                        Logger.Error("---------------------------------------");
                        Logger.Debug(ex, "Include the error below if you\'re contacting Marc on Discord.");

                        #if !__UNIX__
                            Console.Write("Press any key to exit Titan...");
                            Console.Read();
                        #endif

                        Instance.Scheduler.Shutdown();
                        return (int) ExitCodes.UIInitFailed;
                    }

                    Logger.Error(ex, "A error occured while loading UI.");
                    throw;
                }
            }


            Instance.AccountManager = new AccountManager(new FileInfo(
                Path.Combine(Instance.Directory.ToString(), Instance.Options.AccountsFile))
            );

            Instance.ThreadManager = new ThreadManager();

            Instance.WebHandle = new SWAHandle();
            Instance.WebHandle.Preload();

            Instance.VictimTracker = new VictimTracker();

            AppDomain.CurrentDomain.ProcessExit += OnShutdown;

            Instance.AccountManager.ParseAccountFile(); 

            Task.Run(() => TimeAligner.AlignTime());

            if (Instance.EnableUI)
            {
                Instance.UIManager.InitializeForms();
            }

            // Load after Forms were initialized
            Instance.WebHandle.Load();
            
            // VictimTracker depends on the web api key being loaded correctly.
            Instance.VictimTracker.InitTrigger();

            var attribute = Assembly.GetEntryAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>(); 
            var version = attribute != null ? attribute.InformationalVersion : 
                                              Assembly.GetEntryAssembly().GetName().Version.Major + "." +
                                              Assembly.GetEntryAssembly().GetName().Version.Minor + "." +
                                              Assembly.GetEntryAssembly().GetName().Version.Build;
            
            Logger.Information("Hello and welcome to Titan {version}.", "v" + version);

            if (Instance.EnableUI && Instance.ParsedObject == null || Instance.DummyMode)
            {
                Instance.UIManager.ShowForm(UIType.General);
            }
            else
            {
                if (Instance.ParsedObject.GetType() == typeof(ReportOptions))
                {
                    var opt = (ReportOptions) Instance.ParsedObject;

                    var steamID = SteamUtil.Parse(opt.Target);
                    if (steamID.IsBlacklisted(opt.Game.ToAppID()))
                    {
                        Instance.UIManager.SendNotification(
                            "Restriction applied",
                            "The target you are trying to report is blacklisted from botting " +
                            "in Titan.",
                            () => Process.Start("https://github.com/Marc3842h/Titan/wiki/Blacklist")
                        );
                    }
                    else
                    {
                        Instance.AccountManager.StartReporting(Instance.AccountManager.Index,
                            new ReportInfo
                            {
                                SteamID = SteamUtil.Parse(opt.Target),
                                MatchID = SharecodeUtil.Parse(opt.Match),
                                AppID = opt.Game.ToAppID(),

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

            Instance.Scheduler.ScheduleJob(Instance.VictimTracker.Job, Instance.VictimTracker.Trigger);
            
            Logger.Debug("Startup done. Active threads: {threads}", Process.GetCurrentProcess().Threads.Count + 1);
            
            Instance.StartMainLoop();

            // The Shutdown handler gets only called after the last thread finished.
            // Quartz runs a Watchdog until Scheduler#Shutdown is called, so we're calling it
            // before Titan will be calling the Shutdown Hook.
            Logger.Debug("Shutting down Quartz.NET Scheduler.");
            
            Instance.Scheduler.Shutdown();
            
            return (int) ExitCodes.Ok;
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

        public void StartMainLoop()
        {
            if (Instance.EnableUI)
            {
                Instance.UIManager.StartMainLoop();
            }
            else
            {
                // Titan was run in CLI mode so just run infinitely until the background threads
                // finish and abort this loop and the whole application
                while (Instance.IsBotting)
                {
                    Thread.Yield();
                }
                
                Instance.Scheduler.Shutdown();
                //OnShutdown(null, null);
                Environment.Exit((int) ExitCodes.Ok);
            }
        }

    }
}
