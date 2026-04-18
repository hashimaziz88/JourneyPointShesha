using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Defines whether a hire must explicitly acknowledge a task before marking it complete.
    /// Controls the acknowledgement gate in the enrolee portal.
    /// </summary>
    [ReferenceList("JourneyPoint", "OnboardingTaskAcknowledgementRules")]
    public enum OnboardingTaskAcknowledgementRule : long
    {
        /// <summary>No acknowledgement required — the hire can complete the task directly.</summary>
        [Description("Not Required")]
        NotRequired = 1,

        /// <summary>The hire must acknowledge (read and confirm) the task before they can mark it complete.</summary>
        [Description("Required")]
        Required = 2
    }
}
