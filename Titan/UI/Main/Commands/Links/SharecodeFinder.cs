using System;
using System.Diagnostics;
using Eto.Forms;

namespace Titan.UI.Main.Commands.Links
{
    public class SharecodeFinder : Command
    {

        public SharecodeFinder()
        {
            MenuText = "MrCraig Match ID Parser";
            Shortcut = Application.Instance.CommonModifier | Keys.M;
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);

            Process.Start("http://matchid.mrcraig.xyz");
        }

    }
}