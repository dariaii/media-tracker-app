using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Integrations.Apple;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Integrations.YouTube;
using MediaTracker.Core.Models;
using MediaTracker.Data;
using MediaTracker.Data.Entities;

namespace MediaTracker.Core.Services
{
    public interface ISubscriptionService
    {
        List<Subscription> GetSubscriptions();

        Subscription GetSubscriptionById(int id);

        Task<bool> CreateSubscriptionAsync(SubscriptionType type, string platformItemId, bool receiveNotifications);

        bool DeleteSubscription(int id);

        bool ToggleNotifications(int id);

        Task<List<FeedItem>> GetWhatsNewAsync(SubscriptionType? filter = null);

        Task<List<FeedItem>> GetLatestPerSubscriptionAsync();
    }

    public class SubscriptionService(
        IRepository repository, 
        IAuthenticationService authenticationService,
        ISpotifyService spotifyService,
        IYouTubeService youTubeService,
        IApplePodcastService applePodcastService) : ISubscriptionService
    {
        private readonly IRepository _repository = repository;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly ISpotifyService _spotifyService = spotifyService;
        private readonly IYouTubeService _youTubeService = youTubeService;
        private readonly IApplePodcastService _applePodcastService = applePodcastService;

        public List<Subscription> GetSubscriptions()
        {
            var subscriptions = _repository.SetNoTracking<Subscription>()
                .Where(s => s.UserId == _authenticationService.CurrentUser().Id)
                .ToList();

            return subscriptions;
        }

        public Subscription GetSubscriptionById(int id)
        {
            var subscription = _repository.SetNoTracking<Subscription>()
                .FirstOrDefault(s => s.Id == id && s.UserId == _authenticationService.CurrentUser().Id);
            return subscription;
        }

        public async Task<bool> CreateSubscriptionAsync(SubscriptionType type, string platformItemId, bool receiveNotifications)
        {
            Metadata metadata = null;

            switch (type)
            {
                case SubscriptionType.SpotifyArtist:
                    metadata = await _spotifyService.GetArtistMetadataAsync(platformItemId);
                    break;
                case SubscriptionType.SpotifyPodcasts:
                    metadata = await _spotifyService.GetShowMetadataAsync(platformItemId);
                    break;
                case SubscriptionType.Youtube:
                    metadata = await _youTubeService.GetChannelMetadataAsync(platformItemId);
                    break;
                case SubscriptionType.ApplePodcasts:
                    metadata = await _applePodcastService.GetShowMetadataAsync(platformItemId);
                    break;
            }

            var subscription = new Subscription
            {
                Type = (int)type,
                PlatformItemId = metadata.PlatformItemId,
                Name = metadata.Name,
                Description = metadata.Description,
                ImageUrl = metadata.ImageUrl,
                ReceiveNotifications = receiveNotifications,
                UserId = _authenticationService.CurrentUser()?.Id,
            };

            _repository.Add(subscription);
            return true;
        }

        public bool DeleteSubscription(int id)
        {
            var subscription = _repository.Set<Subscription>()
                .FirstOrDefault(s => s.Id == id && s.UserId == _authenticationService.CurrentUser().Id);

            if (subscription == null)
                return false;

            _repository.Delete(subscription);
            return true;
        }

        public bool ToggleNotifications(int id)
        {
            var subscription = _repository.Set<Subscription>()
                .FirstOrDefault(s => s.Id == id && s.UserId == _authenticationService.CurrentUser().Id);

            if (subscription == null)
                return false;

            subscription.ReceiveNotifications = !subscription.ReceiveNotifications;
            _repository.Update(subscription);
            return true;
        }

        public async Task<List<FeedItem>> GetWhatsNewAsync(SubscriptionType? filter = null)
        {
            var subscriptions = GetSubscriptions();

            if (filter.HasValue)
            {
                subscriptions = subscriptions.Where(s => s.Type == (int)filter.Value).ToList();
            }

            var feedItems = new List<FeedItem>();
            var tasks = new List<Task>();

            foreach (var sub in subscriptions)
            {
                var subType = (SubscriptionType)sub.Type;

                switch (subType)
                {
                    case SubscriptionType.SpotifyArtist:
                        tasks.Add(Task.Run(async () =>
                        {
                            var releases = await _spotifyService.GetArtistReleasesAsync(sub.PlatformItemId);
                            lock (feedItems)
                            {
                                feedItems.AddRange(releases.Select(r => new FeedItem
                                {
                                    Title = r.Name,
                                    Subtitle = string.Join(", ", r.Artists),
                                    ImageUrl = r.ImageUrl,
                                    PlatformUrl = r.SpotifyUrl,
                                    ReleaseDate = r.ReleaseDate,
                                    ReleaseDateParsed = DateTime.TryParse(r.ReleaseDate, out var dt) ? dt : DateTime.MinValue,
                                    SourceType = SubscriptionType.SpotifyArtist,
                                    SubscriptionName = sub.Name,
                                    Badge = r.AlbumType,
                                    SubscriptionId = sub.Id,
                                }));
                            }
                        }));
                        break;

                    case SubscriptionType.SpotifyPodcasts:
                        tasks.Add(Task.Run(async () =>
                        {
                            var episodes = await _spotifyService.GetPodcastEpisodesAsync(sub.PlatformItemId);
                            lock (feedItems)
                            {
                                feedItems.AddRange(episodes.Select(e => new FeedItem
                                {
                                    Title = e.Name,
                                    Subtitle = sub.Name,
                                    ImageUrl = e.ImageUrl,
                                    PlatformUrl = e.SpotifyUrl,
                                    ReleaseDate = e.ReleaseDate,
                                    ReleaseDateParsed = DateTime.TryParse(e.ReleaseDate, out var dt) ? dt : DateTime.MinValue,
                                    SourceType = SubscriptionType.SpotifyPodcasts,
                                    SubscriptionName = sub.Name,
                                    Badge = e.Duration.TotalHours >= 1
                                        ? e.Duration.ToString(@"h\:mm\:ss")
                                        : e.Duration.ToString(@"m\:ss"),
                                    SubscriptionId = sub.Id,
                                }));
                            }
                        }));
                        break;

                    case SubscriptionType.Youtube:
                        tasks.Add(Task.Run(async () =>
                        {
                            var videos = await _youTubeService.GetLatestVideosAsync(sub.PlatformItemId);
                            lock (feedItems)
                            {
                                feedItems.AddRange(videos.Select(v => new FeedItem
                                {
                                    Title = v.Title,
                                    Subtitle = v.ChannelTitle,
                                    ImageUrl = v.ImageUrl,
                                    PlatformUrl = v.YouTubeUrl,
                                    ReleaseDate = DateTime.TryParse(v.PublishedAt, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : v.PublishedAt,
                                    ReleaseDateParsed = DateTime.TryParse(v.PublishedAt, out var dt2) ? dt2 : DateTime.MinValue,
                                    SourceType = SubscriptionType.Youtube,
                                    SubscriptionName = sub.Name,
                                    Badge = v.Duration,
                                    SubscriptionId = sub.Id,
                                }));
                            }
                        }));
                        break;

                    case SubscriptionType.ApplePodcasts:
                        tasks.Add(Task.Run(async () =>
                        {
                            var episodes = await _applePodcastService.GetPodcastEpisodesAsync(sub.PlatformItemId);
                            lock (feedItems)
                            {
                                feedItems.AddRange(episodes.Select(e => new FeedItem
                                {
                                    Title = e.Name,
                                    Subtitle = sub.Name,
                                    ImageUrl = e.ImageUrl,
                                    PlatformUrl = e.EpisodeUrl,
                                    ReleaseDate = e.ReleaseDate,
                                    ReleaseDateParsed = DateTime.TryParse(e.ReleaseDate, out var dt) ? dt : DateTime.MinValue,
                                    SourceType = SubscriptionType.ApplePodcasts,
                                    SubscriptionName = sub.Name,
                                    Badge = e.Duration.TotalHours >= 1
                                        ? e.Duration.ToString(@"h\:mm\:ss")
                                        : e.Duration.ToString(@"m\:ss"),
                                    SubscriptionId = sub.Id,
                                }));
                            }
                        }));
                        break;
                }
            }

            await Task.WhenAll(tasks);

            return feedItems
                .OrderByDescending(f => f.ReleaseDateParsed)
                .ToList();
        }

        /// <summary>
        /// Returns only the single most recent item per subscription, 
        /// reusing the cached data from GetWhatsNewAsync.
        /// </summary>
        public async Task<List<FeedItem>> GetLatestPerSubscriptionAsync()
        {
            var allItems = await GetWhatsNewAsync();

            return allItems
                .GroupBy(f => f.SubscriptionId)
                .Select(g => g.OrderByDescending(f => f.ReleaseDateParsed).First())
                .OrderByDescending(f => f.ReleaseDateParsed)
                .ToList();
        }
    }
}
