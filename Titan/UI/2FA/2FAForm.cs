using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Account.Impl;
using Titan.Logging;

namespace Titan.UI._2FA
{
    public class _2FAForm : Form
    {

        private Logger _log;

        public _2FAForm(ProtectedAccount account, string eMail = null)
        {
            Title = "Titan - 2FA Action required - " + account.JsonAccount.Username;
            Resizable = false;
            Topmost = true;
            Icon = Titan.Instance.UIManager.SharedResources.TITAN_ICON;

            _log = LogCreator.Create("2FA Form - " + account.JsonAccount.Username + " (Protected)");
            
            var txtBoxCode = new TextBox { PlaceholderText = "GHC9Y" };
            
            var btnSubmit = new Button { Text = "Submit" };
            btnSubmit.Click += delegate
            {
                if (!string.IsNullOrWhiteSpace(txtBoxCode.Text))
                {
                    if (eMail != null)
                    {
                        account.FeedWithAuthToken(txtBoxCode.Text);
                        if (!Titan.Instance.Options.Secure)
                        {
                            _log.Debug("Feeding {account} with auth token: {token}", account.JsonAccount.Username,
                                txtBoxCode.Text);
                        }
                    }
                    else
                    {
                        account.FeedWith2FACode(txtBoxCode.Text);
                        if (!Titan.Instance.Options.Secure)
                        {
                            _log.Debug("Feeding {account} with 2FA code: {code}", account.JsonAccount.Username,
                                txtBoxCode.Text);
                        }
                    }

                    Close();
                }
                else
                {
                    Titan.Instance.UIManager.SendNotification(
                        "Titan - Error", 
                        "Please provide a valid auth token or 2FA code.",
                        () => txtBoxCode.Focus()
                    );
                }
            };
            
            Content = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new GroupBox
                    {
                        Text = "Steam Guard",
                        Content = new TableLayout
                        {
                            Spacing = new Size(5, 5),
                            Padding = new Padding(10, 10, 10, 10),
                            Rows =
                            {
                                new TableRow(
                                    new TableCell(new Label
                                    {
                                        Text = "Steam Guard has been activated for " + account.JsonAccount.Username +
                                               ". \n" + (eMail != null ? 
                                                       "Please input the code sent to your eMail at " + eMail + "." : 
                                                       "Please input the code from the Steam Guard mobile app.")
                                    })
                                ),
                                new TableRow(
                                    new TableCell(txtBoxCode)
                                )
                            }
                        }
                    },
                    new TableLayout
                    {
                        Spacing = new Size(5, 5),
                        Padding = new Padding(10, 10, 10, 10),
                        Rows =
                        {
                            new TableRow(
                                new TableCell(new Panel(), true),
                                new TableCell(new Panel(), true),
                                new TableCell(btnSubmit)
                            ),
                            new TableRow { ScaleHeight = true }
                        }
                    }
                }
            };
             
        }
        
    }
}