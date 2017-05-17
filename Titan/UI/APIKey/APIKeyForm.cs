using System;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;
using Titan.UI.APIKey.Commands;

namespace Titan.UI.APIKey
{
    public class APIKeyForm : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;

        private TextBox _txtBox;

        public APIKeyForm(UIManager uiManager)
        {
            Title = "Titan";
            ClientSize = new Size(400, 100);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            _uiManager = uiManager;

            // Widgets for the UI
            _txtBox = new TextBox { PlaceholderText = "Steam Web API key" };

            var btn = new Button { Text = "Continue" };
            btn.Click += OnButtonClick;

            Content = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = "Please generate a Steam Web \n" +
                                                          "API key and input it below:" }, true)
                    ),
                    new TableRow(
                        _txtBox,
                        btn
                    ),
                    new TableRow { ScaleHeight = true }
                }
            };

            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem { Text = "Steam", Items =
                    {
                        new SteamKeySite(uiManager)
                    }}
                }
            };
        }

        private void OnButtonClick(object sender, EventArgs args)
        {
            if(!string.IsNullOrWhiteSpace(_txtBox.Text))
            {
                Titan.Instance.BanManager.APIKey = _txtBox.Text;

                _log.Debug("Successfully set Web API key to {WebAPIKey}.", _txtBox.Text);

                Close();
                Platform.Invoke(() =>
                {

                });

                _log.Information("Successfully closed API key form.");
            }
        }

    }
}