using System;
using System.Diagnostics;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace Titan.UI.Main.Commands.Links
{
    public class SteamIO : Command
    {

        private readonly string _icon = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Resources" +
                               Path.DirectorySeparatorChar + "SteamIO.ico";

        public SteamIO()
        {
            MenuText = "SteamIO";
            Image = new Icon(File.Open(_icon, FileMode.Open));
            Shortcut = Application.Instance.CommonModifier | Keys.I;
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);

            Process.Start("https://steamid.io");
        }

    }
}