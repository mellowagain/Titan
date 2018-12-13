using SteamKit2.GC.TF2.Internal;

namespace Titan.Meta
{
    public class ReportInfo : TitanPayloadInfo
    {

        public ulong MatchID { get; set; }
        public ulong GameServerID { get; set; } = 0;

        public bool AbusiveText { get; set; } = true;
        public bool AbusiveVoice { get; set; } = true;
        public bool Griefing { get; set; } = true;
        
        public bool AimHacking { get; set; } = true;
        public bool WallHacking { get; set; } = true;
        public bool OtherHacking { get; set; } = true;

        public CMsgGC_ReportPlayer.EReason Reason { get; set; } = CMsgGC_ReportPlayer.EReason.kReason_INVALID;

    }
}
