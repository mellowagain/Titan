using System;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;

namespace Titan.UI.APIKey
{
    public class SWAKeyForm : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;
        private static bool _seen; // Workaround for a bug with Gtk that calls the close callback when opened

        public SWAKeyForm(UIManager uiManager)
        {
            Title = "Titan - Steam Web API action required";
            Resizable = false;
            Topmost = true;
            Icon = Titan.Instance.UIManager.SharedResources.TITAN_ICON;

            _uiManager = uiManager;

            TextControl txtBoxKey;
            if (Titan.Instance.Options.Secure)
            {
                txtBoxKey = new PasswordBox { PasswordChar = '\u2022' };
            }
            else
            {
                txtBoxKey = new TextBox { PlaceholderText = "F23C62A2B9263314FGE2FDA2F9CC683Z" };
            }
            
            var btnSite = new Button { Text = "Steam API Key Website" };
            btnSite.Click += (sender, args) => Process.Start("https://steamcommunity.com/dev/apikey");
            var btnContinue = new Button { Text = "Continue" };
            btnContinue.Click += (sender, args) =>
            {
                if (!string.IsNullOrWhiteSpace(txtBoxKey.Text))
                {
                    Titan.Instance.WebHandle.SetKey(txtBoxKey.Text);

                    if (!Titan.Instance.Options.Secure)
                    {
                        _log.Debug("Successfully set Steam Web API Key to {key}.", txtBoxKey.Text);
                    }

                    // SetKey will set the key to null if the key is invalid. Check for that.
                    if (!string.IsNullOrEmpty(Titan.Instance.WebHandle.GetKey()))
                    {
                        Close();
                    }
                    else
                    {
                        _uiManager.SendNotification("Titan - Error", "The provided Web API key was invalid. " + 
                                                                     "Please check your Web API key.");
                        txtBoxKey.Text = "";
                    }
                }
                else
                {
                    _uiManager.SendNotification("Titan - Error", "Please provide a valid Steam Web API key.",
                                                                 () => txtBoxKey.Focus());
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
                        Text = "Steam Web API",
                        Content = new TableLayout
                        {
                            Spacing = new Size(5, 5),
                            Padding = new Padding(10, 10, 10, 10),
                            Rows =
                            {
                                new TableRow(
                                    new TableCell(new Label
                                    {
                                        Text = "Titan requires a Steam Web API key to function correctly. \n" +
                                               "Please input a valid Steam Web API key below."
                                    })
                                ),
                                new TableRow(
                                    new TableCell(txtBoxKey)
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
                                new TableCell(btnSite),
                                new TableCell(btnContinue)
                            ),
                            new TableRow { ScaleHeight = true }
                        }
                    }
                }
            };
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _uiManager.GetForm<General.GeneralUI>(UIType.General).Enabled = false;
            _seen = true;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            if (!string.IsNullOrEmpty(Titan.Instance.WebHandle.GetKey()))
            {
                _uiManager.GetForm<General.GeneralUI>(UIType.General).Enabled = true;
            }
            else if (_seen)
            {
                Environment.Exit(-1);
            }
        }
        
    }
}