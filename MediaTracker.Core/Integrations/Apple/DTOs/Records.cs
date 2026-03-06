namespace MediaTracker.Core.Integrations.Apple
{
    public record ApplePodcastEpisode(
        string Id,
        string Name,
        string Description,
        string ReleaseDate,
        TimeSpan Duration,
        string EpisodeUrl,
        string? ImageUrl);
}