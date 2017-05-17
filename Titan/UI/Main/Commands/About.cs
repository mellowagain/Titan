using System;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace Titan.UI.Main.Commands
{
    public class About : Command
    {

        private readonly string _icon = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Resources" +
                                        Path.DirectorySeparatorChar + "Info.ico";

        public About()
        {
            MenuText = "About";
            Image = new Icon(File.Open(_icon, FileMode.Open));
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);

            MessageBox.Show("The Titan Report Bot \n\n" +
                            "Copyright (C) 2017 Marc3842h \n" +
                            "Licensed under the MIT license. \n\n" +
                            "https://github.com/Marc3842h/Titan",
                            "About Titan", MessageBoxButtons.OK, MessageBoxType.Information, MessageBoxDefaultButton.OK);
        }
    }
}