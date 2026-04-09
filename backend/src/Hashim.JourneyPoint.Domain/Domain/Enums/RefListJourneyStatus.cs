using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of a Journey (the hire's active onboarding instance).
    /// </summary>
    [ReferenceList("JourneyPoint", "JourneyStatuses")]
    public enum RefListJourneyStatus : long
    {
        /// <summary>Journey has been generated but not yet activated.</summary>
        [Description("Draft")]
        Draft = 1,

        /// <summary>Journey is active and the hire is working through their tasks.</summary>
        [Description("Active")]
        Active = 2,

        /// <summary>All journey tasks have been completed.</summary>
        [Description("Completed")]
        Completed = 3,

        /// <summary>Journey was cancelled before completion.</summary>
        [Description("Cancelled")]
        Cancelled = 4
    }
}
