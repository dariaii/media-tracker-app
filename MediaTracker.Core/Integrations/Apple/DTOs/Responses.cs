using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.Apple
{
    internal class AppleSearchResponse
    {
        [JsonPropertyName("resultCount")]
        public int ResultCount { get; set; }

        [JsonPropertyName("results")]
        public List<AppleSearchResult> Results { get; set; } = [];
    }

    internal class AppleSearchResult
    {
        [JsonPropertyName("collectionId")]
        public long CollectionId { get; set; }

        [JsonPropertyName("collectionName")]
        public string CollectionName { get; set; } = string.Empty;

        [JsonPropertyName("artistName")]
        public string ArtistName { get; set; } = string.Empty;

        [JsonPropertyName("artworkUrl600")]
        public string? ArtworkUrl600 { get; set; }

        [JsonPropertyName("artworkUrl100")]
        public string? ArtworkUrl100 { get; set; }

        [JsonPropertyName("collectionViewUrl")]
        public string? CollectionViewUrl { get; set; }

        [JsonPropertyName("feedUrl")]
        public string? FeedUrl { get; set; }
    }

    internal class AppleEpisodeLookupResponse
    {
        [JsonPropertyName("resultCount")]
        public int ResultCount { get; set; }

        [JsonPropertyName("results")]
        public List<AppleEpisodeResult> Results { get; set; } = [];
    }

    internal class AppleEpisodeResult
    {
        [JsonPropertyName("wrapperType")]
        public string WrapperType { get; set; } = string.Empty;

        [JsonPropertyName("trackId")]
        public long TrackId { get; set; }

        [JsonPropertyName("trackName")]
        public string TrackName { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("releaseDate")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("trackTimeMillis")]
        public long TrackTimeMillis { get; set; }

        [JsonPropertyName("trackViewUrl")]
        public string? TrackViewUrl { get; set; }

        [JsonPropertyName("artworkUrl160")]
        public string? ArtworkUrl160 { get; set; }

        [JsonPropertyName("artworkUrl600")]
        public string? ArtworkUrl600 { get; set; }

        [JsonPropertyName("collectionName")]
        public string CollectionName { get; set; } = string.Empty;
    }
}