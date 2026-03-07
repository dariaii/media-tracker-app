using MediaTracker.Core.Services;
using MediaTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaTracker.Controllers
{
    [Authorize]
    public class HomeController(
        ILogger<HomeController> logger,
        ISubscriptionService subscriptionService) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ISubscriptionService _subscriptionService = subscriptionService;

        [HttpGet]
        [Route("/")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var latestItems = await _subscriptionService.GetLatestPerSubscriptionAsync();
                var subscriptions = _subscriptionService.GetSubscriptions();
                
                return View(new HomeViewModel
                {
                    LatestItems = latestItems,
                    Subscriptions = subscriptions,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading home page.");
                return View(new HomeViewModel());
            }
        }
    }
}
