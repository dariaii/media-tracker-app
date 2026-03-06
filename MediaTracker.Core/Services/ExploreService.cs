using MediaTracker.Core.Integrations.Apple;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Integrations.YouTube;
using MediaTracker.Core.Models;
using Microsoft.Extensions.Logging;

namespace MediaTracker.Core.Services
{
    public interface IExploreService
    {
        Task<List<ExploreResult>> SearchAsync(string query);
    }

    public class ExploreService(
        ISpotifyService spotifyService,
        IYouTubeService youTubeService,
        IApplePodcastService applePodcastService,
        ILogger<ExploreService> logger) : IExploreService
    {
        private readonly ISpotifyService _spotifyService = spotifyService;
        private readonly IYouTubeService _youTubeService = youTubeService;
        private readonly IApplePodcastService _applePodcastService = applePodcastService;
        private readonly ILogger<ExploreService> _logger = logger;

        public async Task<List<ExploreResult>> SearchAsync(string query)
        {
            try
            {
                //exclude youtube searching since it uses too many of the API daily quota

                var appleTask = _applePodcastService.SearchPodcastsAsync(query);
                var spotifyTask = _spotifyService.SearchAsync(query);
                //var youtubeTask = _youTubeService.SearchChannelsAsync(query);

                //await Task.WhenAll(spotifyTask, youtubeTask, appleTask);
                await Task.WhenAll(spotifyTask, appleTask);

                var results = new List<ExploreResult>();
                results.AddRange(appleTask.Result);
                results.AddRange(spotifyTask.Result);
                //results.AddRange(youtubeTask.Result);
                return results;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during cross-platform search for '{Query}'", query);
                return [];
            }
        }
    }
}