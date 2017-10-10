using System.ComponentModel;
using CommandLine;

namespace Titan.Bootstrap
{
    public class Options
    {
        
        [Option('f', "file", Default = "accounts.json", Required = false,
            HelpText = "The file containg a list of Steam accounts owning CS:GO that should be used")]
        [DefaultValue("accounts.json")]
        public string AccountsFile { get; } = "accounts.json";
        
        [Option('b', "noblacklist", Default = false, Required = false,
            HelpText = "Disables the Blacklist that is preventing botting of the authors and friend's own bots")]
        [DefaultValue(false)]
        public bool DisableBlacklist { get; } = false;

        [Option('d', "debug", Default = true, Required = false, Hidden = true,
            HelpText = "Should the Titan Debug Mode be enabled?")]
        [DefaultValue(true)]
        public bool Debug { get; } = true; 
        
    }
}