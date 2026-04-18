using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Delivery state of the welcome onboarding notification sent to a hire.</summary>
    [ReferenceList("JourneyPoint", "WelcomeNotificationStatuses")]
    public enum WelcomeNotificationStatus : long
    {
        /// <summary>Notification send is queued or has not yet been attempted.</summary>
        [Description("Pending")]
        Pending = 0,

        /// <summary>Notification was delivered successfully.</summary>
        [Description("Sent")]
        Sent = 1,

        /// <summary>Notification delivery failed but may succeed on retry.</summary>
        [Description("Failed (Recoverable)")]
        FailedRecoverable = 2
    }
}
