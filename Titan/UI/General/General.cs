using System;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;
using Titan.Meta;
using Titan.Util;

namespace Titan.UI.General
{
    public class General : Form
    {

        private Logger _log = LogCreator.Create();

        public TrayIndicator TrayIcon;

        public General()
        {
            Title = "Titan";
            ClientSize = new Size(640, 450);
            Resizable = false;
            Icon = Titan.Instance.UIManager.SharedResources.TITAN_ICON;
            
            var tabControl = new TabControl
            {
                Pages =
                {
                    GetReportTab(),
                    GetCommendTab()
                }
            };
            
            tabControl.SelectedIndexChanged += delegate
            {
                ClientSize = tabControl.SelectedIndex == 0 ? new Size(640, 450) : new Size(640, 385);
            };
            
            Content = tabControl;
            
            AddMenuBar();

            if(Platform.IsWpf || Platform.IsWinForms)
            {
                TrayIcon = new TrayIndicator
                {
                    Icon = Titan.Instance.UIManager.SharedResources.TITAN_ICON
                };
                
                TrayIcon.Show();
            }
        }

        public TabPage GetReportTab()
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
            foreach(var i in Titan.Instance.AccountManager.Accounts)
            {
                dropIndexes.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
            }
            dropIndexes.SelectedIndex = Titan.Instance.AccountManager.Index;
            
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
                                cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? - 1 : dropIndexes.SelectedIndex,
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
                        MessageBox.Show("Could not parse Steam ID " +
                                        txtBoxSteamID.Text + " to Steam ID. Please provide a valid " +
                                        "SteamID, SteamID3 or SteamID64.", "Titan - Error", MessageBoxType.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please provide a target.", "Titan - Error", MessageBoxType.Error);
                }
            };
            
            return new TabPage
            {
                Text = "Report",
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

        public TabPage GetCommendTab()
        {
            var txtBoxSteamID = new TextBox { PlaceholderText = "STEAM_0:0:131983088" };

            var cbLeader = new CheckBox { Text = "Leader", Checked = true };
            var cbFriendly = new CheckBox { Text = "Friendly", Checked = true };
            var cbTeacher = new CheckBox { Text = "Teacher", Checked = true };
            
            var dropIndexes = new DropDown();
            foreach(var i in Titan.Instance.AccountManager.Accounts)
            {
                dropIndexes.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
            }
            dropIndexes.SelectedIndex = Titan.Instance.AccountManager.Index;
            
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
                            cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? - 1 : dropIndexes.SelectedIndex, 
                            new CommendInfo {
                                SteamID = steamID,
                            
                                Leader = cbLeader.Checked != null && (bool) cbLeader.Checked,
                                Friendly = cbFriendly.Checked != null && (bool) cbFriendly.Checked,
                                Teacher = cbTeacher.Checked != null && (bool) cbTeacher.Checked
                            });
                    }
                    else
                    {
                        MessageBox.Show("Could not parse Steam ID " +
                                        txtBoxSteamID.Text + " to Steam ID. Please provide a valid " +
                                        "SteamID, SteamID3 or SteamID64.", "Titan - Error", MessageBoxType.Error);
                    }
                }
                else
                {
                    MessageBox.Show("Please provide a target.", "Titan - Error", MessageBoxType.Error);
                }
            };
            
            return new TabPage
            {
                Text = "Commend",
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

        public void AddMenuBar()
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
                            new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.Settings))
                            {
                                MenuText = "Settings"
                            }
                        }
                    },
                    new ButtonMenuItem
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
                    },
                    new ButtonMenuItem
                    {
                        Text = "&Tools",
                        Items =
                        {
                            new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.History))
                            {
                                MenuText = "History"
                            },
                            new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.Accounts))
                            {
                                MenuText = "Account List"
                            }
                            // ==============================================000
                            ,
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
                            new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.Help))
                            {
                                MenuText = "Help"
                            },
                            new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.SystemInfo))
                            {
                                MenuText = "System Informations"
                            },
                            new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.UpdateChecker))
                            {
                                MenuText = "Check for Updates"
                            }
                        }
                    }
                },
                
                AboutItem = new Command((sender, args) => Titan.Instance.UIManager.ShowForm(UIType.About))
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