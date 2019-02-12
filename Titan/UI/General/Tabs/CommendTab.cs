using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Account;
using Titan.Logging;
using Titan.Meta;
using Titan.Util;

namespace Titan.UI.General.Tabs
{
    public class CommendTab : Tab
    {
        
        private Logger _log = LogCreator.Create();
        
        public CommendTab(UIManager uiManager) : base(uiManager, new Size(640, 385))
        {
            // Unused constructor
        }

        public override TabPage GetTabPage()
        {
            var txtBoxSteamID = new TextBox { PlaceholderText = "STEAM_0:0:131983088" };

            var cbLeader = new CheckBox { Text = "Leader", Checked = true };
            var cbFriendly = new CheckBox { Text = "Friendly", Checked = true };
            var cbTeacher = new CheckBox { Text = "Teacher", Checked = true };
            
            var dropIndexes = new DropDown();
            RefreshIndexesDropDown(dropIndexes);
            DropDownIndex.Add(dropIndexes);
            
            var labelWarning = new Label
            {
                Text = "All your indexes sum up to over 100 accounts.\n" +
                       "Titan will delay the botting process to\n" +
                       "prevent Steam rate limit issues.",
                Visible = false
            };
            
            var cbAllIndexes = new CheckBox { Text = "Use all accounts", Checked = false };
            cbAllIndexes.CheckedChanged += (sender, args) =>
            {
                if (cbAllIndexes.Checked != null)
                {
                    dropIndexes.Enabled = (bool) !cbAllIndexes.Checked;

                    if (Titan.Instance.AccountManager.Count() > 100)
                    {
                        labelWarning.Visible = (bool) cbAllIndexes.Checked;
                    }
                }
                else
                {
                    cbAllIndexes.Checked = false;
                }
            };

            var btnCommend = new Button { Text = "Commend" };
            btnCommend.Click += (sender, args) =>
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
                                AppID = TitanAccount.CSGO_APPID,
                            
                                Leader = cbLeader.Checked != null && (bool) cbLeader.Checked,
                                Friendly = cbFriendly.Checked != null && (bool) cbFriendly.Checked,
                                Teacher = cbTeacher.Checked != null && (bool) cbTeacher.Checked
                            });
                    }
                    else
                    {
                        UIManager.SendNotification(
                            "Titan - Error", "Could not parse Steam ID "
                                             + txtBoxSteamID.Text + " to Steam ID. Please provide a valid " +
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
                                        new TableCell(labelWarning),
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
        
    }
}
