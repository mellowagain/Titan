using System;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;

namespace Titan.UI.GameInfo
{
    public class ExtraGameInfo : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;

        private TextBox _txtBox;

        public ExtraGameInfo(UIManager uiManager)
        {
            Title = "Titan";
            ClientSize = new Size(600, 400);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            _uiManager = uiManager;

            var btn = new Button { Text = "Continue" };
            btn.Click += OnButtonClick;

            _txtBox = new TextBox { PlaceholderText = "Titan Idle Bot" };
            
            Content = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = "Please input a custom game name:" }, true)
                    ),
                    new TableRow(
                        _txtBox,
                        btn
                    ),
                    new TableRow { ScaleHeight = true }
                }
            };

        }
        
        private void OnButtonClick(object sender, EventArgs args)
        {
            if(!string.IsNullOrWhiteSpace(_txtBox.Text))
            {
                Titan.Instance.UIManager.GetForm<General.General>(UIType.General).CustomGameName = _txtBox.Text.Trim();
                
                _log.Debug("Successfully set custom game name to {Custom}.", _txtBox.Text.Trim());
                
                Close();
                
                _log.Debug("Successfully closed Extra Game Info form.");
            }
        }

    }
}