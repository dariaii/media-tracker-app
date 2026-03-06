using MediaTracker.Core.Models;

namespace MediaTracker.ViewModels
{
    public class ExploreViewModel
    {
        public string? Query { get; set; }

        public List<ExploreResult> Results { get; set; } = [];
    }
}