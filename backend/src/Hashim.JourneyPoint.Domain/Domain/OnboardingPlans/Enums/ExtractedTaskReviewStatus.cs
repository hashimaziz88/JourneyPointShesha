using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Facilitator review status of an AI-extracted task proposal.</summary>
    [ReferenceList("JourneyPoint", "ExtractedTaskReviewStatuses")]
    public enum ExtractedTaskReviewStatus : long
    {
        /// <summary>Proposal is awaiting Facilitator review.</summary>
        [Description("Pending")]
        Pending = 1,

        /// <summary>Facilitator has accepted the proposal — it will be applied to the plan.</summary>
        [Description("Accepted")]
        Accepted = 2,

        /// <summary>Facilitator has rejected the proposal — it will not be added to the plan.</summary>
        [Description("Rejected")]
        Rejected = 3,

        /// <summary>Proposal has been converted into an OnboardingTask via ApplyAcceptedProposals.</summary>
        [Description("Applied")]
        Applied = 4
    }
}
