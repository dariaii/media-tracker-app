namespace MediaTracker.Core.Integrations.Spotify
{
    public record Metadata(
        string Name,
        string? Description,
        string? ImageUrl,
        string? PlatformItemId);

    public record SpotifyRelease(
        string Id,
        string Name,
        string ReleaseDate,
        string AlbumType,
        string SpotifyUrl,
        List<string> Artists,
        string? ImageUrl);

    public record SpotifyEpisode(
        string Id,
        string Name,
        string Description,
        string ReleaseDate,
        TimeSpan Duration,
        string SpotifyUrl,
        string? ImageUrl);
}
