using System;
using System.Collections.Generic;
using Eto.Forms;
using Serilog.Core;
using Titan.Logging;
using Titan.UI.APIKey;
using Titan.UI.Main;

namespace Titan.UI
{

    public class UIManager
    {

        private Logger _log = LogCreator.Create();

        private Application _etoApp;

        private Dictionary<UIType, Form> _forms = new Dictionary<UIType, Form>();
        public SharedResources SharedResources;

        public bool locked = false;

        // Invoking
        public Action<Form> ShowFormInvokeDelegate = delegate(Form form)
        {

            form.Show();
            form.Focus();

            Titan.Instance.UIManager._etoApp.Run(form);
        };

        public UIManager()
        {
            _etoApp = new Application();
            SharedResources = new SharedResources();

            _forms.Add(UIType.Main, new MainForm(this));
            _forms.Add(UIType.APIKeyInput, new APIKeyForm(this));

            _etoApp.MainForm = GetForm(UIType.Main);
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

        public void StartMainLoop()
        {
            _etoApp.Run();
        }

        public Form GetForm(UIType ui)
        {
            Form form;

            if(_forms.TryGetValue(ui, out form))
            {
                return form;
            }

            _log.Error("Could not find form assigned to UI enum {UI}.", ui);
            return null;
        }

    }
}