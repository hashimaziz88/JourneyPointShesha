using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Completion status of an individual JourneyTask.
    /// </summary>
    [ReferenceList("JourneyPoint", "JourneyTaskStatuses")]
    public enum RefListJourneyTaskStatus : long
    {
        /// <summary>Task has not yet been completed.</summary>
        [Description("Pending")]
        Pending = 1,

        /// <summary>Task has been completed by the assignee.</summary>
        [Description("Complete")]
        Complete = 2
    }
}
