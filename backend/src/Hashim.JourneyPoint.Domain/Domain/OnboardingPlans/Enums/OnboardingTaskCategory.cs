using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Functional category of an onboarding task, used for grouping and reporting.</summary>
    [ReferenceList("JourneyPoint", "OnboardingTaskCategories")]
    public enum OnboardingTaskCategory : long
    {
        /// <summary>Initial orientation activities — introductions, culture, and context-setting.</summary>
        [Description("Orientation")]
        Orientation = 1,

        /// <summary>Learning and training activities — reading, courses, or knowledge transfer.</summary>
        [Description("Learning")]
        Learning = 2,

        /// <summary>Hands-on practice and applied work activities.</summary>
        [Description("Practice")]
        Practice = 3,

        /// <summary>Formal assessments or evaluations to verify competency.</summary>
        [Description("Assessment")]
        Assessment = 4,

        /// <summary>Scheduled check-in meetings with manager or Facilitator.</summary>
        [Description("Check-In")]
        CheckIn = 5
    }
}
