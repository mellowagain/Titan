using System.IO;
using Newtonsoft.Json;
using Serilog.Core;
using Titan.Bootstrap;
using Titan.Logging;

namespace Titan.Config
{
    public class Config
    {

        private Logger _log = LogCreator.Create();
        
        private FileInfo _file = new FileInfo(
            Path.Combine(Titan.Instance.Directory.ToString(), "config.json")
        );

        public void Load()
        {
            if (_file.Exists)
            {
                _log.Information("A config file has been provided. Titan will ignore " +
                                 "command line arguments and use the config values.");
                
                using (var reader = File.OpenText(_file.ToString()))
                {
                    try
                    {
                        Titan.Instance.Options = (Options) Titan.Instance.JsonSerializer.Deserialize(
                            reader, typeof(Options)
                        );
                    }
                    catch (JsonException ex)
                    {
                        _log.Error(ex, "Your config.json file contains errors. Ignoring...");
                    }
                }
            }
        }
        
    }
}
