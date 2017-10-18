using System;
using System.IO;
using Eto.Drawing;

namespace Titan.UI
{

    // ReSharper disable InconsistentNaming
    public class SharedResources
    {

        public Icon TITAN_ICON;

        public Icon STEAM_ICON;

        public SharedResources()
        {
            var resDir = Path.Combine(Environment.CurrentDirectory, "Resources");

            STEAM_ICON = new Icon(File.Open(Path.Combine(resDir, "Steam.ico"), FileMode.Open));

            TITAN_ICON = new Icon(File.Open(Path.Combine(resDir, "Logo.ico"), FileMode.Open));
        }

    }

}