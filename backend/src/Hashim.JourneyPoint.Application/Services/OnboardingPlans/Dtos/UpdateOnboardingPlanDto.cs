using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos
{
    /// <summary>Request model for updating an existing OnboardingPlan template.</summary>
    public class UpdateOnboardingPlanDto
    {
        [Required]
        public Guid Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string TargetAudience { get; set; }
        public int? DurationDays { get; set; }
    }
}
