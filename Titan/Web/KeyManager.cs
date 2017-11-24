using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Mono.CSharp;
using SteamKit2;
using Titan.UI;
using Titan.UI.APIKey;

namespace Titan.Web
{
    public class KeyManager
    {

        private SWAHandle _handle;
        
        private FileInfo _file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "steamapi.key"));
        
        public string SWAKey;
        public string EnvironmentKey => Environment.GetEnvironmentVariable(
            "TITAN_WEB_API_KEY", EnvironmentVariableTarget.User
        );

        public KeyManager(SWAHandle handle)
        {
            _handle = handle;
        }

        public void Load()
        {
            if (_file.Exists)
            {
                using (var reader = File.OpenText(_file.ToString()))
                {
                    SWAKey = reader.ReadLine();
                }
            } 
            
            if (!string.IsNullOrEmpty(EnvironmentKey) && string.IsNullOrEmpty(SWAKey))
            {
                SWAKey = EnvironmentKey;
            }

            if (!string.IsNullOrEmpty(SWAKey))
            {
                if (!_handle.RequestAPIMethods())
                {
                    _handle.Log.Warning("Received invalid Steam Web API key. Ignoring...");
                }
            }
            
            Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);
            Titan.Instance.UIManager.GetForm<APIKeyForm>(UIType.APIKeyInput).Focus();
        }
        
        public void Save()
        {
            if (!string.IsNullOrEmpty(SWAKey))
            {
                using (var writer = new StreamWriter(_file.ToString(), false, Encoding.UTF8))
                {
                    writer.Write(SWAKey);
                }
            }
            else
            {
                _handle.Log.Warning("No Steam Web API key was found. Asking again on next launch.");
            }
        }

    }
}