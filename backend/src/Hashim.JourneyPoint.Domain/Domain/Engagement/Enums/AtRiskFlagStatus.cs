using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Lifecycle status of an AtRiskFlag raised against a hire's engagement record.</summary>
    [ReferenceList("JourneyPoint", "AtRiskFlagStatuses")]
    public enum AtRiskFlagStatus : long
    {
        /// <summary>Flag has been raised and is visible to Facilitators on the pipeline board.</summary>
        [Description("Active")]
        Active = 1,

        /// <summary>A Facilitator has acknowledged the flag and is taking intervention action.</summary>
        [Description("Acknowledged")]
        Acknowledged = 2,

        /// <summary>The flag has been resolved — hire is no longer at risk.</summary>
        [Description("Resolved")]
        Resolved = 3
    }
}
