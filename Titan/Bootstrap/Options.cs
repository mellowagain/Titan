using System.ComponentModel;
using CommandLine;
using Newtonsoft.Json;

namespace Titan.Bootstrap
{
    public class Options
    {
        
        [Option('f', "file", Default = "accounts.json", Required = false,
            HelpText = "The file containg a list of Steam accounts owning CS:GO that should be used")]
        [DefaultValue("accounts.json")]
        [JsonProperty("accounts_file", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string AccountsFile { get; set; } = "accounts.json";

        [Option('s', "secure", Default = false, Required = false,
            HelpText = "If Secure Mode is enabled, all logs in the console like account passwords and Web API key" +
                       "will be hidden. Use this if you're recording a video or taking a screenshot of Titan.")]
        [DefaultValue(false)]
        [JsonProperty("secure", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Secure { get; set; } = false;

        [Option('a', "admin", Default = false, Required = false,
            HelpText = "Allow administrators to execute this program. This is NOT recommended as it may " +
                       "cause security issues. (Steam also doesn't allow to be run as root)")]
        [DefaultValue(false)]
        [JsonProperty("allow_admin", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool AllowAdmin { get; set; } = false;

        [Option('g', "nosteamgroup", Default = false, Required = false,
            HelpText = "Disables automatic joining of the Titan Report Bot Steam Group")]
        [DefaultValue(false)]
        [JsonProperty("no_steam_group", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool NoSteamGroup { get; set; } = false;
        
        [Option('b', "noblacklist", Default = false, Required = false,
            HelpText = "Disables the Blacklist that is preventing botting of the authors and friend's own bots")]
        [DefaultValue(false)]
        [JsonProperty("disable_blacklist", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool DisableBlacklist { get; set; } = false;

        [Option('k', "steamkitdebug", Default = false, Required = false,
            HelpText = "Should SteamKit debug messages be printed?")]
        [DefaultValue(false)]
        [JsonProperty("steamkit_debug", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool SteamKitDebug { get; set; } = false;
        
        [Option('d', "debug", Default = true, Required = false, Hidden = true,
            HelpText = "Should the Titan Debug Mode be enabled?")]
        [DefaultValue(true)]
        [JsonProperty("debug", DefaultValueHandling = DefaultValueHandling.Ignore)]
        public bool Debug { get; set; } = true; 
        
    }
}