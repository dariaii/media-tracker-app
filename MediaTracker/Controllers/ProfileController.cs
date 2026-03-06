using MediaTracker.Core.Services;
using MediaTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MediaTracker.Controllers
{
    [Authorize]
    public class ProfileController(IStringLocalizer localizer, IProfileService profileService) : BaseController
    {
        private readonly IStringLocalizer _localizer = localizer;
        private readonly IProfileService _profileService = profileService;

        [HttpGet]
        [Route("/profile")]
        public IActionResult Index()
        {
            var profile = _profileService.GetProfile();
            if (profile == null)
            {
                return NotFound();
            }

            var viewModel = new ProfileViewModel
            {
                UserName = profile.UserName,
                Email = profile.Email,
                ProfilePicturePath = profile.ProfilePicturePath
            };

            return View(viewModel);
        }

        [HttpPost]
        [Route("/profile/update-avatar")]
        public IActionResult UpdateAvatar([FromForm] IFormFile file)
        {
            if (file != null && _profileService.UploadProfilePicture(file))
            {
                return RedirectToAction("Index");
            }

            TempData["Errors"] = _localizer["Global_ErrorMesage"].ToString();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/profile/reset-avatar")]
        public IActionResult ResetAvatar()
        {
            if (_profileService.UploadProfilePicture(null, true))
            {
                return RedirectToAction("Index");
            }

            TempData["Errors"] = _localizer["Global_ErrorMesage"].ToString();

            return RedirectToAction("Index");
        }

        [HttpPost]
        [Route("/profile/update-username")]
        public IActionResult UpdateUsername(ProfileViewModel model)
        {
            if (!string.IsNullOrWhiteSpace(model.UserName) && _profileService.UpdateUsername(model.UserName))
            {
                return RedirectToAction("Index");
            }

            TempData["Errors"] = _localizer["Global_ErrorMesage"].ToString();

            return RedirectToAction("Index");
        }
    }
}
