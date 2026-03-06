namespace MediaTracker.Core.Integrations.YouTube
{
    public record YouTubeVideo(
        string VideoId,
        string Title,
        string Description,
        string PublishedAt,
        string ChannelTitle,
        string YouTubeUrl,
        string? ImageUrl,
        string? Duration);
}