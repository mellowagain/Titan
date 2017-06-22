using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;

namespace Titan.UI.Accounts
{
    public class AccountUI : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;

        public AccountUI(UIManager uiManager)
        {
            Title = "Accounts";
            ClientSize = new Size(620, 375);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            _uiManager = uiManager;

            Content = new Scrollable
            {
                Border = BorderType.Line,
                Content = GetAccountList()
            };
        }

        public TableLayout GetAccountList()
        {
            var layout = new TableLayout
            {
                Spacing = new Size(5, 5),
                Padding = new Padding(10, 10, 10, 10),
                Rows =
                {
                    new TableRow(
                        new TableCell(
                            new Label { Text = "Index", Font = new Font(SystemFont.Bold) }
                        ),
                        new TableCell(
                            new Label { Text = "Username", Font = new Font(SystemFont.Bold) }
                        ),
                        new TableCell(
                            new Label { Text = "Password", Font = new Font(SystemFont.Bold) }
                        ),
                        new TableCell(
                            new Label { Text = "Steam Guard", Font = new Font(SystemFont.Bold) }
                        ),
                        new TableCell(
                            new Label { Text = "Enabled", Font = new Font(SystemFont.Bold) }
                        ),
                        new TableCell(
                            new Label { Text = "Shared Secret", Font = new Font(SystemFont.Bold) }
                        )
                    )
                }
            };

            foreach(var index in Titan.Instance.AccountManager.Accounts)
            {
                if(index.Key != -1)
                {
                    foreach(var account in index.Value)
                    {
                        layout.Rows.Add(new TableRow(
                            new TableCell(
                                new Label { Text = "#" + index.Key, TextAlignment = TextAlignment.Center }
                            ),
                            new TableCell(
                                new Label { Text = account.JsonAccount.Username }
                            ),
                            new TableCell(
                                new Label { Text = account.JsonAccount.Password }
                            ),
                            new TableCell(
                                new Label { Text = account.JsonAccount.Sentry ? "Yes" : "No" }
                            ),
                            new TableCell(
                                new Label { Text = account.JsonAccount.Enabled ? "Yes" : "No" }
                            ),
                            new TableCell(
                                new Label
                                {
                                    Text = string.IsNullOrWhiteSpace(account.JsonAccount.SharedSecret) 
                                        ? "-" : account.JsonAccount.SharedSecret
                                }
                            )
                        ));
                    }
                }
            }

            return layout;
        }

    }
}