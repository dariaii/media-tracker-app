using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.Spotify
{
    internal class SpotifyExternalUrls
    {
        [JsonPropertyName("spotify")]
        public string Spotify { get; set; } = string.Empty;
    }

    internal class SpotifyArtistRef
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;
    }

    internal class SpotifyImage
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}
