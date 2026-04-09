using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Review status of an AI-extracted task proposal from an OnboardingDocument.
    /// </summary>
    [ReferenceList("JourneyPoint", "ExtractedTaskStatuses")]
    public enum RefListExtractedTaskStatus : long
    {
        /// <summary>Proposal is awaiting facilitator review.</summary>
        [Description("Pending Review")]
        PendingReview = 1,

        /// <summary>Proposal has been accepted and will be applied to the plan.</summary>
        [Description("Accepted")]
        Accepted = 2,

        /// <summary>Proposal has been rejected and will not be added to the plan.</summary>
        [Description("Rejected")]
        Rejected = 3
    }
}
