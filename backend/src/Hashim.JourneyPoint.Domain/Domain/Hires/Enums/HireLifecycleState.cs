using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Lifecycle state of a Hire record during the onboarding process.</summary>
    [ReferenceList("JourneyPoint", "HireLifecycleStates")]
    public enum HireLifecycleState : long
    {
        /// <summary>Hire has been created but their journey has not yet been activated.</summary>
        [Description("Pending Activation")]
        PendingActivation = 1,

        /// <summary>Hire's journey is active and onboarding is in progress.</summary>
        [Description("Active")]
        Active = 2,

        /// <summary>Hire has completed all onboarding tasks.</summary>
        [Description("Completed")]
        Completed = 3,

        /// <summary>Hire has exited the organisation before completing onboarding.</summary>
        [Description("Exited")]
        Exited = 4
    }
}
