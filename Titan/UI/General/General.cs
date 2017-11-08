using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Account;
using Titan.Account.Impl;
using Titan.Json;
using Titan.Logging;
using Titan.Meta;
using Titan.Restrictions;
using Titan.Util;

namespace Titan.UI.General
{
    public class General : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;

        public string CustomGameName;
        
        private List<DropDown> _indexDropDowns = new List<DropDown>();

        public General(UIManager uiManager)
        {
            Title = "Titan";
            ClientSize = new Size(640, 450);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            _uiManager = uiManager;
            
            var tabControl = new TabControl
            {
                Pages =
                {
                    GetReportTab(),
                    GetCommendTab(),
                    GetIdleTabPage(),
                    GetAccountsTabPage()
                }
            };
            
            tabControl.SelectedIndexChanged += delegate
            {
                Size size;
                
                switch(tabControl.SelectedIndex)
                {
                    case 0: // Report
                        size = new Size(640, 450);
                        break;
                    case 1: // Commend
                        size = new Size(640, 385);
                        break;
                    case 2: // Idle
                        size = new Size(640, 450);
                        break;
                    case 3: // Accounts
                        size = new Size(640, 500);
                        break;
                    default:
                        // If this case happens somebody failed to introduce the case here in the switch block.
                        size = new Size(640, 500);
                        break;
                }
                
                ClientSize = size;

                foreach (var indexDropDown in _indexDropDowns)
                {
                    RefreshIndexesDropDown(indexDropDown);
                }
            };
            
            Content = tabControl;
            
            AddMenuBar();

            if (Titan.Instance.DummyMode)
            {
                tabControl.SelectedIndex = 3;
            }
        }

