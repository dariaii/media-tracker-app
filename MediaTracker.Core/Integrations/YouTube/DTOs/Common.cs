using System.Text.Json.Serialization;

namespace MediaTracker.Core.Integrations.YouTube
{
    internal class YouTubeThumbnails
    {
        [JsonPropertyName("high")]
        public YouTubeThumbnail? High { get; set; }

        [JsonPropertyName("medium")]
        public YouTubeThumbnail? Medium { get; set; }

        [JsonPropertyName("default")]
        public YouTubeThumbnail? Default { get; set; }
    }

    internal class YouTubeThumbnail
    {
        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;
    }
}