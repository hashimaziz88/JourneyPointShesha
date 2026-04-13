using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A reusable, tenant-owned template for onboarding a specific type of hire.
    /// Contains ordered modules and tasks. Changes to a Published plan do not affect
    /// journeys already generated from it.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingPlan")]
    public class OnboardingPlan : FullAuditedEntity<Guid>
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>Display name of the onboarding plan.</summary>
        [Required, StringLength(200)]
        public virtual string Name { get; set; }

        /// <summary>Description of the plan's purpose and scope.</summary>
        [Required, StringLength(4000)]
        public virtual string Description { get; set; }

        /// <summary>The hire profile this plan is designed for (e.g. "Software Engineers").</summary>
        [Required, StringLength(200)]
        public virtual string TargetAudience { get; set; }

        /// <summary>Expected onboarding duration in calendar days. Must be at least 1.</summary>
        public virtual int DurationDays { get; set; }

        /// <summary>Current lifecycle status of the plan.</summary>
        [ReferenceList("JourneyPoint", "OnboardingPlanStatuses")]
        public virtual OnboardingPlanStatus Status { get; set; }

        /// <summary>Ordered phases/modules that make up this plan.</summary>
        public virtual ICollection<OnboardingModule> Modules { get; set; }

        /// <summary>Uploaded documents attached to this plan for AI task extraction.</summary>
        public virtual ICollection<OnboardingDocument> Documents { get; set; }
    }
}
