using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.Spotify
{
    internal class SpotifyShowResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; } = [];
    }

    internal class SpotifyEpisodesResponse
    {
        [JsonPropertyName("items")]
        public List<SpotifyEpisodeItem> Items { get; set; } = [];
    }

    internal class SpotifyEpisodeItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("duration_ms")]
        public int DurationMs { get; set; }

        [JsonPropertyName("external_urls")]
        public SpotifyExternalUrls ExternalUrls { get; set; } = new();

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; } = [];
    }
}
