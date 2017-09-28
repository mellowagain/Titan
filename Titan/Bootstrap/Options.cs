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

        [Option('d', "debug", Default = true, Required = false, Hidden = true,
            HelpText = "Should the Titan Debug Mode be enabled?")]
        [DefaultValue(true)]
        public bool Debug { get; } = true; 

        [Verb("report", 
            HelpText = "Reports the provided target in the provided Match ID with the provided options")]
        public class ReportOptions
        {
            
            [Option('t', "target", Required = true,
                HelpText = "The Steam64 ID of that target that should be reported")]
            public string Target { get; }

            [Option('i', "id", Default = "8", Required = false,
                HelpText = "The CS:GO Match ID in which the target should get to Overwatch")]
            [DefaultValue("8")]
            public string MatchID { get; } = "8";
            
            // -----------------------------------------------------------------------
            
            [Option("abusivetext", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool AbusiveTextChat { get; } = true;
        
            [Option("abusivevoice", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool AbusiveVoiceChat { get; } = true;
        
            [Option("griefing", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool Griefing { get; } = true;
        
            [Option("aimhacking", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool AimHacking { get; } = true;
        
            [Option("wallhacking", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool WallHacking { get; } = true;
        
            [Option("otherhacking", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool OtherHacking { get; } = true;

        }

        [Verb("commend", 
            HelpText = "Commends the provided target with the provided options")]
        public class CommendOptions
        {
            
            [Option('t', "target", Required = true,
                HelpText = "The Steam64 ID of that target that should be commended")]
            public string Target { get; }
            
            // -----------------------------------------------------------------------
            
            [Option("leader", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool Leader { get; } = true;
        
            [Option("friendly", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool Friendly { get; } = true;
        
            [Option("teacher", Default = true, Required = false)]
            [DefaultValue(true)]
            public bool Teacher { get; } = true;
            
        }

        [Verb("idle", Hidden = true,
            HelpText = "Idles the provided account in the provided game for provided hours")]
        public class IdleOptions
        {
            
            // TODO
            
        }

    }
}