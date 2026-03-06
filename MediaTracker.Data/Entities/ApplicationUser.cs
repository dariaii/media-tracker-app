using Microsoft.AspNetCore.Identity;

namespace MediaTracker.Data.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfilePicturePath { get; set; }
    }
}
