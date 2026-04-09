using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of an AtRiskFlag raised against a hire.
    /// </summary>
    [ReferenceList("JourneyPoint", "AtRiskFlagStatuses")]
    public enum RefListAtRiskFlagStatus : long
    {
        /// <summary>Flag has been raised and not yet seen by the facilitator.</summary>
        [Description("Active")]
        Active = 1,

        /// <summary>Facilitator has acknowledged the flag and is taking action.</summary>
        [Description("Acknowledged")]
        Acknowledged = 2,

        /// <summary>Facilitator has resolved the flag; hire is no longer at risk.</summary>
        [Description("Resolved")]
        Resolved = 3
    }
}
