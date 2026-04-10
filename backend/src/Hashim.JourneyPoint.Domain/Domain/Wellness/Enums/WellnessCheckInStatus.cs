using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Completion status of a scheduled WellnessCheckIn.</summary>
    [ReferenceList("JourneyPoint", "WellnessCheckInStatuses")]
    public enum WellnessCheckInStatus : long
    {
        /// <summary>Check-in has been scheduled but not yet started by the hire.</summary>
        [Description("Pending")]
        Pending = 1,

        /// <summary>Hire has started answering questions but has not yet submitted.</summary>
        [Description("In Progress")]
        InProgress = 2,

        /// <summary>Hire has submitted all answers for this check-in.</summary>
        [Description("Completed")]
        Completed = 3
    }
}
