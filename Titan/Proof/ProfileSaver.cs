using System;
using System.IO;
using System.Net;
using System.Text;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;
using Titan.Util;

namespace Titan.Proof
{
    public class ProfileSaver
    {
        
        private Logger _log = LogCreator.Create();
        private DirectoryInfo _directory;

        public ProfileSaver(DirectoryInfo dir = null)
        {
            if (dir != null)
            {
                _directory = dir;
            }
            else
            {
                _directory = new DirectoryInfo(Path.Combine(Titan.Instance.Directory.ToString(), "victimprofiles"));
                
                if (!_directory.Exists)
                {
                    _directory.Create();
                }
            }
        }

        public void SaveSteamProfile(SteamID steamID)
        {
            var profile = SaveWebsite("https://steamcommunity.com/profiles/" + steamID.ConvertToUInt64());

            if (!string.IsNullOrWhiteSpace(profile))
            {
                var file = Path.Combine(_directory.ToString(), steamID.ConvertToUInt64() + ".html");

                if (File.Exists(file))
                {
                    file += "." + DateTime.Now.ToEpochTime();
                }
                
                File.WriteAllText(file, profile);
            }
        }

        public string SaveWebsite(string url)
        {
            var result = string.Empty;
            
            try
            {
                var request = (HttpWebRequest) WebRequest.Create(url);
                var response = (HttpWebResponse) request.GetResponse();
                
                var stream = response.GetResponseStream();
                
                if (response.StatusCode == HttpStatusCode.OK && stream != null)
                {
                    var reader = new StreamReader(stream, Encoding.UTF8);
                    
                    result = reader.ReadToEnd();
                    
                    reader.Close();
                    stream.Close();
                }
                else
                {
                    _log.Warning("Unable to connect to Steam: {message}", response.StatusCode);
                }
            }
            catch (WebException ex)
            {
                _log.Error(ex, "Unable to connect to Steam: {error}", ex.Message);
            }

            return result;
        }
        
    }
}
