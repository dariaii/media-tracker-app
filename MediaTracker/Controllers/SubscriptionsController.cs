using MediaTracker.Core.Infrastructure;
using MediaTracker.Core.Integrations.Apple;
using MediaTracker.Core.Integrations.Spotify;
using MediaTracker.Core.Integrations.YouTube;
using MediaTracker.Core.Services;
using MediaTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MediaTracker.Controllers
{
    [Authorize]
    public class SubscriptionsController(
        ILogger<SubscriptionsController> logger,
        IStringLocalizer localizer,
        ISubscriptionService subscriptionService,
        ISpotifyService spotifyService,
        IYouTubeService youTubeService,
        IApplePodcastService applePodcastService) : BaseController
    {
        private readonly ILogger<SubscriptionsController> _logger = logger;
        private readonly IStringLocalizer _localizer = localizer;
        private readonly ISubscriptionService _subscriptionService = subscriptionService;
        private readonly ISpotifyService _spotifyService = spotifyService;
        private readonly IYouTubeService _youTubeService = youTubeService;
        private readonly IApplePodcastService _applePodcastService = applePodcastService;

        [HttpGet]
        [Route("/subscriptions")]
        public IActionResult Index()
        {
            return View(_subscriptionService.GetSubscriptions());
        }

        [HttpGet]
        [Route("/subscriptions/create")]
        public IActionResult Create([FromQuery] SubscriptionType? type, [FromQuery] string? platformItemId, [FromQuery] string? returnUrl)
        {
            var model = new CreateSubscriptionViewModel();
            
            if (type.HasValue) 
                model.Type = type.Value;

            if (!string.IsNullOrEmpty(platformItemId)) 
                model.PlatformItemId = platformItemId;

            model.ReturnUrl = returnUrl;
            
            return View(model);
        }

        [HttpPost]
        [Route("/subscriptions/create")]
        public async Task<IActionResult> Create(CreateSubscriptionViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    await _subscriptionService.CreateSubscriptionAsync(model.Type, model.PlatformItemId.Trim(), model.ReceiveNotifications);
                    TempData["SuccessMessage"] = _localizer["Subscription_CreatedMessage"].Value;

                    if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating subscription.");
                    ModelState.AddModelError(string.Empty, "An unexpected error occurred while creating the subscription.");
                }
            }

            return View(model);
        }

        [HttpPost]
        [Route("/subscriptions/{id:int}/delete")]
        public IActionResult Delete(int id)
        {
            try
            {
                var result = _subscriptionService.DeleteSubscription(id);
                if (!result)
                {
                    _logger.LogWarning("Attempted to delete non-existent or unauthorized subscription {Id}.", id);
                    TempData["Errors"] = _localizer["Global_ErrorMesage"].Value;
                }
                else
                {
                     TempData["SuccessMessage"] = _localizer["Subscription_DeletedMessage"].Value;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting subscription {Id}.", id);
                TempData["Errors"] = _localizer["Global_ErrorMesage"].Value;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [Route("/subscriptions/{id:int}/toggle-notifications")]
        public IActionResult ToggleNotifications(int id)
        {
            try
            {
                var result = _subscriptionService.ToggleNotifications(id);
                if (!result)
                {
                    _logger.LogWarning("Attempted to toggle notifications for non-existent or unauthorized subscription {Id}.", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling notifications for subscription {Id}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        [Route("/whats-new")]
        public async Task<IActionResult> WhatsNew(SubscriptionType? filter)
        {
            try
            {
                var items = await _subscriptionService.GetWhatsNewAsync(filter);
                return View(new WhatsNewViewModel
                {
                    Items = [.. items.Take(30)],
                    ActiveFilter = filter,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching What's New feed.");
                return View(new WhatsNewViewModel());
            }
        }

        [HttpGet]
        [Route("/subscriptions/{id:int}/releases")]
        public async Task<IActionResult> Releases(int id)
        {
            var subscription = _subscriptionService.GetSubscriptionById(id);

            if (subscription is null)
                return NotFound();

            ViewBag.SubscriptionName = subscription.Name;
            ViewBag.SubscriptionImage = subscription.ImageUrl;
            ViewBag.SubscriptionDescription = subscription.Description;

            try
            {
                switch ((SubscriptionType)subscription.Type)
                {
                    case SubscriptionType.SpotifyArtist:
                        var releases = await _spotifyService.GetArtistReleasesAsync(subscription.PlatformItemId);
                        return View("SpotifyArtistReleases", releases);
                    case SubscriptionType.SpotifyPodcasts:
                        var episodes = await _spotifyService.GetPodcastEpisodesAsync(subscription.PlatformItemId);
                        return View("SpotifyPodcastEpisodes", episodes);
                    case SubscriptionType.Youtube:
                        var videos = await _youTubeService.GetLatestVideosAsync(subscription.PlatformItemId);
                        return View("YouTubeVideos", videos);
                    case SubscriptionType.ApplePodcasts:
                        var appleEpisodes = await _applePodcastService.GetPodcastEpisodesAsync(subscription.PlatformItemId);
                        return View("ApplePodcastEpisodes", appleEpisodes);
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching releases for subscription {Id}.", id);
            }

            return BadRequest("Unknown subscription category.");
        }
    }
}
