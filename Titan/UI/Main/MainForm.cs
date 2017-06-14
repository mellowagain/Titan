using System;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;
using Titan.Mode;
using Titan.UI.Main.Commands;
using Titan.UI.Main.Commands.Links;
using Titan.Util;

namespace Titan.UI.Main
{

    public sealed class MainForm : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;

        // This TrayIcon only exists on Windows. See GitHub issue #14
        public TrayIndicator TrayIcon;

        private readonly DropDown _dropDown;
        private readonly TextBox _targetBox;
        private readonly TextBox _matchIDBox;
        private readonly Label _matchIDLabel;

        public MainForm(UIManager uiManager)
        {
            Title = "Titan";
            ClientSize = new Size(600, 230);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            // Selected arguments for the UI
            _targetBox = new TextBox { PlaceholderText = "STEAM_0:0:131983088" };
            _matchIDBox = new TextBox { PlaceholderText = "CSGO-727c4-5oCG3-PurVX-sJkdn-LsXfE" };
            _matchIDLabel = new Label { Text = "Share Link (optional)" };

            _dropDown = new DropDown { Items = { "Report", "Commend", "Un-Commend" }, SelectedIndex = 0 };
            _dropDown.SelectedIndexChanged += OnDropDownIndexChange;

            var bombBtn = new Button { Text = "Bomb!" };
            bombBtn.Click += OnBombButtonClick;

            _uiManager = uiManager;

            Content = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = "Mode" }, true),
                        new TableCell(_dropDown, true)
                    ),
                    new TableRow(
                        new Label { Text = "Target" },
                        _targetBox
                    ),
                    new TableRow(
                        _matchIDLabel,
                        _matchIDBox
                    ),
                    new TableRow(new TableCell(), new TableCell()),
                    new TableRow(new TableCell(), new TableCell()),
                    new TableRow(new TableCell(), new TableCell()),
                    new TableRow(
                        new TableCell(),
                        bombBtn
                    ),
                    new TableRow { ScaleHeight = true }
                }
            };

            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem { Text = "&Links", Items = {
                        new SteamIO(),
                        new JsonValidator()
                    }}
                },
                AboutItem = new About(),
                QuitItem = new Quit()
            };
            
            if(Platform.IsWpf || Platform.IsWinForms) {
                TrayIcon = new TrayIndicator
                {
                    Icon = _uiManager.SharedResources.TITAN_ICON
                };
            }
            
            TrayIcon?.Show();
        }

        public void OnBombButtonClick(object sender, EventArgs args)
        {
            var mode = (BotMode) _dropDown.SelectedIndex;

            if(!string.IsNullOrWhiteSpace(_targetBox.Text))
            {
                var matchid = string.IsNullOrWhiteSpace(_matchIDBox.Text) ? 8 : SharecodeUtil.Parse(_matchIDBox.Text);

                var steamID = SteamUtil.Parse(_targetBox.Text);

                if(steamID == null)
                {
                    MessageBox.Show("Titan - Error", "Could not parse Steam ID " +
                        _targetBox.Text + " to Steam ID. Please provide a valid " +
                        "SteamID, SteamID3 or SteamID64.", MessageBoxType.Error);
                    
                    return;
                }

                var targetBanInfo = Titan.Instance.BanManager.GetBanInfoFor(steamID.ConvertToUInt64());
                if(targetBanInfo != null)
                {
                    if(targetBanInfo.VacBanned || targetBanInfo.GameBanCount > 0)
                    {
                        _log.Warning("The target has already been banned. Are you sure you " +
                                     "want to bot this player? Ignore this message if the " +
                                     "target has been banned in other games.");
                    }
                }

                _log.Information("Starting bombing of {Target} in Match {Match}.",
                    _targetBox.Text, matchid);

                Titan.Instance.AccountManager.StartBotting(mode, steamID, matchid);
            }
            else
            {
                MessageBox.Show("Please provide a target.", MessageBoxType.Error);
            }
        }

        public void OnDropDownIndexChange(object sender, EventArgs args)
        {
            switch(_dropDown.SelectedIndex)
            {
                case 0:
                    _matchIDLabel.Visible = true;
                    _matchIDBox.Visible = true;
                    break;
                case 1:
                case 2:
                    _matchIDLabel.Visible = false;
                    _matchIDBox.Visible = false;
                    break;
            }
        }

    }
}