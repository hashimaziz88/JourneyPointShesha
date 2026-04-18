using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Lifecycle state of a hire's onboarding Journey.</summary>
    [ReferenceList("JourneyPoint", "JourneyStatuses")]
    public enum JourneyStatus : long
    {
        /// <summary>Journey has been generated but not yet activated by the Facilitator.</summary>
        [Description("Draft")]
        Draft = 1,

        /// <summary>Journey is live — the hire is actively working through their tasks.</summary>
        [Description("Active")]
        Active = 2,

        /// <summary>Journey has been temporarily paused by the Facilitator.</summary>
        [Description("Paused")]
        Paused = 3,

        /// <summary>All journey tasks have been completed.</summary>
        [Description("Completed")]
        Completed = 4
    }
}
