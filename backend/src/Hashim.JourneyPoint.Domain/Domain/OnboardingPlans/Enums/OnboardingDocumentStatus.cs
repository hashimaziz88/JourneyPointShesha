using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Processing status of an uploaded OnboardingDocument.</summary>
    [ReferenceList("JourneyPoint", "OnboardingDocumentStatuses")]
    public enum OnboardingDocumentStatus : long
    {
        /// <summary>Document has been uploaded but AI extraction has not started.</summary>
        [Description("Uploaded")]
        Uploaded = 1,

        /// <summary>AI extraction is currently in progress.</summary>
        [Description("Extracting")]
        Extracting = 2,

        /// <summary>Extraction completed — proposals are ready for Facilitator review.</summary>
        [Description("Ready For Review")]
        ReadyForReview = 3,

        /// <summary>All accepted proposals have been applied to the plan as OnboardingTasks.</summary>
        [Description("Applied")]
        Applied = 4,

        /// <summary>AI extraction failed. See FailureReason for details.</summary>
        [Description("Failed")]
        Failed = 5
    }
}
