using System.ComponentModel;
using CommandLine;

namespace Titan.Bootstrap
{
    public class Options
    {
        
        [Option('f', "file", Default = "accounts.json", Required = false,
            HelpText = "The file containg a list of Steam accounts owning CS:GO that should be used")]
        [DefaultValue("accounts.json")]
        public string AccountsFile { get; set; } = "accounts.json";

        [Option('s', "secure", Default = false, Required = false,
            HelpText = "If Secure Mode is enabled, all logs in the console like account passwords and Web API key" +
                       "will be hidden. Use this if you're recording a video or taking a screenshot of Titan.")]
        [DefaultValue(false)]
        public bool Secure { get; set; } = false;

        [Option('a', "admin", Default = false, Required = false,
            HelpText = "Allow administrators to execute this program. This is NOT recommended as it may " +
                       "cause security issues. (Steam also doesn't allow to be run as root)")]
        [DefaultValue(false)]
        public bool AllowAdmin { get; set; } = false;
        
        [Option('b', "noblacklist", Default = false, Required = false,
            HelpText = "Disables the Blacklist that is preventing botting of the authors and friend's own bots")]
        [DefaultValue(false)]
        public bool DisableBlacklist { get; set; } = false;

        [Option('d', "debug", Default = true, Required = false, Hidden = true,
            HelpText = "Should the Titan Debug Mode be enabled?")]
        [DefaultValue(true)]
        public bool Debug { get; set; } = true; 
        
    }
}