using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
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

        private UIManager _uiManager;

        public string CustomGameName;

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
                    GetIdleTabPage()
                }
            };
            
            tabControl.SelectedIndexChanged += delegate
            {
                Size size;
                
                switch(tabControl.SelectedIndex)
                {
                    case 0:
                        size = new Size(640, 450);
                        break;
                    case 1:
                        size = new Size(640, 385);
                        break;
                    case 2:
                        size = new Size(640, 450);
                        break;
                    default:
                        size = new Size(640, 500);
                        // If this case happens somebody failed to introduce the case here in the switch block.
                        break;
                }
                
                ClientSize = size;
            };
            
            Content = tabControl;
            
            AddMenuBar();
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
                if(i.Key != -1)
                {
                    dropIndexes.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
                }
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
                        if(matchID == 8)
                        {
                            _log.Warning("Could not convert {ID} to a valid Match ID. Trying to resolve the " +
                                         "the Match ID in which the target is playing at the moment.", matchID);
                        
                            Titan.Instance.AccountManager.StartMatchIDResolving(
                                cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? -1 : dropIndexes.SelectedIndex,
                                new LiveGameInfo { SteamID = steamID } );
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
                if(i.Key != -1)
                {
                    dropIndexes.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
                }
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

        public TabPage GetIdleTabPage()
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
            foreach(var i in Titan.Instance.AccountManager.Accounts)
            {
                if(i.Key != -1)
                {
                    dropIndexes.Items.Add("#" + i.Key + " (" + i.Value.Count + " accounts)");
                }
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
                            new Command((sender, args) => _uiManager.ShowForm(UIType.Accounts))
                            {
                                MenuText = "Account List"
                            },
                            new SeparatorMenuItem(),
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