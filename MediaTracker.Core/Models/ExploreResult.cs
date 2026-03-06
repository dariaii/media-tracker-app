using MediaTracker.Core.Infrastructure;

namespace MediaTracker.Core.Models
{
    public class ExploreResult
    {
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        public string? ImageUrl { get; set; }

        public string PlatformUrl { get; set; } = string.Empty;

        public SubscriptionType SourceType { get; set; }

        public string PlatformItemId { get; set; } = string.Empty;
    }
}