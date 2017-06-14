using System.ComponentModel;
using CommandLine;

namespace Titan.Bootstrap
{
    public class Options
    {

        [Option('t', "target", Required = true,
            HelpText = "The Steam64 ID of that target that should be commended / reported")]
        public string Target { get; set; }

        [Option('i', "id", Required = false,
            HelpText = "The CS:GO Match ID with which the target should get to Overwatch")]
        public string MatchID { get; set; }

        [Option('m', "mode", Required = true,
            HelpText = "The Mode in which the bot should be operating")]
        public string Mode { get; set; } // REPORT, COMMEND or UNCOMMEND

        [Option('f', "file", Required = false,
            HelpText = "The file containg a list of Steam accounts owning CS:GO that should be used")]
        [DefaultValue("accounts.json")]
        public string File { get; set; }

        [Option('d', "debug", Required = false,
            HelpText = "Should the Titan Debug Mode be enabled?")]
        [DefaultValue(false)]
        public bool Debug { get; set; } = true; // TODO: Remove this "true" on release.

    }
}