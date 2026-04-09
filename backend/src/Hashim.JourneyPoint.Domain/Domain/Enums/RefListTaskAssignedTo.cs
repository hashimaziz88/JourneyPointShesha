using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>
    /// Indicates which role is responsible for completing an onboarding task.
    /// </summary>
    [ReferenceList("JourneyPoint", "TaskAssignedTo")]
    public enum RefListTaskAssignedTo : long
    {
        /// <summary>Task is assigned to the hire (enrolee).</summary>
        [Description("Enrolee")]
        Enrolee = 1,

        /// <summary>Task is assigned to the hire's line manager.</summary>
        [Description("Manager")]
        Manager = 2,

        /// <summary>Task is assigned to the HR facilitator managing the onboarding.</summary>
        [Description("Facilitator")]
        Facilitator = 3
    }
}
