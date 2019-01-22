using System;
using System.Collections.Generic;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;
using Titan.UI.About;
using Titan.UI.APIKey;
using Titan.Util;

namespace Titan.UI
{

    public class UIManager
    {

        private Logger _log = LogCreator.Create();

        private Application _etoApp;

        private Dictionary<UIType, Form> _forms = new Dictionary<UIType, Form>();
        
        public SharedResources SharedResources;
        public TrayIndicator TrayIcon;
        
        public UIManager()
        {
            _etoApp = new Application();
            SharedResources = new SharedResources();
        }

        public void InitializeForms()
        {
            _forms.Add(UIType.General, new General.GeneralUI(this));
            _forms.Add(UIType.APIKeyInput, new SWAKeyForm(this));
            
            _etoApp.MainForm = GetForm<General.GeneralUI>(UIType.General);
            
            InitializeTrayIcon();
        }

        private void InitializeTrayIcon()
        {
            TrayIcon = new TrayIndicator
            {
                Title = "Titan",
                Image = SharedResources.TITAN_ICON,
                Visible = true,
                Menu = new ContextMenu
                {
                    Items =
                    {
                        new Command((sender, args) => ShowForm(UIType.General))
                        {
                            MenuText = "Show"
                        },
                        new Command((sender, args) => Environment.Exit((int) ExitCodes.Ok))
                        {
                            MenuText = "Exit"
                        }
                    }
                }
            };

            GetForm<General.GeneralUI>(UIType.General).Closing += (sender, args) =>
            {
                HideForm(UIType.General);
                
                SendNotification("Titan", "Titan will continue to run in the background " +
                                          "and will notify you as soon as a victim got banned.",
                                          () => ShowForm(UIType.General));

                args.Cancel = true;
            };
        }

        public void Destroy()
        {
            foreach (var pair in _forms)
            {
                if (pair.Value.Visible)
                {
                    pair.Value.Close();
                }
            }

            if (TrayIcon.Visible)
            {
                TrayIcon.Hide();
            }
        }

        public void ShowForm(UIType ui)
        {
            if(_forms.TryGetValue(ui, out var form))
            {
                form.Show();
                form.Focus();
            }
            else
            {
                _log.Error("Could not find form assigned to UI enum {UI}.", ui);
            }
        }

        public void ShowForm(UIType ui, Form form)
        {
            form.Show();
            form.Focus();
        }

        public void ShowForm(UIType ui, Dialog dialog)
        {
            dialog.Focus();
            dialog.ShowModal();
        }

        public DialogResult ShowForm(UIType ui, CommonDialog dialog)
        {
            return dialog.ShowDialog(_etoApp.MainForm);
        }
        
        public void HideForm(UIType ui)
        {
            if(_forms.TryGetValue(ui, out var form))
            {
                form.Visible = false;
            }
            else
            {
                _log.Error("Could not find form assigned to UI enum {UI}.", ui);
            }
        }

        public void RunForm(UIType ui)
        {
            if(_forms.TryGetValue(ui, out var form))
            {
                _etoApp.Run(form);
            }
            else
            {
                _log.Error("Could not find form assigned to UI enum {UI}.", ui);
            }
        }

        public void RunForm(UIType ui, Form form)
        {
            _etoApp.Run(form);
        }

        public void StartMainLoop()
        {
            _etoApp.Run();
        }

        public T GetForm<T>(UIType ui) where T : Form
        {
            if(_forms.TryGetValue(ui, out var form))
            {
                return (T) form;
            }

            _log.Error("Could not find form assigned to UI enum {UI}.", ui);
            return null;
        }

        public void SendNotification(string title, string message, Action a = null)
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                ContentImage = Titan.Instance.UIManager.SharedResources.TITAN_ICON
            };

            if(a != null)
            {
                // TODO: This is deprecated. Needs workaround for per-notification based activation
                notification.Activated += (sender, args) => a();
            }

            Application.Instance.Invoke(() =>
                notification.Show(TrayIcon)
            );
        }

    }
}
