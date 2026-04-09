using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Lifecycle status of a Hire record.
    /// </summary>
    [ReferenceList("JourneyPoint", "HireStatuses")]
    public enum RefListHireStatus : long
    {
        /// <summary>Hire is active and currently being onboarded.</summary>
        [Description("Active")]
        Active = 1,

        /// <summary>Hire's onboarding journey has been completed.</summary>
        [Description("Completed")]
        Completed = 2,

        /// <summary>Hire record has been cancelled or withdrawn.</summary>
        [Description("Cancelled")]
        Cancelled = 3
    }
}
