using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Milestone period at which a wellness check-in is scheduled, relative to the hire's start date.
    /// Cadence: Day 1, Day 2, Week 1, then monthly for 6 months.
    /// </summary>
    [ReferenceList("JourneyPoint", "WellnessCheckInPeriods")]
    public enum WellnessCheckInPeriod : long
    {
        /// <summary>Check-in on Day 1 — first day pulse check.</summary>
        [Description("Day 1")]
        Day1 = 1,

        /// <summary>Check-in on Day 2 — second day follow-up.</summary>
        [Description("Day 2")]
        Day2 = 2,

        /// <summary>Check-in at end of Week 1.</summary>
        [Description("Week 1")]
        Week1 = 3,

        /// <summary>Check-in at end of Month 1.</summary>
        [Description("Month 1")]
        Month1 = 4,

        /// <summary>Check-in at end of Month 2.</summary>
        [Description("Month 2")]
        Month2 = 5,

        /// <summary>Check-in at end of Month 3.</summary>
        [Description("Month 3")]
        Month3 = 6,

        /// <summary>Check-in at end of Month 4.</summary>
        [Description("Month 4")]
        Month4 = 7,

        /// <summary>Check-in at end of Month 5.</summary>
        [Description("Month 5")]
        Month5 = 8,

        /// <summary>Check-in at end of Month 6.</summary>
        [Description("Month 6")]
        Month6 = 9
    }
}
