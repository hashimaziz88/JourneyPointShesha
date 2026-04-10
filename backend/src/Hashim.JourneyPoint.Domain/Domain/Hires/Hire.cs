using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Shesha.Domain.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.Hires
{
    /// <summary>
    /// A tenant-scoped hire enrolled against one onboarding plan.
    /// Created by the Facilitator when a new employee joins. The hire is enrolled in exactly
    /// one OnboardingPlan, which generates their Journey upon activation.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.Hire")]
    public class Hire : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — the onboarding plan this hire is enrolled against.</summary>
        public virtual Guid OnboardingPlanId { get; set; }

        /// <summary>Navigation to the plan.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>Linked platform user account (ABP UserId). Null until the user account is created.</summary>
        public virtual long? PlatformUserId { get; set; }

        /// <summary>Assigned manager user (ABP UserId). Optional.</summary>
        public virtual long? ManagerUserId { get; set; }

        /// <summary>Hire's full legal name.</summary>
        [Required, StringLength(200)]
        public virtual string FullName { get; set; }

        /// <summary>Hire's work email address. Used to create their platform user account.</summary>
        [Required, StringLength(256)]
        public virtual string EmailAddress { get; set; }

        /// <summary>The hire's job title within the organisation.</summary>
        [StringLength(200)]
        public virtual string RoleTitle { get; set; }

        /// <summary>Department or directorate the hire is joining.</summary>
        [StringLength(200)]
        public virtual string Department { get; set; }

        /// <summary>Official first day of employment. Anchor date for all journey task due dates.</summary>
        public virtual DateTime StartDate { get; set; }

        /// <summary>Current lifecycle state of the hire record.</summary>
        [ReferenceList("JourneyPoint", "HireLifecycleStates")]
        public virtual HireLifecycleState Status { get; set; }

        /// <summary>Delivery state of the welcome onboarding notification email.</summary>
        [ReferenceList("JourneyPoint", "WelcomeNotificationStatuses")]
        public virtual WelcomeNotificationStatus WelcomeNotificationStatus { get; set; }

        /// <summary>Timestamp of the last welcome notification delivery attempt.</summary>
        public virtual DateTime? WelcomeNotificationLastAttemptedAt { get; set; }

        /// <summary>Timestamp of the successful welcome notification send.</summary>
        public virtual DateTime? WelcomeNotificationSentAt { get; set; }

        /// <summary>Failure reason if the welcome notification could not be delivered.</summary>
        [StringLength(500)]
        public virtual string WelcomeNotificationFailureReason { get; set; }

        /// <summary>When the hire's journey was activated by the Facilitator.</summary>
        public virtual DateTime? ActivatedAt { get; set; }

        /// <summary>When the hire completed all onboarding tasks.</summary>
        public virtual DateTime? CompletedAt { get; set; }

        /// <summary>When the hire exited the organisation before completing onboarding.</summary>
        public virtual DateTime? ExitedAt { get; set; }

        /// <summary>The hire's journey (1-to-1). Null until the journey is generated.</summary>
        public virtual Journey Journey { get; set; }
    }
}
