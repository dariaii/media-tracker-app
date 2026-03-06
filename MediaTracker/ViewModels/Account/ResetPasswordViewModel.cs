using System.ComponentModel.DataAnnotations;

namespace MediaTracker.ViewModels
{
    public class ResetPasswordViewModel
    {
        [Required(ErrorMessage = "Message_RequiredField")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Message_RequiredField")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Message_PasswordsDontMatch")]
        public string? ConfirmPassword { get; set; }

        public string? Token { get; set; }

        public string? Email { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
