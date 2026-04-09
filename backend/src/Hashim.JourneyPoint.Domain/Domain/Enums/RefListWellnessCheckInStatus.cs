using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Completion status of a scheduled WellnessCheckIn.
    /// </summary>
    [ReferenceList("JourneyPoint", "WellnessCheckInStatuses")]
    public enum RefListWellnessCheckInStatus : long
    {
        /// <summary>Check-in has been scheduled but the hire has not yet submitted answers.</summary>
        [Description("Pending")]
        Pending = 1,

        /// <summary>Hire has submitted all answers for this check-in.</summary>
        [Description("Submitted")]
        Submitted = 2
    }
}
