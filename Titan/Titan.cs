using System;
using System.Net;
using System.Threading;
using Eto.Forms;
using Serilog.Core;
using Titan.Bootstrap;
using Titan.Bot;
using Titan.Bot.Mode;
using Titan.Logging;
using Titan.Protobufs;
using Titan.UI;

namespace Titan
{
    public sealed class Titan
    {

        public static readonly Logger Logger = LogCreator.Create(); // Global logger
        public static Titan Instance;

        public Application EtoApp;
        public Options Options;
        public bool EnableUI = true;

        public MainForm MainForm;

        [STAThread]
        public static void Main(string[] args)
        {
            ShutdownHook.Hook();
            Thread.CurrentThread.Name = "Main";

            Logger.Information("Initializing libraries...");

            /* Initialize libraries: Eto.Forms, SteamKit2, SteamKit-CSGO */
            Instance = new Titan
            {
                EtoApp = new Application(),
                Options = new Options()
            };

            Logger.Information("Parse arguments");

            /* Parse arguments provided with the starting of this */
            if(CommandLine.Parser.Default.ParseArguments(args, Instance.Options))
            {
                Logger.Information("Skipping UI and going directly to botting - Target: {Target} - Match ID: {Id}", Instance.Options.Target, Instance.Options.MatchId);
                Instance.EnableUI = false;
            }
            else
            {
                Logger.Information("The arguments --target and --id were omitted - opening the UI.");
                Instance.EnableUI = true;
            }

            Logger.Debug("Checking if Protobufs require updates");

            if(Updater.RequiresUpdate() || Instance.Options.ForceUpdate)
            {
                Logger.Information("Protobufs require update. Updating...");
                try
                {
                    Updater.Update();
                }
                catch (WebException ex)
                {
                    Logger.Error("A error occured while updating Protobufs.", ex);
                }
            }

            if(Hub.ReadFile())
            {
                Logger.Information("Welcome to Titan v1.0.0.");

                if(Instance.EnableUI)
                {
                    Instance.EtoApp.Run(Instance.MainForm = new MainForm());
                }
                else
                {
                    var mode = ModeParser.Parse(Instance.Options.Mode);

                    if(mode == BotMode.Report)
                    {
                        if(string.IsNullOrEmpty(Instance.Options.MatchId))
                        {
                            MessageBox.Show("Please provide a Match ID when starting " +
                                            "via command line and mode \"REPORT\".",
                                "Titan - Error", MessageBoxType.Error);
                            Instance.EtoApp.Run(Instance.MainForm = new MainForm());
                            return;
                        }
                    }

                    Hub.StartBotting(Instance.Options.Target, Instance.Options.MatchId, mode);
                }
            }

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