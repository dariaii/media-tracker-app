using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;

namespace MediaTracker.Core.Integrations.Spotify
{
    public interface ISpotifyService
    {
        Task<Metadata?> GetArtistMetadataAsync(string artistId);

        Task<Metadata?> GetShowMetadataAsync(string showId);

        Task<List<SpotifyRelease>> GetArtistReleasesAsync(string artistId);

        Task<List<SpotifyEpisode>> GetPodcastEpisodesAsync(string showId);

        Task<List<ExploreResult>> SearchAsync(string query);
    }

    public class SpotifyService(
        HttpClient httpClient,
        IOptions<SpotifySettings> settings,
        IMemoryCache cache,
        ILogger<SpotifyService> logger) : ISpotifyService
    {
        private readonly HttpClient _httpClient = httpClient;
        private readonly SpotifySettings _settings = settings.Value;
        private readonly IMemoryCache _cache = cache;
        private readonly ILogger<SpotifyService> _logger = logger;

        private string? _accessToken;
        private DateTime _tokenExpiry = DateTime.MinValue;

        public async Task<Metadata?> GetArtistMetadataAsync(string artistId)
        {
            var cacheKey = $"spotify:artist:metadata:{artistId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    await EnsureAccessTokenAsync();

                    var url = $"https://api.spotify.com/v1/artists/{artistId}";
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Spotify API returned {StatusCode} for artist metadata {ArtistId}. Error: {Error}",
                            response.StatusCode, artistId, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    var result = await response.Content.ReadFromJsonAsync<SpotifyArtistResponse>();

                    if (result == null)
                    {
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched artist metadata for {ArtistId}", artistId);

                    return new Metadata(
                        result.Name,
                        null,
                        result.Images.FirstOrDefault()?.Url,
                        artistId
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching artist metadata for {ArtistId}", artistId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return null;
                }
            });
        }

