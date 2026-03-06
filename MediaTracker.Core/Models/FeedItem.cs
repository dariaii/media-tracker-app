using MediaTracker.Core.Infrastructure;

namespace MediaTracker.Core.Models
{
    public class FeedItem
    {
        public string Title { get; set; } = string.Empty;

        public string? Subtitle { get; set; }

        public string? ImageUrl { get; set; }

        public string PlatformUrl { get; set; } = string.Empty;

        public string ReleaseDate { get; set; } = string.Empty;

        public DateTime ReleaseDateParsed { get; set; }

        public SubscriptionType SourceType { get; set; }

        public string? SubscriptionName { get; set; }

        public string? Badge { get; set; }

        public int SubscriptionId { get; set; }
    }
}