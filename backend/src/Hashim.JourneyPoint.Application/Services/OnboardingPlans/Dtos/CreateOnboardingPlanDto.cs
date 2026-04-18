using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos
{
    /// <summary>Request model for creating a new OnboardingPlan template.</summary>
    public class CreateOnboardingPlanDto
    {
        [Required]
        public string Name { get; set; }

        public string Description { get; set; }

        [Required]
        public string TargetAudience { get; set; }

        [Required]
        public int DurationDays { get; set; }
    }
}
