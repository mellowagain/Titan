using System.ComponentModel;
using CommandLine;

namespace Titan.Bootstrap.Verbs
{
    
    [Verb("commend", HelpText = "Commends the provided target with the selected options")]
    public class CommendOptions
    {
        
        [Option('t', "target", Required = true,
            HelpText = "SteamID of the target that should be reported")]
        public string Target { get; set; }
        
        [Option('i', "index", Required = false, Default = -2,
            HelpText = "Index which should be used for botting: -1 for all accounts, -2 for available index")]
        [DefaultValue(-2)]
        public int Index { get; set; } = -2;
        
        // -------------------------------------
        
        [Option("leader", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool Leader { get; set; } = true;
        
        [Option("friendly", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool Friendly { get; set; } = true;
        
        [Option("teacher", Default = true, Required = false)]
        [DefaultValue(true)]
        public bool Teacher { get; set; } = true;
        
    }
    
}
