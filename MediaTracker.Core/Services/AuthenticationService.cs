using System.Web;
using MediaTracker.Data;
using MediaTracker.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Localization;

namespace MediaTracker.Core.Services
{
    public interface IAuthenticationService
    {
        Task<SignInResult> SignIn(string? userName, string? password, bool rememberMe, bool lockoutOnFailure);

        Task SignOut();

        Task<IdentityResult> RegisterAsync(string? username, string? email, string? password, string? returnUrl);

        Task<IdentityResult> ConfirmEmailAsync(string token, string email, string returnUrl);

        bool SendResetPasswordEmail(string? email, string? returnUrl);

        Task<IdentityResult> ResetPassword(string email, string? token, string? newPassword);

        Task<IdentityResult> ConfirmPasswordAsync(string token, string email, string returnUrl);

        ApplicationUser CurrentUser();

        ApplicationUser? GetUserById(string userId);

        ApplicationUser FindUserByEmail(string? email);
    }

    public class AuthenticationService : IAuthenticationService
    {
        readonly UserManager<ApplicationUser> _userManager;
        readonly SignInManager<ApplicationUser> _signInManager;
        readonly IEmailService _emailService;
        readonly IUrlHelper _urlHelper;
        readonly IRepository _repository;
        readonly IStringLocalizer _localizer;
        readonly ApplicationUser? _currentUser;

        public AuthenticationService(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailService emailService,
            IHttpContextAccessor httpContextAccessor,
            IUrlHelper urlHelper,
            IRepository repository,
            IStringLocalizer localizer)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _urlHelper = urlHelper;
            _repository = repository;
            _localizer = localizer;

            if (httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false)
            {
                _currentUser = _userManager.FindByNameAsync(httpContextAccessor.HttpContext?.User?.Identity?.Name).Result;
            }
        }

        public async Task<SignInResult> SignIn(string? userName, string? password, bool rememberMe, bool lockoutOnFailure)
        {
            return await _signInManager.PasswordSignInAsync(userName, password, false, lockoutOnFailure: true);
        }

        public async Task SignOut()
        {
            await _signInManager.SignOutAsync();
        }

        public async Task<IdentityResult> RegisterAsync(string? userName, string? email, string? password, string? returnUrl)
        {
            var user = new ApplicationUser
            {
                UserName = userName,
                Email = email,
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                var url =
                    _urlHelper.Action(
                        new UrlActionContext
                        {
                            Action = "ConfirmEmail",
                            Controller = "Account",
                            Values = new
                            {
                                Token = _userManager.GenerateEmailConfirmationTokenAsync(user).Result,
                                user.Email,
                                ReturnUrl = returnUrl
                            },
                            Protocol = _urlHelper.ActionContext.HttpContext.Request.Scheme
                        });

                SendEmail(user, url, _localizer["Email_ConfirmEmailSubject"], _localizer["Email_ConfirmEmailBody"]);
            }

            return result;
        }

        public async Task<IdentityResult> ConfirmEmailAsync(string token, string email, string returnUrl)
        {
            var user = _repository.Set<ApplicationUser>().FirstOrDefault(x => x.Email == email);
            if (user != null)
            {
                var result = await _userManager.ConfirmEmailAsync(user, token);

                if (result.Succeeded)
                {
                    _repository.Update(user);
                }

                return result;
            }

            return null;
        }

        public bool SendResetPasswordEmail(string? email, string? returnUrl)
        {
            var user = FindUserByEmail(email);
            if (user != null)
            {
                var url =
                    _urlHelper.Action(
                        new UrlActionContext
                        {
                            Action = "ResetPassword",
                            Controller = "Account",
                            Values = new
                            {
                                Token = _userManager.GeneratePasswordResetTokenAsync(user).Result,
                                user.Email,
                                ReturnUrl = returnUrl
                            },
                            Protocol = _urlHelper.ActionContext.HttpContext.Request.Scheme
                        });

                SendEmail(user, url, _localizer["Email_ResetPasswordSubject"], _localizer["Email_ResetPasswordBody"]);

                return true;
            }

            return false;
        }

        public async Task<IdentityResult> ResetPassword(string email, string? token, string? newPassword)
        {
            var user
                = _repository.Set<ApplicationUser>().FirstOrDefault(x => x.Email == email);

            if (user != null)
            {
                return await _userManager.ResetPasswordAsync(user, token, newPassword);
            }

            return null;
        }

        public async Task<IdentityResult> ConfirmPasswordAsync(string token, string email, string returnUrl)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                return await _userManager.ConfirmEmailAsync(user, token);
            }

            return null;
        }

        public ApplicationUser CurrentUser()
        {
            return
                _currentUser;
        }

        public ApplicationUser? GetUserById(string userId)
        {
            return
                _repository.SetNoTracking<ApplicationUser>().FirstOrDefault(x => x.Id == userId);
        }

        public ApplicationUser FindUserByEmail(string? email)
        {
            return
                _repository.SetNoTracking<ApplicationUser>().FirstOrDefault(x => x.Email == email);
        }

        void SendEmail(ApplicationUser user, string url, string subject, string template)
        {
            var recievers = new List<string> { user.Email };
            var body = HttpUtility.HtmlEncode(string.Format(template, url));

            _emailService.SendEmail(recievers, subject, HttpUtility.HtmlDecode(body));
        }
    }
}