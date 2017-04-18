using System;
using System.Net;
using System.Threading;
using Eto.Forms;
using log4net;
using log4net.Config;
using Titan.Protobufs;

namespace Titan
{
    public sealed class Titan
    {

        public static readonly ILog Log = LogManager.GetLogger(typeof(Titan));
        public static Titan Instance;

        public Application EtoApp;
        public Options Options;
        public bool EnableUI = true;

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
            Instance.EnableUI = !CommandLine.Parser.Default.ParseArguments(args, Instance.Options);

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

            // TODO: Load SteamKit Protobufs & SteamKit-CSGO Protobufs

            if(Instance.EnableUI)
            {
                Instance.EtoApp.Run();
            }

        }
    }
}