using CommandLine;

namespace Titan
{
    public class Options
    {

        [Option('t', "target", Required = true,
            HelpText = "The Steam64 ID of that target that should be commended / reported")]
        public string Target { get; set; }

        [Option('i', "id", Required = false,
            HelpText = "The CS:GO Match ID with which the target should get to Overwatch")]
        public string MatchId { get; set; } // This is required when the Mode value has been set to REPORT

        [Option('m', "mode", Required = true,
            HelpText = "The Mode in which the bot should be operating")]
        public string Mode { get; set; } // REPORT or COMMEND

        [Option('f', "file", Required = false,
            HelpText = "The file containg a list of Steam accounts owning CS:GO that should be used")]
        public string File { get; set; } = "accounts.json";

        [Option('u', "update", Required = false,
            HelpText = "Force a update of the CS:GO Protobufs from the SteamKit GitHub repository")]
        public bool ForceUpdate { get; set; } = false;

    }
}