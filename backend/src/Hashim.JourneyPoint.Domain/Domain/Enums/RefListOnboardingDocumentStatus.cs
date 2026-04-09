using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Processing status of an uploaded OnboardingDocument.
    /// </summary>
    [ReferenceList("JourneyPoint", "OnboardingDocumentStatuses")]
    public enum RefListOnboardingDocumentStatus : long
    {
        /// <summary>Document has been uploaded but AI extraction has not started.</summary>
        [Description("Uploaded")]
        Uploaded = 1,

        /// <summary>AI extraction is currently in progress.</summary>
        [Description("Processing")]
        Processing = 2,

        /// <summary>AI extraction completed successfully and proposals are ready for review.</summary>
        [Description("Extracted")]
        Extracted = 3,

        /// <summary>AI extraction failed. Document may need to be re-uploaded or manually processed.</summary>
        [Description("Failed")]
        Failed = 4,

        /// <summary>Accepted proposals have been applied to the plan as OnboardingTasks.</summary>
        [Description("Applied")]
        Applied = 5
    }
}
