namespace MediaTracker.Core.Integrations.Spotify
{
    public class SpotifySettings
    {
        public const string SectionName = "SpotifySettings";

        public string ClientId { get; set; } = string.Empty;
        public string ClientSecret { get; set; } = string.Empty;
    }
}