using System;
using System.Net;
using System.Threading;
using Eto.Forms;
using log4net;
using log4net.Config;
using Titan.Core;
using Titan.Protobufs;
using Titan.UI;

namespace Titan
{
    public sealed class Titan
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(Titan));
        public static Titan Instance;

        public Application EtoApp;
        public Options Options;
        public bool EnableUI = true;

        public MainForm MainForm;

        [STAThread]
        public static void Main(string[] args)
        {

            Thread.CurrentThread.Name = "Main";
            BasicConfigurator.Configure();

            Log.Info("Initializing libraries...");

            /* Initialize libraries: Eto.Forms, SteamKit2, SteamKit-CSGO */
            Instance = new Titan
            {
                EtoApp = new Application(),
                Options = new Options()
            };

            Log.Info("Parse arguments");

            /* Parse arguments provided with the starting of this */
            if(CommandLine.Parser.Default.ParseArguments(args, Instance.Options))
            {
                Log.InfoFormat("Skipping UI and going directly to botting - Target: {0} - Match ID: {1}", Instance.Options.Target, Instance.Options.MatchId);
                Instance.EnableUI = false;
            }
            else
            {
                Log.Info("The arguments --target and --id were omitted - opening the UI.");
                Instance.EnableUI = true;
            }

            Log.Debug("Checking if Protobufs require updates");

            if(Updater.RequiresUpdate() || Instance.Options.ForceUpdate)
            {
                Log.Info("Protobufs require update. Updating...");
                try
                {
                    Updater.Update();
                }
                catch (WebException ex)
                {
                    Log.Error("A error occured while updating Protobufs.", ex);
                }
            }

            if(Hub.ReadFile())
            {
                Log.Info("Welcome to Titan v1.0.0.");

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

        /* Limitations of this report bot
         * > The CS:GO game coordinator doesn't accepts games 10 minutes after game end.
         * > The Share Code parser is a Node.js library (node-csgo) and hasn't been ported to C#.
         * You need to parse the Match ID from Sharecode by yourself. The "links" tab provides a website for that.
         * > Reports and Commendations are only possible once per 6 hours.
         */

    }
}