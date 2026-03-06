using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.YouTube
{
    internal class YouTubeVideoListResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeVideoItem> Items { get; set; } = [];
    }

    internal class YouTubeVideoItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("contentDetails")]
        public YouTubeContentDetails ContentDetails { get; set; } = new();
    }

    internal class YouTubeContentDetails
    {
        [JsonPropertyName("duration")]
        public string Duration { get; set; } = string.Empty;
    }
}