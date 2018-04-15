using System.ComponentModel;
using CommandLine;

namespace Titan.Bootstrap.Verbs
{
    
    [Verb("report", HelpText = "Reports the provided target in the provided match with the selected options.")]
    public class ReportOptions
    {

        [Option('t', "target", Required = true,
            HelpText = "SteamID of the target that should be reported")]
        public string Target { get; set; }

        [Option('m', "match", Required = false, Default = "8",
            HelpText = "Match Sharelink with that the target should be sent into Overwatch")]
        [DefaultValue("8")]
        public string Match { get; set; } = "8";

        [Option('i', "index", Required = false, Default = -2,
            HelpText = "Index which should be used for botting: -1 for all accounts, -2 for available index")]
        [DefaultValue(-2)]
        public int Index { get; set; } = -2;
        
        // -------------------------------------
        
        [Option("abusivetext", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool AbusiveTextChat { get; set; } = true;
        
        [Option("abusivevoice", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool AbusiveVoiceChat { get; set; } = true;
        
        [Option("griefing", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool Griefing { get; set; } = true;
        
        [Option("aimhacking", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool AimHacking { get; set; } = true;
        
        [Option("wallhacking", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool WallHacking { get; set; } = true;
        
        [Option("otherhacking", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool OtherHacking { get; set; } = true;
        
    }
    
}
