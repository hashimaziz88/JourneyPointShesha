using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Completion state of a single JourneyTask.</summary>
    [ReferenceList("JourneyPoint", "JourneyTaskStatuses")]
    public enum JourneyTaskStatus : long
    {
        /// <summary>Task has been assigned but not yet completed by the assignee.</summary>
        [Description("Pending")]
        Pending = 1,

        /// <summary>Task has been completed by the assignee.</summary>
        [Description("Completed")]
        Completed = 2
    }
}
