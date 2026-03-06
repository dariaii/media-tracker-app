using System.ComponentModel.DataAnnotations;

namespace MediaTracker.ViewModels
{
    public class ProfileViewModel
    {
        [Required(ErrorMessage = "Message_RequiredField")]
        public string? UserName { get; set; }

        public string? Email { get; set; }

        public string? ProfilePicturePath { get; set; }

        public IFormFile? ProfilePictureFile { get; set; }
    }
}
