using System;
using System.Diagnostics;
using System.IO;
using Eto.Drawing;
using Eto.Forms;

namespace Titan.UI.Main.Commands.Links
{
    public class JsonValidator : Command
    {

        private readonly string _icon = Environment.CurrentDirectory + Path.DirectorySeparatorChar + "Resources" +
                                        Path.DirectorySeparatorChar + "JsonLint.ico";

        public JsonValidator()
        {
            MenuText = "JsonLint";
            Image = new Icon(File.Open(_icon, FileMode.Open));
            Shortcut = Application.Instance.CommonModifier | Keys.J;
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);

            Process.Start("http://jsonlint.com");
        }

    }
}