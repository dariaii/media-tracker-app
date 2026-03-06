using System.ComponentModel.DataAnnotations;

namespace MediaTracker.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Message_RequiredField")]
        public string? UserName { get; set; }

        [Required(ErrorMessage = "Message_RequiredField")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }

        public string? ReturnUrl { get; set; }
    }
}
