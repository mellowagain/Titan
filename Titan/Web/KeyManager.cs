using System;
using System.IO;
using Titan.UI;
using Titan.UI.APIKey;

namespace Titan.Web
{
    public class KeyManager
    {

        private SWAHandle _handle;

        private FileInfo _file;
        
        public string SWAKey;
        public string EnvironmentKey => Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY");

        public KeyManager(SWAHandle handle)
        {
            _handle = handle;

            _file = new FileInfo(Path.Combine(
                Titan.Instance != null ? Titan.Instance.Directory.ToString() : Environment.CurrentDirectory, 
                "steamapi.key"
            ));
        }

        public void Load()
        {
            if (_file.Exists)
            {
                using (var reader = File.OpenText(_file.ToString()))
                {
                    SWAKey = reader.ReadLine();
                }

                if (!Titan.Instance.Options.Secure)
                {
                    _handle.Log.Debug("Received key from {file} file: {key}", "steamapi.key", SWAKey);
                }
            } 
            
            if (!string.IsNullOrEmpty(EnvironmentKey) && string.IsNullOrEmpty(SWAKey))
            {
                SWAKey = EnvironmentKey;

                if (!Titan.Instance.Options.Secure)
                {
                    _handle.Log.Debug("Received key from environment variable: {key}", SWAKey);
                }
            }

            if (!string.IsNullOrEmpty(SWAKey))
            {
                if (!_handle.TestAPIKey())
                {
                    _handle.Log.Warning("Received invalid Steam Web API key. Ignoring...");
                }
                else
                {
                    _handle.Log.Information("Steam Web API is valid and will be used.");
                    return;
                }
            }
            
            Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);
            Titan.Instance.UIManager.GetForm<SWAKeyForm>(UIType.APIKeyInput).Focus();
        }
        
        public void Save()
        {
            if (!string.IsNullOrEmpty(SWAKey))
            {
                File.WriteAllText(_file.ToString(), SWAKey);
                
                Environment.SetEnvironmentVariable("TITAN_WEB_API_KEY", SWAKey);
            }
            else
            {
                _handle.Log.Warning("No Steam Web API key was found. Asking again on next launch.");
            }
        }

    }
}