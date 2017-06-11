using System;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Account.Impl;
using Titan.Logging;

namespace Titan.UI._2FA
{
    public class TwoFactorAuthForm : Form
    {

        private Logger _log = LogCreator.Create();

        private TextBox _txtBox;

        private ProtectedAccount _account;
        private string _email;

        public TwoFactorAuthForm(UIManager uiManager, ProtectedAccount account, string email)
        {
            Title = "Titan - " + account.JsonAccount.Username;
            ClientSize = new Size(400, 100);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            _account = account;
            _email = email;

            // Widgets for the UI
            _txtBox = new TextBox { PlaceholderText = "Auth Code" };

            var btn = new Button { Text = "Continue" };
            btn.Click += OnButtonClick;

            var msg = email != null ? "Please input the Auth Code sent \nto your email at " + email + "."
                : "Please input the 2FA Code from \nthe Steam App.";

            Content = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new TableRow(
                        new TableCell(new Label { Text = msg }, true)
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
                if(_email != null)
                {
                    _account.FeedWithAuthToken(_txtBox.Text);
                }
                else
                {
                    _account.FeedWith2FACode(_txtBox.Text);
                }

                _log.Debug("Successfully found 2FA Code: {2FA}.", _txtBox.Text);

                Close();

                _log.Information("Successfully closed 2FA code form.");
            }
        }

    }
}