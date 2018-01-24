using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Serilog.Core;
using SteamKit2;
using Titan.Logging;

namespace Titan.Proof
{
    public class ProfileScreenshotter
    {

        private Logger _log = LogCreator.Create();
        private DirectoryInfo _directory;

        public ProfileScreenshotter(DirectoryInfo dir = null)
        {
            if (dir != null)
            {
                _directory = dir;
            }
            else
            {
                _directory = new DirectoryInfo(Path.Combine(Titan.Instance.Directory.ToString(), "media"));
                
                if (!_directory.Exists)
                {
                    _directory.Create();
                }
            }
        }

        public void ScreenshotProfile(SteamID steamID)
        {
            var image = ScreenshotSite("https://steamcommunity.com/profiles/" + steamID.ConvertToUInt64());

            if (image != null)
            {
                var file = new FileInfo(Path.Combine(_directory.ToString(), steamID.ConvertToUInt64() + ".orig.png"));
                if (file.Exists)
                {
                    file = new FileInfo(Path.Combine(_directory.ToString(), steamID.ConvertToUInt64() + ".banned.png"));
                }

                using (var writer = File.OpenWrite(file.ToString()))
                {
                    image.Save(writer, ImageFormat.Png);
                }
            }
        }
        
        public Image ScreenshotSite(string url)
        {
            try
            {
                var task = Task.Run(() =>
                {
                    using (var client = new WebClient())
                    {
                        client.Headers.Add(HttpRequestHeader.UserAgent, "Titan Report & Commend Bot");

                        var data = client.DownloadData("http://s.wontfix.club/?url=" + HttpUtility.HtmlEncode(url));

                        using (var image = Image.FromStream(new MemoryStream(data)))
                        {
                            return image;
                        }
                    }
                });

                return task.Result;
            }
            catch (WebException ex) when (ex.Response is HttpWebResponse response)
            {
                if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    _log.Warning("We\'re getting rate limited. Trying again in one minute.");
                    
                    Thread.Sleep(TimeSpan.FromMinutes(1));
                    return ScreenshotSite(url);
                }

                _log.Error(ex, "Titan was unable to connect to the wontfix.club Screenshot Service.");
                return null;
            }
            catch (NotSupportedException ex)
            {
                _log.Warning(ex, "We're waiting for a screenshot at the moment. Retrying in 3 seconds.");

                Thread.Sleep(TimeSpan.FromSeconds(3));
                return ScreenshotSite(url);
            }
            catch (Exception ex)
            {
                _log.Error(ex, "A error occured with the Screenshot service.");

                return null;
            }
        }
        
    }
}