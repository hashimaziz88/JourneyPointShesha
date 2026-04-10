using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos
{
    /// <summary>Request model for previewing a markdown import before saving.</summary>
    public class MarkdownPreviewDto
    {
        /// <summary>Raw markdown content to parse and preview as an OnboardingPlan structure.</summary>
        [Required]
        public string MarkdownContent { get; set; }
    }
}
