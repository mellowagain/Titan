using System;
using System.IO;
using Eto.Drawing;

// ReSharper disable InconsistentNaming
namespace Titan.UI
{

    public class SharedResources
    {

        public Icon TITAN_ICON;

        // 2FA

        // API KEY
        public Icon STEAM_ICON;

        public SharedResources()
        {
            var resDir = Path.Combine(Environment.CurrentDirectory, "Resources");

            STEAM_ICON = new Icon(File.Open(Path.Combine(resDir, "Steam.ico"), FileMode.Open));

            TITAN_ICON = new Icon(File.Open(Path.Combine(resDir, "Logo.ico"), FileMode.Open));
        }

    }

}