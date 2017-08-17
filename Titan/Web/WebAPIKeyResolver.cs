using System;
using System.IO;
using System.Text;
using Serilog.Core;
using Titan.Logging;
using Titan.UI;

namespace Titan.Web
{
    public class WebAPIKeyResolver
    {

        private Logger _log = LogCreator.Create();

        public static string APIKey { get; set; }

        private FileInfo _file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "steamapi.key"));

        public void ParseKeyFile()
        {
            if(_file.Exists)
            {
                using (var reader = File.OpenText(_file.ToString()))
                {
                    APIKey = reader.ReadLine();
                }

                if(string.IsNullOrEmpty(APIKey))
                {
                    if(!string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY")))
                    {
                        APIKey = Environment.GetEnvironmentVariable("TITAN_WEB_API_KEY");
                    }
                    else
                    {
                        APIKey = null;
                        Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);
                    }
                    
                    SaveKeyFile();
                }
            }
            else
            {
                Titan.Instance.UIManager.ShowForm(UIType.APIKeyInput);
            }
            
            _log.Debug("Using Steam API Key {Key} for the Steam Web API.", APIKey);
        }

        public void SaveKeyFile()
        {
            if(!string.IsNullOrWhiteSpace(APIKey))
            {
                Environment.SetEnvironmentVariable("TITAN_WEB_API_KEY", APIKey, EnvironmentVariableTarget.User);

                File.WriteAllText(_file.ToString(), APIKey, Encoding.UTF8);
            }
        }

    }
}