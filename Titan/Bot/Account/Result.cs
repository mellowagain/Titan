namespace Titan.Bot.Account
{
    public enum Result
    {

        Success,
        AlreadyLoggedInSomewhereElse,
        AccountBanned,
        TimedOut,
        SentryRequired,
        RateLimit

    }
}