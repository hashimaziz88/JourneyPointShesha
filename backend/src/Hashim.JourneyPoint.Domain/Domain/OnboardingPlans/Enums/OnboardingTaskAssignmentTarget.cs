using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Indicates which role is responsible for completing an onboarding task.</summary>
    [ReferenceList("JourneyPoint", "OnboardingTaskAssignmentTargets")]
    public enum OnboardingTaskAssignmentTarget : long
    {
        /// <summary>Task is assigned to the hire (enrolee) to complete.</summary>
        [Description("Enrolee")]
        Enrolee = 1,

        /// <summary>Task is assigned to the hire's line manager to complete.</summary>
        [Description("Manager")]
        Manager = 2,

        /// <summary>Task is assigned to the HR Facilitator managing the onboarding.</summary>
        [Description("Facilitator")]
        Facilitator = 3
    }
}
