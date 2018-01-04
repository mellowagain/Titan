namespace Titan.Meta
{
    public class CommendInfo : TitanPayloadInfo
    {
        
        public bool Leader { get; set; }
        public bool Friendly { get; set; }
        public bool Teacher { get; set; }

        // Ghetto way. I'm sorry.
        public string ToPrettyString()
        {
            if (Leader && Friendly && Teacher)
            {
                return "Leader, Friendly and Teacher";
            }
            else if (Leader && Friendly)
            {
                return "Leader and Friendly";
            }
            else if (Friendly && Teacher)
            {
                return "Friendly and Teacher";
            }
            else if (Leader && Teacher)
            {
                return "Leader and Teacher";
            }
            else if (Leader)
            {
                return "a Leader";
            }
            else if (Friendly)
            {
                return "a Friendly";
            }
            else if (Teacher)
            {
                return "a Teacher";
            }
            else
            {
                return "nothing";
            }
        }
        
    }
}
