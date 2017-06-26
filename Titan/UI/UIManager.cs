using System;
using System.Collections.Generic;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;
using Titan.UI.About;
using Titan.UI.Accounts;
using Titan.UI.APIKey;

namespace Titan.UI
{

    public class UIManager
    {

        private Logger _log = LogCreator.Create();

        private Application _etoApp;

        private Dictionary<UIType, Form> _forms = new Dictionary<UIType, Form>();
        public SharedResources SharedResources;

        public UIManager()
        {
            _etoApp = new Application();
            SharedResources = new SharedResources();

            _forms.Add(UIType.General, new General.General(this));
            _forms.Add(UIType.APIKeyInput, new APIKeyForm(this));
            _forms.Add(UIType.About, new AboutUI(this));
            _forms.Add(UIType.Accounts, new AccountUI(this));

            _etoApp.MainForm = GetForm<General.General>(UIType.General);
        }

        public void ShowForm(UIType ui)
        {
            Form form;

            if(_forms.TryGetValue(ui, out form))
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

        public void RunForm(UIType ui)
        {
            Form form;

            if(_forms.TryGetValue(ui, out form))
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
            Form form;

            if(_forms.TryGetValue(ui, out form))
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
                Icon = Titan.Instance.UIManager.SharedResources.TITAN_ICON
            };

            if(a != null)
            {
                notification.Activated += delegate
                {
                    a();
                };
            }

            Application.Instance.Invoke(() =>
                notification.Show(Titan.Instance.UIManager.GetForm<General.General>(UIType.General).TrayIcon)
            );
        }

    }
}