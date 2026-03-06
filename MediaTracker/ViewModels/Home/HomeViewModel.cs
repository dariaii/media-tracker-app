using MediaTracker.Core.Models;
using MediaTracker.Data.Entities;

namespace MediaTracker.ViewModels
{
    public class HomeViewModel
    {
        public List<FeedItem> LatestItems { get; set; } = [];

        public List<Subscription> Subscriptions { get; set; } = [];
    }
}
