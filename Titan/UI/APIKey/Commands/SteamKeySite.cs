using System;
using System.Diagnostics;
using Eto.Forms;

namespace Titan.UI.APIKey.Commands
{

    public class SteamKeySite : Command
    {

        public SteamKeySite(UIManager uiManager)
        {
            MenuText = "Web API Key Site";
            Image = uiManager.SharedResources.STEAM_ICON;
            Shortcut = Application.Instance.CommonModifier | Keys.A;
        }

        protected override void OnExecuted(EventArgs e)
        {
            base.OnExecuted(e);

            Process.Start("https://steamcommunity.com/dev/apikey");
        }
    }

}