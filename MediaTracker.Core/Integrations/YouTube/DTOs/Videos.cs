using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.YouTube
{
    internal class YouTubeSearchListResponse
    {
        [JsonPropertyName("items")]
        public List<YouTubeSearchItem> Items { get; set; } = [];
    }

    internal class YouTubeSearchItem
    {
        [JsonPropertyName("id")]
        public YouTubeSearchId Id { get; set; } = new();

        [JsonPropertyName("snippet")]
        public YouTubeVideoSnippet Snippet { get; set; } = new();
    }

    internal class YouTubeSearchId
    {
        [JsonPropertyName("videoId")]
        public string? VideoId { get; set; }

        [JsonPropertyName("channelId")]
        public string? ChannelId { get; set; }
    }

    internal class YouTubeVideoSnippet
    {
        [JsonPropertyName("title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("publishedAt")]
        public string PublishedAt { get; set; } = string.Empty;

        [JsonPropertyName("channelTitle")]
        public string ChannelTitle { get; set; } = string.Empty;

        [JsonPropertyName("thumbnails")]
        public YouTubeThumbnails Thumbnails { get; set; } = new();
    }
}