using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Account;
using Titan.Logging;
using Titan.Meta;
using Titan.Restrictions;
using Titan.Util;

namespace Titan.UI.General.Tabs
{
    public class ReportTab : Tab
    {

        private Logger _log = LogCreator.Create();
        
        public ReportTab(UIManager uiManager) : base(uiManager, new Size(640, 450))
        {
            // Unused constructor
        }

        public override TabPage GetTabPage()
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
            DropDownIndex.Add(dropIndexes);
            
            var cbAllIndexes = new CheckBox { Text = "Use all accounts", Checked = false };
            cbAllIndexes.CheckedChanged += (sender, args) =>
            {
                if (cbAllIndexes.Checked != null)
                {
                    dropIndexes.Enabled = (bool) !cbAllIndexes.Checked;
                }
                else
                {
                    cbAllIndexes.Checked = false;
                }
            };

            var btnReport = new Button { Text = "Report" };
            btnReport.Click += (sender, args) =>
            {
                if(!string.IsNullOrWhiteSpace(txtBoxSteamID.Text))
                {
                    var steamID = SteamUtil.Parse(txtBoxSteamID.Text);
                    var matchID = SharecodeUtil.Parse(txtBoxMatchID.Text);

                    if(steamID != null)
                    {
                        if (Blacklist.IsBlacklisted(steamID))
                        {
                            UIManager.SendNotification(
                                "Restriction applied", 
                                "The target you are trying to report is blacklisted from botting " +
                                "in Titan.", 
                                () => Process.Start("https://github.com/Marc3842h/Titan/wiki/Blacklist")
                            );
                            return;
                        }
                        
                        if(matchID == 8)
                        {
                            _log.Warning("Could not convert {ID} to a valid Match ID. Trying to resolve the " +
                                         "the Match ID in which the target is playing at the moment.", matchID);
                        
                            Titan.Instance.AccountManager.StartMatchIDResolving(
                                cbAllIndexes.Checked != null && (bool) cbAllIndexes.Checked ? -1 : dropIndexes.SelectedIndex,
                                new LiveGameInfo
                                {
                                    SteamID = steamID,
                                    AppID = TitanAccount.CSGO_APPID
                                });
                        }

                        if (Titan.Instance.WebHandle.RequestBanInfo(steamID.ConvertToUInt64(), out var banInfo))
                        {
                            if(banInfo.VacBanned || banInfo.GameBanCount > 0)
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
                                    AppID = TitanAccount.CSGO_APPID,
                                
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
                        UIManager.SendNotification(
                            "Titan - Error", "Could not parse Steam ID " +
                                     txtBoxSteamID.Text + " to Steam ID. Please provide a valid " +
                                     "SteamID, SteamID3 or SteamID64."
                        );
                    }
                }
                else
                {
                    UIManager.SendNotification(
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
        
    }
}