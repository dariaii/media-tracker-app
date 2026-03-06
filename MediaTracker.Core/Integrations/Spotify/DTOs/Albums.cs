using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.Spotify
{
    internal class SpotifyArtistResponse
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; } = [];
    }

    internal class SpotifyAlbumsResponse
    {
        [JsonPropertyName("items")]
        public List<SpotifyAlbumItem> Items { get; set; } = [];
    }

    internal class SpotifyAlbumItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("release_date")]
        public string ReleaseDate { get; set; } = string.Empty;

        [JsonPropertyName("album_type")]
        public string AlbumType { get; set; } = string.Empty;

        [JsonPropertyName("external_urls")]
        public SpotifyExternalUrls ExternalUrls { get; set; } = new();

        [JsonPropertyName("artists")]
        public List<SpotifyArtistRef> Artists { get; set; } = [];

        [JsonPropertyName("images")]
        public List<SpotifyImage> Images { get; set; } = [];
    }
}
