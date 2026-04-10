using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos
{
    /// <summary>Request model for saving a parsed markdown structure as a Draft OnboardingPlan.</summary>
    public class SaveDraftFromMarkdownDto
    {
        /// <summary>Raw markdown content to parse and persist as a Draft plan.</summary>
        [Required]
        public string MarkdownContent { get; set; }

        /// <summary>Display name for the new plan. Defaults to the first heading in the markdown if not supplied.</summary>
        public string PlanName { get; set; }
    }
}