        public async Task<Metadata?> GetShowMetadataAsync(string showId)
        {
            var cacheKey = $"spotify:show:metadata:{showId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    await EnsureAccessTokenAsync();

                    var url = $"https://api.spotify.com/v1/shows/{showId}";
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Spotify API returned {StatusCode} for show metadata {ShowId}. Error: {Error}",
                            response.StatusCode, showId, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    var result = await response.Content.ReadFromJsonAsync<SpotifyShowResponse>();

                    if (result == null)
                    {
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return null;
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched show metadata for {ShowId}", showId);

                    return new Metadata(
                        result.Name,
                        result.Description,
                        result.Images.FirstOrDefault()?.Url,
                        showId
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching show metadata for {ShowId}", showId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return null;
                }
            });
        }

        public async Task<List<SpotifyRelease>> GetArtistReleasesAsync(string artistId)
        {
            var cacheKey = $"spotify:artist:releases:{artistId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    await EnsureAccessTokenAsync();

                    var url = $"https://api.spotify.com/v1/artists/{artistId}/albums?limit={SubscriptionConstants.MaxResults}&market=BG";
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Spotify API returned {StatusCode} for artist {ArtistId}. Error: {Error}",
                            response.StatusCode, artistId, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<SpotifyAlbumsResponse>();

                    if (result?.Items == null || result.Items.Count == 0)
                    {
                        _logger.LogInformation("No releases found for artist {ArtistId}", artistId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched {Count} releases for artist {ArtistId}", result.Items.Count, artistId);

                    return result.Items.Select(a => new SpotifyRelease(
                        a.Id,
                        a.Name,
                        a.ReleaseDate,
                        a.AlbumType,
                        a.ExternalUrls.Spotify,
                        [.. a.Artists.Select(ar => ar.Name)],
                        a.Images.FirstOrDefault()?.Url
                    )).OrderByDescending(r => r.ReleaseDate).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching artist releases for {ArtistId}", artistId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }

        public async Task<List<SpotifyEpisode>> GetPodcastEpisodesAsync(string showId)
        {
            var cacheKey = $"spotify:show:episodes:{showId}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    await EnsureAccessTokenAsync();

                    var url = $"https://api.spotify.com/v1/shows/{showId}/episodes?limit={SubscriptionConstants.MaxResults}";
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Spotify API returned {StatusCode} for show {ShowId}. Error: {Error}",
                            response.StatusCode, showId, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<SpotifyEpisodesResponse>();

                    if (result?.Items == null || result.Items.Count == 0)
                    {
                        _logger.LogInformation("No episodes found for show {ShowId}", showId);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched {Count} episodes for show {ShowId}", result.Items.Count, showId);

                    return result.Items.Select(e => new SpotifyEpisode(
                        e.Id,
                        e.Name,
                        e.Description,
                        e.ReleaseDate,
                        TimeSpan.FromMilliseconds(e.DurationMs),
                        e.ExternalUrls.Spotify,
                        e.Images.FirstOrDefault()?.Url
                    )).OrderByDescending(r => r.ReleaseDate).ToList();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching podcast episodes for {ShowId}", showId);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }

        public async Task<List<ExploreResult>> SearchAsync(string query)
        {
            var cacheKey = $"spotify:search:{query}";

            return await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.CacheDuration;

                try
                {
                    await EnsureAccessTokenAsync();

                    var url = $"https://api.spotify.com/v1/search?q={Uri.EscapeDataString(query)}&type=artist,album,track,show&limit={SubscriptionConstants.MaxSearchResults}";
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                    var response = await _httpClient.GetAsync(url);

                    if (!response.IsSuccessStatusCode)
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Spotify API returned {StatusCode} for search query '{Query}'. Error: {Error}",
                            response.StatusCode, query, errorContent);
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    var result = await response.Content.ReadFromJsonAsync<SpotifySearchResponse>();

                    if (result == null)
                    {
                        entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                        return [];
                    }

                    _logger.LogInformation("[CACHE MISS] Fetched search results for query '{Query}'", query);

                    var exploreResults = new List<ExploreResult>();

                    if (result.Shows?.Items != null)
                    {
                        exploreResults.AddRange(result.Shows.Items.Select(s => new ExploreResult
                        {
                            PlatformItemId = s.Id,
                            Name = s.Name,
                            Description = s.Publisher,
                            SourceType = SubscriptionType.SpotifyPodcasts,
                            ImageUrl = s.Images.FirstOrDefault()?.Url,
                            PlatformUrl = s.ExternalUrls?.Spotify ?? $"https://open.spotify.com/show/{s.Id}"
                        }));
                    }

                    if (result.Artists?.Items != null)
                    {
                        exploreResults.AddRange(result.Artists.Items.Select(a => new ExploreResult
                        {
                            PlatformItemId = a.Id,
                            Name = a.Name,
                            SourceType = SubscriptionType.SpotifyArtist,
                            ImageUrl = a.Images.FirstOrDefault()?.Url,
                            PlatformUrl = a.ExternalUrls?.Spotify ?? $"https://open.spotify.com/artist/{a.Id}"
                        }));
                    }

                    return exploreResults;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error searching Spotify for query '{Query}'", query);
                    entry.AbsoluteExpirationRelativeToNow = SubscriptionConstants.NoCacheDuration;
                    return [];
                }
            }) ?? [];
        }

        private async Task EnsureAccessTokenAsync()
        {
            if (_accessToken is not null && DateTime.UtcNow < _tokenExpiry)
                return;

            var credentials = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));

            var request = new HttpRequestMessage(HttpMethod.Post, "https://accounts.spotify.com/api/token")
            {
                Content = new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["grant_type"] = "client_credentials"
                })
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);

            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();

            var token = await response.Content.ReadFromJsonAsync<SpotifyTokenResponse>();
            _accessToken = token!.AccessToken;
            _tokenExpiry = DateTime.UtcNow.AddSeconds(token.ExpiresIn - 60);

            _logger.LogInformation("Spotify access token acquired, expires in {Seconds}s.", token.ExpiresIn);
        }
    }
}