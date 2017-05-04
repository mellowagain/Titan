using System;
using System.IO;
using System.Threading;
using CommandLine;
using Eto.Forms;
using Serilog;
using Serilog.Core;
using Titan.Bootstrap;
using Titan.Bot;
using Titan.Bot.Mode;
using Titan.Bot.Threads;
using Titan.Logging;
using Titan.UI;

namespace Titan
{
    public sealed class Titan
    {

        public static Logger Logger = LogCreator.Create(); // Global logger
        public static Titan Instance;

        public Application EtoApp;
        public Options Options;
        public bool EnableUI = true;

        public AccountManager AccountManager;
        public ThreadManager ThreadManager;

        public MainForm MainForm;

        [STAThread]
        public static void Main(string[] args)
        {
            ShutdownHook.Hook();
            Thread.CurrentThread.Name = "Main";

            Logger.Debug("Initializing libraries...");

            /* Initialize libraries: Eto.Forms, SteamKit2, SteamKit-CSGO */
            Instance = new Titan
            {
                EtoApp = new Application(),
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
                Logger.Information("The arguments --target, --mode and/or --id were omitted - opening the UI.");
                Instance.EnableUI = true;
            }

            Instance.MainForm = new MainForm();

            var file = string.IsNullOrEmpty(Instance.Options.File) ? "accounts.json" : Instance.Options.File;
            Instance.AccountManager = new AccountManager(new FileInfo(Path.Combine(Environment.CurrentDirectory, file)));
            Instance.ThreadManager = new ThreadManager();

            if(Instance.AccountManager.ParseAccountFile())
            {
                Logger.Information("Hello and welcome to Titan.");

                if(Instance.EnableUI)
                {
                    Instance.EtoApp.Run(Instance.MainForm);
                }
                else
                {
                    var mode = ModeParser.Parse(Instance.Options.Mode);

                    if(mode == BotMode.Report)
                    {
                        if(string.IsNullOrEmpty(Instance.Options.MatchId))
                        {
                            Logger.Error("Match ID was not provided when starting Titan!");
                            Instance.EtoApp.Run(Instance.MainForm);
                            return;
                        }

                        Instance.AccountManager.StartBotting(mode, Instance.Options.Target, Instance.Options.MatchId);
                    }
                }
            }

            Instance.AccountManager.SaveIndexFile();

            Log.CloseAndFlush();

            Console.WriteLine("Thank you and have a nice day.");
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