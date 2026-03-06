using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;

namespace MediaTracker.Core.Integrations.Apple
{
    public interface IApplePodcastService
    {
        Task<Metadata?> GetShowMetadataAsync(string podcastId);

        Task<List<ApplePodcastEpisode>> GetPodcastEpisodesAsync(string podcastId);

        Task<List<ExploreResult>> SearchPodcastsAsync(string query);
    }

    public class ApplePodcastService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<ApplePodcastService> logger) : IApplePodcastService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<ApplePodcastService> _logger = logger;

        private const string BaseUrl = "https://itunes.apple.com";

        public async Task<Metadata?> GetShowMetadataAsync(string podcastId)
        {
            var cacheKey = $"apple:podcast:metadata:{podcastId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var url = $"{BaseUrl}/lookup?id={podcastId}&entity=podcast";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Apple API returned {StatusCode} for podcast {PodcastId}",
                            response.StatusCode, podcastId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    var result = await response.Content.ReadFromJsonAsync<AppleSearchResponse>();
                    var podcast = result?.Results.FirstOrDefault();

                    if (podcast == null)
                    {
                        _logger.LogWarning("No podcast found for ID {PodcastId}", podcastId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched Apple podcast metadata for {PodcastId}", podcastId);

                    return new Metadata(
                        podcast.CollectionName,
                        podcast.ArtistName,
                        podcast.ArtworkUrl600 ?? podcast.ArtworkUrl100,
                        podcastId
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Apple podcast metadata for {PodcastId}", podcastId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return null;
                }
            });
        }

        public async Task<List<ApplePodcastEpisode>> GetPodcastEpisodesAsync(string podcastId)
        {
            var cacheKey = $"apple:podcast:episodes:{podcastId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var url = $"{BaseUrl}/lookup?id={podcastId}&media=podcast&entity=podcastEpisode&limit={SubscriptionConstants.MaxResults}&sort=recent";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Apple API returned {StatusCode} for podcast episodes {PodcastId}",
                            response.StatusCode, podcastId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<AppleEpisodeLookupResponse>();

                    // First result is the podcast itself (wrapperType=collection), the rest are episodes
                    var episodes = result?.Results
                        .Where(r => r.WrapperType == "podcastEpisode")
                        .ToList();

                    if (episodes == null || episodes.Count == 0)
                    {
                        _logger.LogInformation("No episodes found for Apple podcast {PodcastId}", podcastId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched {Count} episodes for Apple podcast {PodcastId}",
                        episodes.Count, podcastId);

                    return episodes.Select(e => new ApplePodcastEpisode(
                        e.TrackId.ToString(),
                        e.TrackName,
                        e.Description,
                        DateTime.TryParse(e.ReleaseDate, out var dt) ? dt.ToString("yyyy-MM-dd HH:mm") : e.ReleaseDate,
                        TimeSpan.FromMilliseconds(e.TrackTimeMillis),
                        e.TrackViewUrl ?? $"https://podcasts.apple.com/podcast/id{podcastId}",
                        e.ArtworkUrl600 ?? e.ArtworkUrl160
                    )).OrderByDescending(e => e.ReleaseDate).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Apple podcast episodes for {PodcastId}", podcastId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }

        public async Task<List<ExploreResult>> SearchPodcastsAsync(string query)
        {
            var cacheKey = $"apple:podcast:search:{query}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var url = $"{BaseUrl}/search?term={Uri.EscapeDataString(query)}&media=podcast&entity=podcast&limit={SubscriptionConstants.MaxSearchResults}";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Apple API returned {StatusCode} for podcast search {Query}",
                            response.StatusCode, query);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<AppleSearchResponse>();

                    var podcasts = result?.Results;

                    if (podcasts == null || podcasts.Count == 0)
                    {
                        _logger.LogInformation("No podcasts found for search query {Query}", query);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched {Count} podcasts for search query {Query}",
                        podcasts.Count, query);

                    return podcasts.Select(p => new ExploreResult
                    {
                        PlatformItemId = p.CollectionId.ToString(),
                        Name = p.CollectionName,
                        Description = p.ArtistName,
                        ImageUrl = p.ArtworkUrl600 ?? p.ArtworkUrl100,
                        SourceType = SubscriptionType.ApplePodcasts,
                        PlatformUrl = p.CollectionViewUrl ?? ""
                    }).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching Apple podcasts for {Query}", query);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }
    }
}