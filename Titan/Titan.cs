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
                Log.Info("The arguments --target (or --id) were omitted - opening the UI.");
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

            Hub.ReadFile();

            if(Instance.EnableUI)
            {
                Instance.EtoApp.Run(Instance.MainForm = new MainForm());
            }

        }
    }
}