        private TabPage GetReportTab()
        {
            var txtBoxSteamID = new TextBox { PlaceholderText = "STEAM_0:0:131983088" };
            var txtBoxMatchID = new TextBox { PlaceholderText = "CSGO-727c4-5oCG3-PurVX-sJkdn-LsXfE" };

            var cbAbusiveText = new CheckBox { Text = "Abusive Text Chat", Checked = true };
            var cbAbusiveVoice = new CheckBox { Text = "Abusive Voice Chat", Checked = true };
            var cbGriefing = new CheckBox { Text = "Griefing", Checked = true };
            var cbCheatAim = new CheckBox { Text = "Aim Hacking", Checked = true };
            var cbCheatWall = new CheckBox { Text = "Wall Hacking", Checked = true };
            var cbCheatOther = new CheckBox { Text = "Other Hacking", Checked = true };

            var dropIndexes = new DropDown();
            RefreshIndexesDropDown(dropIndexes);
            _indexDropDowns.Add(dropIndexes);
            
            var cbAllIndexes = new CheckBox { Text = "Use all accounts", Checked = false };
            cbAllIndexes.CheckedChanged += delegate
            {
                if(cbAllIndexes.Checked != null)
                {
                    dropIndexes.Enabled = (bool) !cbAllIndexes.Checked;
                }
                else
                {
                    cbAllIndexes.Checked = false;
                }
            };

            var btnReport = new Button { Text = "Report" };
            btnReport.Click += delegate
            {
                if(!string.IsNullOrWhiteSpace(txtBoxSteamID.Text))
                {
                    var steamID = SteamUtil.Parse(txtBoxSteamID.Text);
                    var matchID = SharecodeUtil.Parse(txtBoxMatchID.Text);

                    if(steamID != null)
                    {
                        if(matchID == 8)
                        {
                            _log.Warning("Could not convert {ID} to a valid Match ID. Trying to resolve the " +
                                         "the Match ID in which the target is playing at the moment.", matchID);
                        
                            Titan.Instance.AccountManager.StartMatchIDResolving(
                                cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? -1 : dropIndexes.SelectedIndex,
                                new LiveGameInfo { SteamID = steamID } );
                        }

                        if (Blacklist.IsBlacklisted(steamID))
                        {
                            Titan.Instance.UIManager.SendNotification(
                                "Restriction applied", 
                                "The target you are trying to report is blacklisted from botting " +
                                "in Titan.", 
                                delegate { Process.Start("https://github.com/Marc3842h/Titan/wiki/Blacklist"); }
                            );
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

                            if(Titan.Instance.VictimTracker.IsVictim(steamID))
                            {
                                _log.Warning("You already report botted this victim. " +
                                             "Are you sure you want to bot this player? " +
                                             "Ignore this message if the first report didn't have enough reports.");
                            }

                            _log.Information("Starting reporting of {Target} in Match {Match}.",
                                steamID.ConvertToUInt64(), matchID);

                            Titan.Instance.AccountManager.StartReporting(
                                cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? -1 : dropIndexes.SelectedIndex,
                                new ReportInfo {
                                    SteamID = steamID,
                                    MatchID = matchID,
                                
                                    AbusiveText = cbAbusiveText.Checked != null && (bool) cbAbusiveText.Checked,
                                    AbusiveVoice = cbAbusiveVoice.Checked != null && (bool) cbAbusiveVoice.Checked,
                                    Griefing = cbGriefing.Checked != null && (bool) cbGriefing.Checked,
                                    AimHacking = cbCheatAim.Checked != null && (bool) cbCheatAim.Checked,
                                    WallHacking = cbCheatWall.Checked != null && (bool) cbCheatWall.Checked,
                                    OtherHacking = cbCheatOther.Checked != null && (bool) cbCheatOther.Checked
                                });
                        }
                    }
                    else
                    {
                        Titan.Instance.UIManager.SendNotification(
                            "Titan - Error", "Could not parse Steam ID " +
                                     txtBoxSteamID.Text + " to Steam ID. Please provide a valid " +
                                     "SteamID, SteamID3 or SteamID64."
                        );
                    }
                }
                else
                {
                    Titan.Instance.UIManager.SendNotification(
                        "Titan - Error", "Please provide a valid target."
                    );
                }
            };
            
            return new TabPage
            {
                Text = "Report",
                Enabled = !Titan.Instance.DummyMode,
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new GroupBox
                        {
                            Text = "Target",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label { Text = "Steam ID" }, true),
                                        new TableCell(txtBoxSteamID, true)
                                    ),
                                    new TableRow(
                                        new TableCell(new Label { Text = "Match ID" }),
                                        new TableCell(txtBoxMatchID)
                                    )
                                }
                            }
                        },
                        new GroupBox
                        {
                            Text = "Options",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(cbAbusiveText, true),
                                        new TableCell(cbAbusiveVoice, true),
                                        new TableCell(cbGriefing, true)
                                    ),
                                    new TableRow(
                                        new TableCell(cbCheatAim),
                                        new TableCell(cbCheatWall),
                                        new TableCell(cbCheatOther)
                                    )
                                }
                            }
                        },
                        new GroupBox
                        {
                            Text = "Bots",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label { Text = "Use Index" }, true),
                                        new TableCell(dropIndexes, true)
                                    ),
                                    new TableRow(
                                        new TableCell(new Panel()),
                                        new TableCell(cbAllIndexes)
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
                                    new TableCell(btnReport)
                                ),
                                new TableRow { ScaleHeight = true }
                            }
                        }
                    }
                }
            };
        }

        private TabPage GetCommendTab()
        {
            var txtBoxSteamID = new TextBox { PlaceholderText = "STEAM_0:0:131983088" };

            var cbLeader = new CheckBox { Text = "Leader", Checked = true };
            var cbFriendly = new CheckBox { Text = "Friendly", Checked = true };
            var cbTeacher = new CheckBox { Text = "Teacher", Checked = true };
            
            var dropIndexes = new DropDown();
            RefreshIndexesDropDown(dropIndexes);
            _indexDropDowns.Add(dropIndexes);
            
            var cbAllIndexes = new CheckBox { Text = "Use all accounts", Checked = false };
            cbAllIndexes.CheckedChanged += delegate
            {
                if(cbAllIndexes.Checked != null)
                {
                    dropIndexes.Enabled = (bool) !cbAllIndexes.Checked;
                }
                else
                {
                    cbAllIndexes.Checked = false;
                }
            };

            var btnCommend = new Button { Text = "Commend" };
            btnCommend.Click += delegate
            {
                if(!string.IsNullOrWhiteSpace(txtBoxSteamID.Text))
                {
                    var steamID = SteamUtil.Parse(txtBoxSteamID.Text);

                    if(steamID != null)
                    {
                        _log.Information("Starting commending of {Target}.",
                            steamID.ConvertToUInt64());
                        
                        Titan.Instance.AccountManager.StartCommending(
                            cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? -1 : dropIndexes.SelectedIndex, 
                            new CommendInfo {
                                SteamID = steamID,
                            
                                Leader = cbLeader.Checked != null && (bool) cbLeader.Checked,
                                Friendly = cbFriendly.Checked != null && (bool) cbFriendly.Checked,
                                Teacher = cbTeacher.Checked != null && (bool) cbTeacher.Checked
                            });
                    }
                    else
                    {
                        Titan.Instance.UIManager.SendNotification(
                            "Titan - Error", "Could not parse Steam ID "
                                             + txtBoxSteamID.Text + " to Steam ID. Please provide a valid " +
                                             "SteamID, SteamID3 or SteamID64."
                        );
                    }
                }
                else
                {
                    Titan.Instance.UIManager.SendNotification(
                        "Titan - Error", "Please provide a valid target."
                    );
                }
            };
            
            return new TabPage
            {
                Text = "Commend",
                Enabled = !Titan.Instance.DummyMode,
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new GroupBox
                        {
                            Text = "Target",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label { Text = "Steam ID" }, true),
                                        new TableCell(txtBoxSteamID, true)
                                    )
                                }
                            }
                        },
                        new GroupBox
                        {
                            Text = "Options",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(cbLeader, true),
                                        new TableCell(cbFriendly, true),
                                        new TableCell(cbTeacher, true)
                                    )
                                }
                            }
                        },
                        new GroupBox
                        {
                            Text = "Bots",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label { Text = "Use Index" }, true),
                                        new TableCell(dropIndexes, true)
                                    ),
                                    new TableRow(
                                        new TableCell(new Panel()),
                                        new TableCell(cbAllIndexes)
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
                                    new TableCell(btnCommend)
                                ),
                                new TableRow
                                {
                                    ScaleHeight = true
                                }
                            }
                        }
                    }
                }
            };
        }

        private TabPage GetIdleTabPage()
        {
            var games = new List<int> { 730 };
            
            var labelGames = new Label { Text = GetFormattedGameIDList(games) };
            
            var nsGameID = new NumericStepper
            {
                MinValue = 0,
                MaxValue = double.MaxValue
            };

            var btnAddGame = new Button { Text = "Add" };
            btnAddGame.Click += delegate
            {
                var available = true;
                var gameID = Convert.ToInt32(Math.Round(nsGameID.Value));
                
                for(var i = 0; i < games.Count; i++)
                {
                    if(games[i] == gameID)
                    {
                        Titan.Instance.UIManager.SendNotification("Titan - Error", "The game id is already in the " +
                                                                                   "list of idle games.");

                        available = false;
                    }
                }

                if(available)
                {
                    if(gameID == 0)
                    {
                        Titan.Instance.UIManager.ShowForm(UIType.ExtraGameInfo);
                        
                        Titan.Instance.UIManager.SendNotification("Titan", "You inputted game id 0. Please specify " +
                                                                           "the custom game for your game in the popup.");
                    }
                    
                    games.Add(gameID);
                    labelGames.Text = GetFormattedGameIDList(games);
                }
            };

            var btnRemoveGame = new Button { Text = "Remove" };
            btnRemoveGame.Click += delegate
            {
                var available = false;
                var gameID = Convert.ToInt32(Math.Round(nsGameID.Value));
                
                for(var i = 0; i < games.Count; i++)
                {
                    if(games[i] == gameID)
                    {
                        games.RemoveAt(i);
                        labelGames.Text = GetFormattedGameIDList(games);
                        
                        Titan.Instance.UIManager.SendNotification("Titan", "Successfully removed game id " 
                                                                           + gameID + " from the list of" +
                                                                           "idle games.");

                        available = true;
                    }
                }

                if(!available)
                {
                    Titan.Instance.UIManager.SendNotification("Titan - Error", "Could not find game id " 
                                                                               + gameID + " in the list of " +
                                                                               "idle games.");
                }
            };
            
            ////////////////////////////////////////////////////////////////////////////
            
            var nsMinutes = new NumericStepper
            {
                MinValue = 0,
                Increment = 10
            };
            
            var dropIndexes = new DropDown();
            RefreshIndexesDropDown(dropIndexes);
            _indexDropDowns.Add(dropIndexes);
            
            var cbAllIndexes = new CheckBox { Text = "Use all accounts", Checked = false };
            cbAllIndexes.CheckedChanged += delegate
            {
                if(cbAllIndexes.Checked != null)
                {
                    dropIndexes.Enabled = (bool) !cbAllIndexes.Checked;
                }
                else
                {
                    cbAllIndexes.Checked = false;
                }
            };

            var btnIdle = new Button { Text = "Idle" };
            btnIdle.Click += delegate
            {
                if(games.Count > 0)
                {
                    var minutes = Convert.ToInt32(Math.Round(nsMinutes.Value));

                    Titan.Instance.AccountManager.StartIdleing(
                        cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? -1 : dropIndexes.SelectedIndex,
                        new IdleInfo
                        {
                            GameID = games.ToArray(),
                            Minutes = minutes
                        }
                    );
                }
                else
                {
                    Titan.Instance.UIManager.SendNotification("Titan - Error", "Please enter atleast one game " +
                                                                               "to idle in.");
                }
            };
            
            return new TabPage
            {
                Text = "Idle",
                Enabled = !Titan.Instance.DummyMode,
                Visible = false,
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new GroupBox
                        {
                            Text = "Games",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label
                                        {
                                            Text = "List of games: ", Font = new Font(SystemFont.Bold) 
                                        }, true),
                                        new TableCell(labelGames, true)
                                    ),
                                    new TableRow(
                                        new TableCell(new Label { Text = "Game ID" }),
                                        new TableCell(nsGameID)
                                    ),
                                    new TableRow(
                                        new TableCell(btnAddGame),
                                        new TableCell(btnRemoveGame)
                                    )
                                }
                            }
                        },
                        new GroupBox
                        {
                            Text = "Time",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label { Text = "Idle for minutes" }, true),
                                        new TableCell(nsMinutes, true)
                                    )
                                }
                            }
                        },
                        new GroupBox
                        {
                            Text = "Bots",
                            Content = new TableLayout
                            {
                                Spacing = new Size(5, 5),
                                Padding = new Padding(10, 10, 10, 10),
                                Rows =
                                {
                                    new TableRow(
                                        new TableCell(new Label { Text = "Use Index" }, true),
                                        new TableCell(dropIndexes, true)
                                    ),
                                    new TableRow(
                                        new TableCell(new Panel()),
                                        new TableCell(cbAllIndexes)
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
                                    new TableCell(btnIdle)
                                ),
                                new TableRow
                                {
                                    ScaleHeight = true
                                }
                            }
                        }
                    }
                }
            };
        }

        private string GetFormattedGameIDList(List<int> gameList)
        {
            var list = "";

            for(var i = 0; i < gameList.Count; i++)
            {
                list += gameList[i];
                if(gameList.Last() != gameList[i])
                    list += ", ";
            }

            return list;
        }

        private TabPage GetAccountsTabPage()
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
                HeaderText = "Password"
            });
            
            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Sentry")
                {
                    TextAlignment = TextAlignment.Center
                },
                HeaderText = "Steam Guard"
            });

            grid.Columns.Add(new GridColumn
            {
                DataCell = new TextBoxCell("Secret"),
                HeaderText = "Shared Secret"
            });
            
            var txtBoxUsername = new TextBox { PlaceholderText = "Username" };
            var txtBoxPassword = new TextBox { PlaceholderText = "Password" };
            var cbSentry = new CheckBox { Text = "Steam Guard", Checked = false };
            
            var btnAddUpdate = new Button { Text = "Add / Update" };
            btnAddUpdate.Click += delegate
            {
                if (!string.IsNullOrWhiteSpace(txtBoxUsername.Text) && !string.IsNullOrWhiteSpace(txtBoxPassword.Text))
                {
                    if (Titan.Instance.AccountManager.TryGetAccount(txtBoxUsername.Text.Trim(), out var account))
                    {
                        account.JsonAccount.Username = txtBoxUsername.Text.Trim();
                        account.JsonAccount.Password = txtBoxPassword.Text.Trim();
                        account.JsonAccount.Sentry = cbSentry.Checked != null && (bool) cbSentry.Checked;
                        
                        _uiManager.SendNotification("Titan", "Successfully updated account " + account.JsonAccount.Username);
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
                        
                        _uiManager.SendNotification("Titan", "Successfully added account " + acc.JsonAccount.Username);

                        var tabControl = (TabControl) Content;
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
                    
                    RefreshList(ref grid);
                }
                else
                {
                    _uiManager.SendNotification("Titan - Error", "Please input a username and password.");
                }
            };
            
            var btnRemove = new Button { Text = "Remove" };
            btnRemove.Click += delegate
            {
                dynamic selected = grid.SelectedItem;

                if (selected != null)
                {
                    _log.Information("{Type}: {@Info}", selected.GetType(), selected);
                    
                    var username = selected.Username;

                    if (username != null)
                    {
                        if (Titan.Instance.AccountManager.TryGetAccount(username, out TitanAccount account))
                        {
                            Titan.Instance.AccountManager.RemoveAccount(account);
                            RefreshList(ref grid);
                        }
                        else
                        {
                            _uiManager.SendNotification("Titan - Error", "The account doesn't exist.");
                        }
                    }
                    else
                    {
                        _uiManager.SendNotification("Titan", "The account could not be found.");
                    }
                }
                else
                {
                    _uiManager.SendNotification("Titan - Error", "Please select a account before removing it.");
                }

                if (Titan.Instance.AccountManager.Count() < 1)
                {
                    var tabControl = (TabControl) Content;
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
            dataTable.Columns.Add("Username");
            dataTable.Columns.Add("Password");
            dataTable.Columns.Add("Sentry", typeof(bool));
            dataTable.Columns.Add("Shared Secret");
            
            foreach (var index in Titan.Instance.AccountManager.Accounts)
            {
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
                                ? "-" : account.JsonAccount.SharedSecret
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
                    Secret = x[5]
                })
                .ToList();

            grid.DataStore = collection;
        }

        private void RefreshIndexesDropDown(DropDown drop)
        {
            drop.Items.Clear();
            
            foreach(var i in Titan.Instance.AccountManager.Accounts)
            {
                if(i.Key != -1)
                {
                    drop.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
                }
            }

            drop.SelectedIndex = Titan.Instance.AccountManager.Index;
        }

        private void AddMenuBar()
        {
            Menu = new MenuBar
            {
                Items =
                {
                    new ButtonMenuItem
                    {
                        Text = "&File",
                        Items =
                        {
                            new Command((s, a) => { Titan.Instance.UIManager.SendNotification("Titan", "Not implemented yet."); })
                            {
                                MenuText = "Settings"
                            }
                        }
                    },
                    /*new ButtonMenuItem
                    {
                        Text = "&Edit",
                        Items =
                        {
                            // TODO: Implement Cut, Copy and Paste
                            new Command((sender, args) => {})
                            {
                                MenuText = "Cut",
                                Shortcut = Application.Instance.CommonModifier | Keys.X
                            },
                            new Command((sender, args) => {})
                            {
                                MenuText = "Copy",
                                Shortcut = Application.Instance.CommonModifier | Keys.C
                            },
                            new Command((sender, args) => {})
                            {
                                MenuText = "Paste",
                                Shortcut = Application.Instance.CommonModifier | Keys.V
                            }
                        }
                    },*/
                    new ButtonMenuItem
                    {
                        Text = "&Tools",
                        Items =
                        {
                            new Command((sender, args) => Process.Start("https://steamid.io"))
                            {
                                MenuText = "SteamIO"
                            },
                            new Command((sender, args) => Process.Start("http://jsonlint.com"))
                            {
                                MenuText = "Json Validator"
                            }
                        }
                    },
                    new ButtonMenuItem
                    {
                        Text = "&Help",
                        Items =
                        {
                            new Command((sender, args) => Process.Start("https://github.com/Marc3842h/Titan"))
                            {
                                MenuText = "GitHub"
                            },
                            new Command((sender, args) => Process.Start("https://discord.me/titanbot"))
                            {
                                MenuText = "Discord"
                            },
                            new SeparatorMenuItem(),
                            new Command((s, a) => { Titan.Instance.UIManager.SendNotification("Titan", "Not implemented yet."); })
                            {
                                MenuText = "System Informations"
                            },
                            new Command((s, a) => { Titan.Instance.UIManager.SendNotification("Titan", "Not implemented yet."); })
                            {
                                MenuText = "Check for Updates"
                            }
                        }
                    }
                },
                
                AboutItem = new Command((sender, args) => _uiManager.ShowForm(UIType.About))
                {
                    MenuText = "About"
                },
                QuitItem = new Command((sender, args) => Environment.Exit(0))
                {
                    MenuText = "Exit"
                }
            };
        }
        
    }
}