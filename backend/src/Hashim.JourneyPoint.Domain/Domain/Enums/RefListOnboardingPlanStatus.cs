using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of an OnboardingPlan template.
    /// </summary>
    [ReferenceList("JourneyPoint", "OnboardingPlanStatuses")]
    public enum RefListOnboardingPlanStatus : long
    {
        /// <summary>Plan is being built and is not yet available for hire enrolment.</summary>
        [Description("Draft")]
        Draft = 1,

        /// <summary>Plan is live and can be assigned to new hires.</summary>
        [Description("Published")]
        Published = 2,

        /// <summary>Plan is retired and no longer assignable to new hires.</summary>
        [Description("Archived")]
        Archived = 3
    }
}
