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
        public string Mode { get; set; } // REPORT, COMMEND

        //////////////////////////////////////////////////////////////////////////////
        
        [Option("abusivetext", Required = false)]
        public bool AbusiveTextChat { get; set; } = true;
        
        [Option("abusivevoice", Required = false)]
        public bool AbusiveVoiceChat { get; set; } = true;
        
        [Option("griefing", Required = false)]
        public bool Griefing { get; set; } = true;
        
        [Option("aimhacking", Required = false)]
        public bool AimHacking { get; set; } = true;
        
        [Option("wallhacking", Required = false)]
        public bool WallHacking { get; set; } = true;
        
        [Option("otherhacking", Required = false)]
        public bool OtherHacking { get; set; } = true;
        
        [Option("leader", Required = false)]
        public bool Leader { get; set; } = true;
        
        [Option("friendly", Required = false)]
        public bool Friendly { get; set; } = true;
        
        [Option("teacher", Required = false)]
        public bool Teacher { get; set; } = true;
        
        //////////////////////////////////////////////////////////////////////////////

        [Option('f', "file", Required = false,
            HelpText = "The file containg a list of Steam accounts owning CS:GO that should be used")]
        [DefaultValue("accounts.json")]
        public string File { get; set; } = "accounts.json";

        [Option('d', "debug", Required = false,
            HelpText = "Should the Titan Debug Mode be enabled?")]
        [DefaultValue(false)]
        public bool Debug { get; set; } = true; // TODO: Remove this "true" on release.

    }
}