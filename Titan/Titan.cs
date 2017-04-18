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

            Log.Debug("Checking if Protobufs require updates");

            if(Updater.RequiresUpdate())
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

            Log.Info("Parse arguments");

            /* Parse arguments provided with the starting of this */
            if(!CommandLine.Parser.Default.ParseArguments(args, Instance.Options))
            {
                /* One of the required arguments was most likely not given. Open the UI for the user */
            }
            else
            {
                /* Manual mode: All required arguments were given, go directly to reporting */
            }

            //if(Instance.Options.ForceUpdate)
            //{
              //  Updater.Update();
            //}

        }
    }
}