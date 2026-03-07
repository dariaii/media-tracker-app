using MediaTracker.Core.Services;
using MediaTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediaTracker.Controllers
{
    [Authorize]
    public class ExploreController(
        ILogger<ExploreController> logger,
        IExploreService exploreService) : Controller
    {
        private readonly ILogger<ExploreController> _logger = logger;
        private readonly IExploreService _exploreService = exploreService;

        [HttpGet]
        [Route("/explore")]
        public async Task<IActionResult> Index([FromQuery] string? q)
        {
            var model = new ExploreViewModel { Query = q };

            if (string.IsNullOrWhiteSpace(q))
                return View(model);

            try
            {
                model.Results = await _exploreService.SearchAsync(q.Trim());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during explore search for '{Query}'", q);
            }

            return View(model);
        }
    }
}