using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace Titan.UI.Commands
{
    public class Quit : Command
    {

        private readonly string _icon = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Resources" +
                                        Path.DirectorySeparatorChar + "Exit.ico";

        public Quit()
        {
            MenuText = "Quit";
            Image = new Icon(File.Open(_icon, FileMode.Open));
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);

            Environment.Exit(0);
        }

    }
}