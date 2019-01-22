using System;
using System.Data;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Account;
using Titan.Account.Impl;
using Titan.Import;
using Titan.Import.Impl;
using Titan.Json;
using Titan.Logging;
using Titan.Util;

namespace Titan.UI.General.Tabs
{
    public class AccountsTab : Tab
    {
        
        private Logger _log = LogCreator.Create();

        private GeneralUI _generalUI;
        
        public AccountsTab(UIManager uiManager, GeneralUI generalUI) : base(uiManager, new Size(640, 460))
        {
            _generalUI = generalUI;
        }

        public override TabPage GetTabPage()
        {
            var grid = new GridView { AllowMultipleSelection = false };
            
            RefreshList(ref grid);
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Enabled")
                {
                    TextAlignment = TextAlignment.Center
                },
                HeaderText = "Enabled"
            });
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Index")
                {
                    TextAlignment = TextAlignment.Center
                },
                HeaderText = "Index"
            });
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Username"),
                HeaderText = "Username"
            });
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Password"),
                HeaderText = "Password",
                Visible = !Titan.Instance.Options.Secure
            });
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Sentry")
                {
                    TextAlignment = TextAlignment.Center
                },
                HeaderText = "Steam Guard",
                Visible = !Titan.Instance.Options.Secure
            });

            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Secret"),
                HeaderText = "Shared Secret",
                Visible = !Titan.Instance.Options.Secure
            });
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Cooldown"),
                HeaderText = "Cooldown"
            });
            
            var txtBoxUsername = new TextBox { PlaceholderText = "Username" };

            TextControl txtBoxPassword;
            if (Titan.Instance.Options.Secure)
            {
                txtBoxPassword = new PasswordBox { PasswordChar = '\u2022' };
            }
            else
            {
                txtBoxPassword = new TextBox { PlaceholderText = "Password" };
            }
            
            var cbSentry = new CheckBox { Text = "Steam Guard", Checked = false };
            
            var btnAddUpdate = new Button { Text = "Add / Update" };
            btnAddUpdate.Click += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(txtBoxUsername.Text) && !string.IsNullOrWhiteSpace(txtBoxPassword.Text))
                {
                    if (Titan.Instance.AccountManager.TryGetAccount(txtBoxUsername.Text.Trim(), out var account))
                    {
                        if (account.JsonAccount.Sentry != (cbSentry.Checked != null && (bool) cbSentry.Checked))
                        {
                            var clone = account.JsonAccount.Clone();
                            
                            Titan.Instance.AccountManager.RemoveAccount(account);
                            if (cbSentry.Checked != null && (bool) cbSentry.Checked)
                            {
                                account = new ProtectedAccount(clone);
                            }
                            else
                            {
                                account = new UnprotectedAccount(clone);
                            }
                            Titan.Instance.AccountManager.AddAccount(account);
                        }
                        
                        account.JsonAccount.Username = txtBoxUsername.Text.Trim();
                        account.JsonAccount.Password = txtBoxPassword.Text.Trim();
                        account.JsonAccount.Sentry = cbSentry.Checked != null && (bool) cbSentry.Checked;
                        
                        UIManager.SendNotification("Titan", "Successfully updated account " + account.JsonAccount.Username);
                    }
                    else
                    {
                        var jsonAccount = new JsonAccounts.JsonAccount
                        {
                            Username = txtBoxUsername.Text.Trim(),
                            Password = txtBoxPassword.Text.Trim(),
                            Sentry = cbSentry.Checked != null && (bool) cbSentry.Checked,
                            Enabled = true,
                            SharedSecret = null
                        };
                        
                        TitanAccount acc;
                        
                        if (jsonAccount.Sentry)
                        {
                            acc = new ProtectedAccount(jsonAccount);
                        }
                        else
                        {
                            acc = new UnprotectedAccount(jsonAccount);
                        }
                        
                        Titan.Instance.AccountManager.AddAccount(acc);
                        
                        UIManager.SendNotification("Titan", "Successfully added account " + acc.JsonAccount.Username);

                        var tabControl = (TabControl) _generalUI.Content;
                        foreach (var page in tabControl.Pages)
                        {
                            page.Enabled = true;
                        }

                        if (Titan.Instance.DummyMode)
                        {
                            _log.Information("There are now accounts specified. Turning dummy mode {off}.", "off");
                            
                            Titan.Instance.DummyMode = false;
                        }
                    }
                    
                    Titan.Instance.AccountManager.SaveAccountsFile();
                    RefreshList(ref grid);
                }
                else
                {
                    UIManager.SendNotification("Titan - Error", "Please input a username and password.");
                }
            };
            
            var btnRemove = new Button { Text = "Remove" };
            btnRemove.Click += (sender, args) =>
            {
                dynamic selected = grid.SelectedItem;

                if (selected != null)
                {
                    var username = selected.Username;

                    if (username != null)
                    {
                        if (Titan.Instance.AccountManager.TryGetAccount(username, out TitanAccount account))
                        {
                            Titan.Instance.AccountManager.RemoveAccount(account);
                            Titan.Instance.AccountManager.SaveAccountsFile();
                            RefreshList(ref grid);
                        }
                        else
                        {
                            UIManager.SendNotification("Titan - Error", "The account doesn't exist.");
                        }
                    }
                    else
                    {
                        UIManager.SendNotification("Titan", "The account could not be found.");
                    }
                }
                else
                {
                    UIManager.SendNotification("Titan - Error", "Please select a account before removing it.");
                }

                if (Titan.Instance.AccountManager.Count() < 1)
                {
                    var tabControl = (TabControl) _generalUI.Content;
                    foreach (var page in tabControl.Pages)
                    {
                        if (!page.Text.Equals("Accounts"))
                        {
                            page.Enabled = false;
                        }
                    }

                    if (!Titan.Instance.DummyMode)
                    {
                        _log.Warning("There are no longer accounts specified. Turning dummy mode back {on}.", "on");

                        Titan.Instance.DummyMode = true;
                    }
                }
            };
            
            var btnImport = new Button { Text = "Import" };
            btnImport.Click += (sender, args) =>
            {
                var dialog = new OpenFileDialog
                {
                    Title = "Select file to import",
                    Filters =
                    {
                        new FileFilter("", "txt")
                    },
                    MultiSelect = false,
                    CheckFileExists = true
                };
                
                var result = UIManager.ShowForm(UIType.FilePicker, dialog);

                if (result != DialogResult.Ok)
                {
                    return;
                }
                
                AccountImporter importer = new GenericAccountListImporter(dialog.FileName);

                var accounts = importer.ParseAccounts();

                if (accounts.Count <= 0)
                {
                    UIManager.SendNotification("Titan", "No accounts were imported.");
                    return;
                }

                foreach (var account in accounts)
                {
                    Titan.Instance.AccountManager.AddAccount(account);
                }
                
                UIManager.SendNotification("Titan", "Successfully imported " + accounts.Count + " accounts.");

                // Disable dummy mode now that accounts are here.
                var tabControl = (TabControl) _generalUI.Content;
                foreach (var page in tabControl.Pages)
                {
                    page.Enabled = true;
                }

                if (Titan.Instance.DummyMode)
                {
                    _log.Information("There are now accounts specified. Turning dummy mode {off}.", "off");
                            
                    Titan.Instance.DummyMode = false;
                }
                
                RefreshList(ref grid);
            };
            
            return new TabPage
            {
                Text = "Accounts",
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new TableRow(new TableCell(grid, true)) { ScaleHeight = true },
                        new GroupBox
                        {
                            Text = "Edit",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(txtBoxUsername, true),
                                        new TableCell(txtBoxPassword, true),
                                        new TableCell(cbSentry)
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
                                    new TableCell(btnImport),
                                    new TableCell(btnRemove),
                                    new TableCell(btnAddUpdate)
                                )
                            }
                        }
                    }
                }
            };
        }
        
        private void RefreshList(ref GridView grid)
        {
            var dataTable = new DataTable();
            dataTable.Columns.Add("Enabled", typeof(bool));
            dataTable.Columns.Add("Index", typeof(int));
            dataTable.Columns.Add("Username", typeof(string));
            dataTable.Columns.Add("Password", typeof(string));
            dataTable.Columns.Add("Sentry", typeof(bool));
            dataTable.Columns.Add("Shared Secret", typeof(string));
            dataTable.Columns.Add("Cooldown", typeof(string));
            
            foreach (var index in Titan.Instance.AccountManager.Accounts)
            {
                var cooldownEnd = Titan.Instance.AccountManager.GetExpireTimeForIndex(index.Key);
                var difference = cooldownEnd.Subtract(DateTime.Now);
                var cooldown = "-";
                if (cooldownEnd != DateTime.MinValue)
                {
                    cooldown = difference.Hours == 1 ? "1 hour" : difference.Hours + " hours";
                    cooldown += ", " + difference.Minutes + " minutes";
                }

                if (index.Key != -1)
                {
                    foreach (var account in index.Value)
                    {
                        dataTable.Rows.Add(
                            account.JsonAccount.Enabled,
                            index.Key,
                            account.JsonAccount.Username,
                            account.JsonAccount.Password,
                            account.JsonAccount.Sentry,
                            string.IsNullOrWhiteSpace(account.JsonAccount.SharedSecret) 
                                ? "-" : account.JsonAccount.SharedSecret,
                            cooldown
                        );
                    }
                }
            }
            
            var collection = dataTable.Rows.Cast<DataRow>()
                .Select(x => new
                {
                    Enabled = (bool) x[0] ? "\u2714" : "\u2718", // ✔ : ✘
                    Index = "#" + x[1],
                    Username = x[2],
                    Password = x[3],
                    Sentry = (bool) x[4] ? "\u2714" : "\u2718", // ✔ : ✘
                    Secret = x[5],
                    Cooldown = x[6]
                })
                .ToList();

            grid.DataStore = collection;
        }
        
    }
}
