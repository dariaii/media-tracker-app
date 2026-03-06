using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;

namespace MediaTracker.Core.Infrastructure.Localization
{
    public class MultilanguageIdentityErrorDescriber(IStringLocalizer localizer) : IdentityErrorDescriber
    {
        private readonly IStringLocalizer _localizer = localizer;

        public override IdentityError DefaultError() { return new IdentityError { Code = nameof(DefaultError), Description = _localizer["Identity_DefaultError"] }; }
        public override IdentityError ConcurrencyFailure() { return new IdentityError { Code = nameof(ConcurrencyFailure), Description = _localizer["Identity_ConcurrencyFailure"] }; }
        public override IdentityError PasswordMismatch() { return new IdentityError { Code = nameof(PasswordMismatch), Description = _localizer["Identity_PasswordMismatch"] }; }
        public override IdentityError InvalidToken() { return new IdentityError { Code = nameof(InvalidToken), Description = _localizer["Identity_InvalidToken"] }; }
        public override IdentityError LoginAlreadyAssociated() { return new IdentityError { Code = nameof(LoginAlreadyAssociated), Description = _localizer["Identity_LoginAlreadyAssociated"] }; }
        public override IdentityError InvalidUserName(string userName) { return new IdentityError { Code = nameof(InvalidUserName), Description = string.Format(_localizer["Identity_InvalidUserName"], userName) }; }
        public override IdentityError InvalidEmail(string email) { return new IdentityError { Code = nameof(InvalidEmail), Description = string.Format(_localizer["Identity_InvalidEmail"], email) }; }
        public override IdentityError DuplicateUserName(string userName) { return new IdentityError { Code = nameof(DuplicateUserName), Description = string.Format(_localizer["Identity_DuplicateUserName"], userName) }; }
        public override IdentityError DuplicateEmail(string email) { return new IdentityError { Code = nameof(DuplicateEmail), Description = string.Format(_localizer["Identity_DuplicateEmail"], email) }; }
        public override IdentityError InvalidRoleName(string role) { return new IdentityError { Code = nameof(InvalidRoleName), Description = string.Format(_localizer["Identity_InvalidRoleName"], role) }; }
        public override IdentityError DuplicateRoleName(string role) { return new IdentityError { Code = nameof(DuplicateRoleName), Description = string.Format(_localizer["Identity_DuplicateRoleName"], role) }; }
        public override IdentityError UserAlreadyHasPassword() { return new IdentityError { Code = nameof(UserAlreadyHasPassword), Description = _localizer["Identity_UserAlreadyHasPassword"] }; }
        public override IdentityError UserLockoutNotEnabled() { return new IdentityError { Code = nameof(UserLockoutNotEnabled), Description = _localizer["Identity_UserLockoutNotEnabled"] }; }
        public override IdentityError UserAlreadyInRole(string role) { return new IdentityError { Code = nameof(UserAlreadyInRole), Description = string.Format(_localizer["Identity_UserAlreadyInRole"], role) }; }
        public override IdentityError UserNotInRole(string role) { return new IdentityError { Code = nameof(UserNotInRole), Description = string.Format(_localizer["Identity_UserNotInRole"], role) }; }
        public override IdentityError PasswordTooShort(int length) { return new IdentityError { Code = nameof(PasswordTooShort), Description = string.Format(_localizer["Identity_PasswordTooShort"], length) }; }
        public override IdentityError PasswordRequiresNonAlphanumeric() { return new IdentityError { Code = nameof(PasswordRequiresNonAlphanumeric), Description = _localizer["Identity_PasswordRequiresNonAlphanumeric"] }; }
        public override IdentityError PasswordRequiresDigit() { return new IdentityError { Code = nameof(PasswordRequiresDigit), Description = _localizer["Identity_PasswordRequiresDigit"] }; }
        public override IdentityError PasswordRequiresLower() { return new IdentityError { Code = nameof(PasswordRequiresLower), Description = _localizer["Identity_PasswordRequiresLower"] }; }
        public override IdentityError PasswordRequiresUpper() { return new IdentityError { Code = nameof(PasswordRequiresUpper), Description = _localizer["Identity_PasswordRequiresUpper"] }; }
    }
}
