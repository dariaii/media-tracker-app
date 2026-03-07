using Hangfire;
using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Integrations.Apple;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Integrations.YouTube;
using MediaTracker.Core.Models;
using MediaTracker.Core.Services;
using MediaTracker.Data;
using MediaTracker.Data.Entities;
using System.Text;

namespace MediaTracker.Infrastructure.Hangfire
{
    [AutomaticRetry(Attempts = 3)]
    [DisableConcurrentExecution(timeoutInSeconds: 3600)]
    public class DailyJob(
        IRepository repository,
        ISpotifyService spotifyService,
        IYouTubeService youTubeService,
        IApplePodcastService applePodcastService,
        IEmailService emailService)
    {
        public async Task ExecuteAsync()
        {
            var from = DateTime.Today.AddDays(-7);
            var to   = DateTime.Today;

            var subscriptions = repository
                .SetNoTracking<Subscription>(nameof(Subscription.User))
                .Where(s => s.ReceiveNotifications)
                .ToList();

            if (subscriptions.Count == 0)
                return;

            var allFeedItems = await FetchAllFeedItemsAsync(subscriptions);

            var userEmails = subscriptions
                .GroupBy(s => s.UserId)
                .ToDictionary(g => g.Key, g => g.First().User?.Email);

            var userReleases = subscriptions.GroupBy(s => s.UserId)
                .ToDictionary(
                    g => g.Key,
                    g =>
                    {
                        var userSubIds = g.Select(s => s.Id).ToHashSet();
                        return allFeedItems
                            .Where(f => userSubIds.Contains(f.SubscriptionId)
                                     && f.ReleaseDateParsed.Date >= from
                                     && f.ReleaseDateParsed.Date < to)
                            .OrderByDescending(f => f.ReleaseDateParsed)
                            .ToList();
                    });

            foreach (var (userId, releases) in userReleases.Where(kvp => kvp.Value.Count > 0))
            {
                var email = userEmails.GetValueOrDefault(userId);
                if (string.IsNullOrEmpty(email))
                    continue;

                var subject = $"Your weekly media digest — {from:dd MMM} – {to.AddDays(-1):dd MMM yyyy}";
                var body = BuildEmailBody(releases, from, to.AddDays(-1));

                emailService.SendEmail([email], subject, body);
            }
        }

        private static string BuildEmailBody(List<FeedItem> releases, DateTime from, DateTime to)
        {
            var sb = new StringBuilder();

            sb.Append($"""
                <div style="font-family: sans-serif; max-width: 620px; margin: auto; color: #1a1a1a;">
                    <h2 style="margin-bottom: 4px;">Your Weekly Media Digest</h2>
                    <p style="color: #888; margin-top: 0;">{from:dd MMM} – {to:dd MMM yyyy}</p>
                    <hr style="border: none; border-top: 1px solid #eee;" />
                """);

            foreach (var item in releases)
            {
                var platform = GetPlatformLabel(item.SourceType);
                var platformColor = GetPlatformColor(item.SourceType);
                var actionLabel = item.SourceType == SubscriptionType.Youtube ? "Watch →" : "Listen →";
                var releaseDate = item.ReleaseDateParsed != DateTime.MinValue
                    ? item.ReleaseDateParsed.ToString("dd MMM yyyy")
                    : item.ReleaseDate;

                sb.Append($"""
                    <div style="margin: 16px 0; padding: 14px 16px; border: 1px solid #eee; border-radius: 8px;">
                        <span style="font-size: 11px; font-weight: 600; text-transform: uppercase; color: {platformColor};">{platform}</span>
                        <p style="margin: 6px 0 2px; font-size: 16px; font-weight: 600;">{System.Net.WebUtility.HtmlEncode(item.Title)}</p>
                        <p style="margin: 0 0 4px; font-size: 13px; color: #888;">{System.Net.WebUtility.HtmlEncode(item.SubscriptionName ?? item.Subtitle ?? string.Empty)}</p>
                        <p style="margin: 0 0 8px; font-size: 12px; color: #bbb;">Released {releaseDate}</p>
                        <a href="{item.PlatformUrl}" style="font-size: 13px; color: {platformColor}; text-decoration: none;">{actionLabel}</a>
                    </div>
                    """);
            }

            sb.Append($"""
                    <hr style="border: none; border-top: 1px solid #eee;" />
                    <p style="font-size: 11px; color: #bbb;">
                        You're receiving this because you have notifications enabled for {releases.Count} item{(releases.Count == 1 ? "" : "s")} across your subscriptions.
                    </p>
                </div>
                """);

            return sb.ToString();
        }

        private static string GetPlatformLabel(SubscriptionType type) => type switch
        {
            SubscriptionType.SpotifyArtist   => "Spotify",
            SubscriptionType.SpotifyPodcasts => "Spotify Podcasts",
            SubscriptionType.Youtube         => "YouTube",
            SubscriptionType.ApplePodcasts   => "Apple Podcasts",
            _                                => "Unknown"
        };

        private static string GetPlatformColor(SubscriptionType type) => type switch
        {
            SubscriptionType.SpotifyArtist   => "#1DB954",
            SubscriptionType.SpotifyPodcasts => "#1DB954",
            SubscriptionType.Youtube         => "#FF0000",
            SubscriptionType.ApplePodcasts   => "#FC3C44",
            _                                => "#888888"
        };

        private async Task<List<FeedItem>> FetchAllFeedItemsAsync(List<Subscription> subscriptions)
        {
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
                            var releases = await spotifyService.GetArtistReleasesAsync(sub.PlatformItemId);
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
                            var episodes = await spotifyService.GetPodcastEpisodesAsync(sub.PlatformItemId);
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
                            var videos = await youTubeService.GetLatestVideosAsync(sub.PlatformItemId);
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
                            var episodes = await applePodcastService.GetPodcastEpisodesAsync(sub.PlatformItemId);
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
            return feedItems;
        }
    }
}