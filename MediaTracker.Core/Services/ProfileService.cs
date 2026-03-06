using MediaTracker.Data.Entities;
using MediaTracker.Data.Repository;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace MediaTracker.Core.Services
{
    public interface IProfileService
    {
        ApplicationUser? GetProfile();

        bool UploadProfilePicture(IFormFile? file, bool isReset = false);

        bool UpdateUsername(string newUsername);
    }

    public class ProfileService(
        IRepository repository,
        IAuthenticationService authenticationService,
        IFileService fileService,
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager) : IProfileService
    {
        readonly IRepository _repository = repository;
        readonly IFileService _fileService = fileService;
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        readonly ApplicationUser _currentUser = authenticationService.CurrentUser();

        public ApplicationUser? GetProfile()
        {
            return _repository.SetNoTracking<ApplicationUser>().FirstOrDefault(x => x.Id == _currentUser.Id);
        }

        public bool UploadProfilePicture(IFormFile? file, bool isReset = false)
        {
            var user = _repository.Set<ApplicationUser>().FirstOrDefault(record => record.Id == _currentUser.Id);
            if (user == null)
            {
                return false;
            }

            if (file != null)
            {
                user.ProfilePicturePath = _fileService.UploadProfilePicture(file).Result;
            }
            if (isReset)
            {
                user.ProfilePicturePath = null;
            }

            _repository.Update(user);

            return true;
        }

        public bool UpdateUsername(string newUsername)
        {
            var user = _userManager.FindByIdAsync(_currentUser.Id).Result;
            if (user == null)
            {
                return false;
            }

            var result = _userManager.SetUserNameAsync(user, newUsername).Result;
            if (result.Succeeded)
            {
                _signInManager.RefreshSignInAsync(user).GetAwaiter().GetResult();
                return true;
            }

            return false;
        }
    }
}