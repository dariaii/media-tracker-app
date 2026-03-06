using System.ComponentModel.DataAnnotations;

namespace MediaTracker.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Message_RequiredField")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Message_RequiredField")]
        [EmailAddress(ErrorMessage = "Message_InvalidEmail")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Message_RequiredField")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Message_RequiredField")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Message_PasswordsDontMatch")]
        public string? ConfirmPassword { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
