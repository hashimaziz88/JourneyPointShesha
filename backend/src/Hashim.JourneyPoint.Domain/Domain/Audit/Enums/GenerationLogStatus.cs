using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Execution status of an AI generation workflow recorded in a GenerationLog.</summary>
    [ReferenceList("JourneyPoint", "GenerationLogStatuses")]
    public enum GenerationLogStatus : long
    {
        /// <summary>Workflow completed successfully.</summary>
        [Description("Succeeded")]
        Succeeded = 1,

        /// <summary>Workflow failed. See FailureReason for details.</summary>
        [Description("Failed")]
        Failed = 2
    }
}
