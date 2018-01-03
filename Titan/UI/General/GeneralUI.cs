using System;
using System.Collections.Generic;
using System.Diagnostics;
using Eto.Drawing;
using Eto.Forms;
using Titan.UI.About;
using Titan.UI.General.Tabs;

namespace Titan.UI.General
{
    public class GeneralUI : Form
    {

        private UIManager _uiManager;

        private List<Tab> _tabs = new List<Tab>();

        public GeneralUI(UIManager uiManager)
        {
            Title = "Titan";
            ClientSize = new Size(640, 450);
            Resizable = false;
            Icon = uiManager.SharedResources.TITAN_ICON;

            _uiManager = uiManager;
            
            _tabs.Add(new ReportTab(uiManager));
            _tabs.Add(new CommendTab(uiManager));
            _tabs.Add(new AccountsTab(uiManager, this));
            //_tabs.Add(new ValveStatusTab(uiManager));
            
            var tabControl = new TabControl();
            foreach (var tab in _tabs)
            {
                tabControl.Pages.Add(tab.GetTabPage());
            }
            
            tabControl.SelectedIndexChanged += delegate
            {
                //ClientSize = _tabs[tabControl.SelectedIndex].TabSize;

                foreach (var tab in _tabs)
                {
                    foreach (var dropDowns in tab.DropDownIndex)
                    {
                        Tab.RefreshIndexesDropDown(dropDowns);
                    }
                }
            };
            
            Content = tabControl;
            
            AddMenuBar();

            if (Titan.Instance.DummyMode)
            {
                tabControl.SelectedIndex = 3;
            }
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
                            new Command((s, a) => { _uiManager.SendNotification("Titan", "Not implemented yet."); })
                            {
                                MenuText = "Settings"
                            }
                        }
                    },
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
                            new Command((s, a) => { _uiManager.SendNotification("Titan", "Not implemented yet."); })
                            {
                                MenuText = "System Informations"
                            },
                            new Command((s, a) => { _uiManager.SendNotification("Titan", "Not implemented yet."); })
                            {
                                MenuText = "Check for Updates"
                            }
                        }
                    }
                },
                
                AboutItem = new Command((sender, args) => new AboutUI().ShowDialog(this))
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