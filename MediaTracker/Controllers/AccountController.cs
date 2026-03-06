using MediaTracker.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace MediaTracker.Controllers
{
    [AllowAnonymous]
    public class AccountController(
        Core.Services.IAuthenticationService authenticationService,
        IStringLocalizer localizer) : BaseController
    {
        readonly Core.Services.IAuthenticationService _authenticationService = authenticationService;
        readonly IStringLocalizer _localizer = localizer;

        [HttpGet]
        [Route("login")]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("/login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authenticationService.SignIn(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(model.ReturnUrl))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    return RedirectToAction(nameof(HomeController.Index), "Home");
                }

                HandleErrors(_localizer["Account_InvalidLogin"]);
            }

            return View(model);
        }

        [HttpGet]
        [Route("/register")]
        public IActionResult Register(string? returnUrl = null)
        {
            return View(new RegisterViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("/register")]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result =
                    await _authenticationService.RegisterAsync(
                        model.UserName,
                        model.Email,
                        model.Password,
                        model.ReturnUrl);

                if (result.Succeeded)
                {
                    return View("RegisterConfirmation", model.Email);
                }

                HandleErrors(result.Errors);
            }

            return View(model);
        }

        [HttpGet]
        [Route("/confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token, string email, string returnUrl)
        {
            var result = await _authenticationService.ConfirmEmailAsync(token, email, returnUrl);

            if (result != null && !result.Succeeded)
            {
                HandleErrors(result.Errors);
            }

            return View("EmailConfirmed", returnUrl);
        }

        [HttpGet]
        [Route("/forgot-password")]
        public IActionResult ForgotPassword(string? returnUrl = null)
        {
            return View(new ForgotPasswordViewModel { ReturnUrl = returnUrl });
        }

        [HttpPost]
        [Route("/forgot-password")]
        public IActionResult ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = _authenticationService.SendResetPasswordEmail(model.Email, model.ReturnUrl);
                if (result)
                {
                    return View("ForgotPasswordConfirmation", model.Email);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Route("/reset-password")]
        public IActionResult ResetPassword(string? token, string? email, string? returnUrl)
        {
            return View(new ResetPasswordViewModel
            {
                Token = token,
                Email = email,
                ReturnUrl = returnUrl
            });
        }

        [HttpPost]
        [Route("/reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await _authenticationService.ResetPassword(model.Email, model.Token, model.Password);
                if (result != null)
                {
                    if (result.Succeeded == true)
                    {
                        return View("ResetPasswordConfirmation", model.ReturnUrl);
                    }
                    else
                    {
                        HandleErrors(result.Errors);
                    }
                }
                else
                {
                    HandleErrors(_localizer["Global_ErrorMesage"]);
                }
            }

            return View(model);
        }

        [HttpGet]
        [Route("/logout")]
        public async Task<IActionResult> Logout()
        {
            await _authenticationService.SignOut();

            return RedirectToAction(nameof(Login));
        }
    }
}
