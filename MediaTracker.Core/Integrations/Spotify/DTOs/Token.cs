using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.Spotify
{
    internal class SpotifyTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }
    }
}
