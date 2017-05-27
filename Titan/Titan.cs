using System;
using System.IO;
using System.Threading;
using CommandLine;
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
        public BanManager BanManager;
        public UIManager UIManager;

        [STAThread]
        public static int Main(string[] args)
        {
            Thread.CurrentThread.Name = "Main";

            Logger.Debug("Initializing libraries...");

            /* Initialize libraries: Eto.Forms, SteamKit2, SteamKit-CSGO */
            Instance = new Titan
            {
                Options = new Options()
            };

            Logger.Debug("Parsing arguments");

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

            Logger.Debug("Refreshing Steam server list");

            SteamDirectory.Initialize().Wait();

            Instance.UIManager = new UIManager();

            var file = string.IsNullOrEmpty(Instance.Options.File) ? "accounts.json" : Instance.Options.File;
            Instance.AccountManager = new AccountManager(new FileInfo(Path.Combine(Environment.CurrentDirectory, file)));

            Instance.ThreadManager = new ThreadManager();

            Instance.BanManager = new BanManager();
            Instance.BanManager.ParseApiKeyFile();

            // Register hook
            AppDomain.CurrentDomain.ProcessExit += OnShutdown;

            if(Instance.AccountManager.ParseAccountFile())
            {
                Logger.Information("Hello and welcome to Titan v1.4.0.");

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

            return 0; // OK.
        }

        public static void OnShutdown(object sender, EventArgs args)
        {
            // Cleanup a few things before shutting down

            Instance.BanManager.SaveAPIKeyFile();
            Instance.AccountManager.SaveIndexFile();

            Logger.Information("Thank you and have a nice day.");

            Log.CloseAndFlush();
        }

        /*
         * Limitations of this report bot
         * > The CS:GO game coordinator doesn't accepts games 10 minutes after game end.
         * > The Share Code parser is a Node.js library (node-csgo) and hasn't been ported to C#.
         * You need to parse the Match ID from Sharecode by yourself. The "links" tab provides a website for that.
         * > Reports and Commendations are only possible once per 6 hours.
         */

    }
}