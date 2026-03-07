using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Xml;

namespace MediaTracker.Core.Integrations.YouTube
{
    public interface IYouTubeService
    {
        Task<Metadata?> GetChannelMetadataAsync(string channelHandle);

        Task<List<YouTubeVideo>> GetLatestVideosAsync(string channelId);

        Task<List<ExploreResult>> SearchChannelsAsync(string query);
    }

    public class YouTubeService(
        HttpClient httpClient,
        IOptions<YouTubeSettings> settings,
        IMemoryCache cache,
        ILogger<YouTubeService> logger) : IYouTubeService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly YouTubeSettings _settings = settings.Value;
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<YouTubeService> _logger = logger;

        private const string BaseUrl = "https://www.googleapis.com/youtube/v3";

        public async Task<Metadata?> GetChannelMetadataAsync(string channelHandle)
        {
            var cacheKey = $"youtube:channel:metadata:{channelHandle.ToLowerInvariant()}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var channelId = await ResolveChannelHandleAsync(channelHandle);
                    if (channelId == null)
                    {
                        _logger.LogWarning("Could not resolve channel handle {ChannelHandle} to ID", channelHandle);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    var url = $"{BaseUrl}/channels?part=snippet&id={channelId}&key={_settings.ApiKey}";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("YouTube API returned {StatusCode} for channel {ChannelId}. Error: {Error}",
                            response.StatusCode, channelId, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    var result = await response.Content.ReadFromJsonAsync<YouTubeChannelListResponse>();
                    var channel = result?.Items.FirstOrDefault();

                    if (channel == null)
                    {
                        _logger.LogWarning("No channel found for ID {ChannelId}", channelId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched channel metadata for {ChannelHandle}", channelHandle);

                    var thumbnail = channel.Snippet.Thumbnails.High
                        ?? channel.Snippet.Thumbnails.Medium
                        ?? channel.Snippet.Thumbnails.Default;

                    return new Metadata(
                        channel.Snippet.Title,
                        channel.Snippet.Description,
                        thumbnail?.Url,
                        channel.Id
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching channel metadata for {ChannelHandle}", channelHandle);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return null;
                }
            });
        }

        public async Task<List<YouTubeVideo>> GetLatestVideosAsync(string channelId)
        {
            var cacheKey = $"youtube:channel:videos:{channelId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var url = $"{BaseUrl}/search?part=snippet&channelId={channelId}&order=date&type=video&maxResults={SubscriptionConstants.MaxResults}&key={_settings.ApiKey}";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("YouTube API returned {StatusCode} for channel videos {ChannelId}. Error: {Error}",
                            response.StatusCode, channelId, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<YouTubeSearchListResponse>();

                    if (result?.Items == null || result.Items.Count == 0)
                    {
                        _logger.LogInformation("No videos found for channel {ChannelId}", channelId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var videoIds = result.Items
                        .Where(v => !string.IsNullOrEmpty(v.Id.VideoId))
                        .Select(v => v.Id.VideoId!)
                        .ToList();

                    var durations = await FetchVideoDurationsAsync(videoIds);

                    _logger.LogInformation("[CACHE MISS] Fetched {Count} videos for channel {ChannelId}", result.Items.Count, channelId);

                    var thumbnail = (YouTubeThumbnails t) =>
                        (t.High ?? t.Medium ?? t.Default)?.Url;

                    return result.Items
                        .Where(v => !string.IsNullOrEmpty(v.Id.VideoId))
                        .Select(v =>
                        {
                            durations.TryGetValue(v.Id.VideoId!, out var duration);
                            return new YouTubeVideo(
                                v.Id.VideoId!,
                                v.Snippet.Title,
                                v.Snippet.Description,
                                v.Snippet.PublishedAt,
                                v.Snippet.ChannelTitle,
                                $"https://www.youtube.com/watch?v={v.Id.VideoId}",
                                thumbnail(v.Snippet.Thumbnails),
                                duration
                            );
                        }).Take(SubscriptionConstants.MaxResults).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching videos for channel {ChannelId}", channelId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }

        public async Task<List<ExploreResult>> SearchChannelsAsync(string query)
        {
            var cacheKey = $"youtube:search:channels:{query.ToLowerInvariant()}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var url = $"{BaseUrl}/search?part=snippet&q={Uri.EscapeDataString(query)}&type=channel&maxResults={SubscriptionConstants.MaxSearchResults}&key={_settings.ApiKey}";
                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("YouTube API returned {StatusCode} for channel search {Query}. Error: {Error}",
                            response.StatusCode, query, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<YouTubeSearchListResponse>();

                    if (result?.Items == null)
                    {
                        _logger.LogInformation("No channels found for query {Query}", query);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched {Count} channels for query {Query}", result.Items.Count, query);

                    return result.Items
                        .Where(c => c.Id.ChannelId != null)
                        .Select(c => new ExploreResult
                        {
                            PlatformItemId = c.Id.ChannelId!,
                            Name = c.Snippet.Title,
                            Description = c.Snippet.Description,
                            ImageUrl = c.Snippet.Thumbnails.High?.Url,
                            SourceType = SubscriptionType.Youtube,
                            PlatformUrl = $"https://www.youtube.com/channel/{c.Id.ChannelId}"
                        }).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching channels with query {Query}", query);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }

        private async Task<Dictionary<string, string>> FetchVideoDurationsAsync(List<string> videoIds)
        {
            var result = new Dictionary<string, string>();

            if (videoIds.Count == 0)
                return result;

            try
            {
                var url = $"{BaseUrl}/videos?part=contentDetails&id={string.Join(",", videoIds)}&key={_settings.ApiKey}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Failed to fetch video durations");
                    return result;
                }

                var details = await response.Content.ReadFromJsonAsync<YouTubeVideoListResponse>();

                if (details?.Items == null)
                    return result;

                foreach (var item in details.Items)
                {
                    try
                    {
                        var timeSpan = XmlConvert.ToTimeSpan(item.ContentDetails.Duration);
                        result[item.Id] = timeSpan.TotalHours >= 1
                            ? timeSpan.ToString(@"h\:mm\:ss")
                            : timeSpan.ToString(@"m\:ss");
                    }
                    catch
                    {
                        _logger.LogDebug("Failed to parse duration for video {VideoId}", item.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error fetching video durations");
            }

            return result;
        }

        private async Task<string?> ResolveChannelHandleAsync(string channelHandle)
        {
            var cacheKey = $"youtube:handle:{channelHandle.ToLowerInvariant()}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    var searchQuery = channelHandle.TrimStart('@');
                    var url = $"{BaseUrl}/search?part=snippet&q={searchQuery}&type=channel&maxResults=1&key={_settings.ApiKey}";

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("YouTube API returned {StatusCode} when resolving handle {Handle}. Error: {Error}",
                            response.StatusCode, channelHandle, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    var result = await response.Content.ReadFromJsonAsync<YouTubeSearchListResponse>();
                    var channelResult = result?.Items.FirstOrDefault();

                    if (channelResult?.Id?.ChannelId == null)
                    {
                        _logger.LogWarning("No channel found for handle {Handle}", channelHandle);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    _logger.LogInformation("[CACHE MISS] Resolved handle {Handle} to channel ID {ChannelId}",
                        channelHandle, channelResult.Id.ChannelId);

                    return channelResult.Id.ChannelId;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error resolving channel handle {Handle}", channelHandle);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return null;
                }
            });
        }
    }
}