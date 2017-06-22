using System;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;

namespace Titan.UI.About
{
    public class AboutUI : Form
    {

        private Logger _log = LogCreator.Create();

        private UIManager _uiManager;

        public AboutUI(UIManager uiManager)
        {
            Title = "Titan";
            ClientSize = new Size(540, 375);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;
            
            _uiManager = uiManager;
            
            var tabControl = new TabControl
            {
                Pages =
                {
                    GetAboutTab(),
                    GetCreditsTab(),
                    GetLicenseTab()
                }
            };
            
            tabControl.SelectedIndexChanged += delegate
            {
                switch(tabControl.SelectedIndex)
                {
                    case 0:
                        ClientSize = new Size(540, 375);
                        break;
                    case 1:
                        ClientSize = new Size(540, 185);
                        break;
                    case 2:
                        ClientSize = new Size(540, 575);
                        break;
                }
            };

            Content = tabControl;
        }

        public TabPage GetAboutTab()
        {
            return new TabPage
            {
                Text = "About",
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new TableLayout
                        {
                            Spacing = new Size(5, 5),
                            Padding = new Padding(10, 10, 10, 10),
                            Rows =
                            {
                                new TableRow(
                                    new TableCell(TableLayout.AutoSized(new ImageView
                                    {
                                        Image = _uiManager.SharedResources.TITAN_ICON,
                                        Size = new Size(128, 128)
                                    }, centered: true))
                                ),
                                new TableRow(
                                    new TableCell(TableLayout.AutoSized(new Label
                                    {
                                        Text = "Titan",
                                        Font = new Font(SystemFont.Bold)
                                    }, centered: true))
                                ),
                                new TableRow(
                                    new TableCell(
                                        TableLayout.AutoSized(new Label
                                        {
                                            Text = "Titan is an advanced Counter-Strike Global Offensive report and" +
                                                   "commendation bot. Its goal is to maintain a clean Matchmaking system " +
                                                   "by sending a target forcefully (by 11 reports) into Overwatch. It provides " +
                                                   "a advanced set of features and high effiency when compared against other bots.",
                                            TextAlignment = TextAlignment.Center,
                                            Wrap = WrapMode.Word
                                        }, centered: true)
                                    )
                                ),
                                new TableRow(
                                    new TableCell(
                                        TableLayout.AutoSized(new LinkButton
                                        {
                                            Text = "GitHub",
                                            Command = new Command((s, a) => Process.Start("https://github.com/Marc3842h/Titan"))
                                        }, centered: true)
                                    )
                                )
                            }
                        }
                    }
                }
            };
        }

        public TabPage GetCreditsTab()
        {
            return new TabPage
            {
                Text = "Credits",
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new TableRow(
                            new TableCell(
                                TableLayout.AutoSized(new Label
                                {
                                    Text = "Marc3842h",
                                    TextAlignment = TextAlignment.Center,
                                    Font = new Font(SystemFont.Bold)
                                }, centered: true)
                            )
                        ),
                        new TableRow(
                            new TableCell(
                                TableLayout.AutoSized(new Label
                                {
                                    Text = "Lead developer and project maintainer",
                                    TextAlignment = TextAlignment.Center
                                }, centered: true)
                            )
                        ),
                        new TableRow(
                            new TableCell(
                                TableLayout.AutoSized(new Label
                                {
                                    Text = "info@marcsteiner.me",
                                    TextAlignment = TextAlignment.Center
                                }, centered: true)
                            )
                        ),
                        new TableRow(new Label()),
                        new TableRow(
                            TableLayout.AutoSized(new Label
                            {
                                Text = "Thanks to all contributors.",
                                TextAlignment = TextAlignment.Center
                            }, centered: true)
                        ),
                        new TableRow { ScaleHeight = true }
                    }
                }
            };
        }

        public TabPage GetLicenseTab()
        {
            return new TabPage
            {
                Text = "License",
                Content = new TableLayout
                {
                    Spacing = new Size(5, 5),
                    Padding = new Padding(10, 10, 10, 10),
                    Rows =
                    {
                        new TableRow(
                            TableLayout.AutoSized(new Label
                            {
                                Text = "MIT License",
                                Font = new Font(SystemFont.Bold),
                                TextAlignment = TextAlignment.Center,
                                Wrap = WrapMode.Word
                            }, centered: true)
                        ),
                        new TableRow(
                            TableLayout.AutoSized(new Label
                            {
                                Text = "Copyright (c) 2017 Marc3842h",
                                TextAlignment = TextAlignment.Center,
                                Wrap = WrapMode.Word
                            }, centered: true)
                        ),
                        new TableRow(
                            TableLayout.AutoSized(new Label
                            {
                                Text = "Permission is hereby granted, free of charge, to any person obtaining a copy of " +
                                       "this software and associated documentation files (the \"Software\"), to deal " +
                                       "in the Software without restriction, including without limitation the rights " +
                                       "to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies " +
                                       "of the Software, and to permit persons to whom the Software is furnished to do so, " +
                                       "subject to the following conditions:",
                                TextAlignment = TextAlignment.Center,
                                Wrap = WrapMode.Word
                            }, centered: true)
                        ),
                        new TableRow(
                            TableLayout.AutoSized(new Label
                            {
                                Text = "The above copyright notice and this permission notice shall be included in all " +
                                       "copies or substantial portions of the Software.",
                                TextAlignment = TextAlignment.Center,
                                Wrap = WrapMode.Word
                            }, centered: true)
                        ),
                        new TableRow(
                            TableLayout.AutoSized(new Label
                            {
                                Text = "THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, " +
                                       "INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR " +
                                       "PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE " +
                                       "FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR " +
                                       "OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER " +
                                       "DEALINGS IN THE SOFTWARE.",
                                TextAlignment = TextAlignment.Center,
                                Wrap = WrapMode.Word
                            }, centered: true)
                        )
                    }
                }
            };
        }

    }
}