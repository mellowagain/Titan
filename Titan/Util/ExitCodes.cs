using System;

namespace Titan.Util
{
    [Flags]
    public enum ExitCodes : int
    {
        
        /* Success */
        Ok = 1 << 0,
        
        /* Normal failures */
        RunningAsAdmin = 1 << 1,
        UIInitFailed = 1 << 2,
        InvalidWebAPIKey = 1 << 3,
        UnableToParseAccounts = 1 << 4,
        
        /* Botting failures */
        ReportFailed = 1 << 5,
        CommendFailed = 1 << 6,

        /* What are you doing failures */
        WrongOS = 1 << 7

    }
}
