namespace Titan.Meta
{
    public class ReportInfo : TitanPayloadInfo
    {

        public ulong MatchID { get; set; }

        public bool AbusiveText { get; set; }
        public bool AbusiveVoice { get; set; }
        public bool Griefing { get; set; }
        
        public bool AimHacking { get; set; }
        public bool WallHacking { get; set; }
        public bool OtherHacking { get; set; }

    }
}
