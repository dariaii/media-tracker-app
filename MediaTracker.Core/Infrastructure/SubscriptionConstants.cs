namespace MediaTracker.Core.Infrastructure
{
    public static class SubscriptionConstants
    {
        public static readonly TimeSpan CacheDuration = TimeSpan.FromHours(4);
        public static readonly TimeSpan NoCacheDuration = TimeSpan.FromSeconds(1);

        public static readonly int MaxResults = 10;
        public static readonly int MaxSearchResults = 5;
    }
}
