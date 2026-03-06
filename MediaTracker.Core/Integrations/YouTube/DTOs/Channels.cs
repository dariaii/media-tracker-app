using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.YouTube
{
    internal class YouTubeChannelListResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeChannelItem> Items { get; set; } = [];
    }

    internal class YouTubeChannelItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("snippet")]
        public YouTubeChannelSnippet Snippet { get; set; } = new();
    }

    internal class YouTubeChannelSnippet
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("thumbnails")]
        public YouTubeThumbnails Thumbnails { get; set; } = new();
    }
}