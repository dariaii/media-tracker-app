using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.Spotify
{
    internal class SpotifySearchResponse
    {
        [JsonPropertyName("artists")]
        public SpotifySearchCategory? Artists { get; set; }

        [JsonPropertyName("shows")]
        public SpotifySearchCategory? Shows { get; set; }
    }

    internal class SpotifySearchCategory
    {
        [JsonPropertyName("items")]
        public List<SpotifySearchItem> Items { get; set; } = [];
    }

    internal class SpotifySearchItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; } = [];

        [JsonPropertyName("external_urls")]
        public SpotifyExternalUrls ExternalUrls { get; set; } = new();

        [JsonPropertyName("publisher")]
        public string? Publisher { get; set; }
    }
}