using MediaTracker.Core.Infrastructure;
using System.ComponentModel.DataAnnotations;

namespace MediaTracker.ViewModels
{
    public class CreateSubscriptionViewModel
    {
        [Required(ErrorMessage = "Message_RequiredField")]
        public SubscriptionType Type { get; set; }

        [Required(ErrorMessage = "Message_RequiredField")]
        public string PlatformItemId { get; set; } = string.Empty;

        public bool ReceiveNotifications { get; set; } = true;

        public string? ReturnUrl { get; set; }
    }
}
