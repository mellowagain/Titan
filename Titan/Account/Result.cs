namespace Titan.Account
{
    public enum Result
    {

        Success,
        AlreadyLoggedInSomewhereElse,
        AccountBanned,
        TimedOut,
        SentryRequired,
        RateLimit,
        NoMatches,
        Unknown

    }
}