using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Models;

namespace MediaTracker.ViewModels
{
    public class WhatsNewViewModel
    {
        public List<FeedItem> Items { get; set; } = [];

        public SubscriptionType? ActiveFilter { get; set; }
    }
}