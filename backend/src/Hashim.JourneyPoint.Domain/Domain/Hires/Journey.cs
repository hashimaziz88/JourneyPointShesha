using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Shesha.Domain.Attributes;
using System;
using System.Collections.Generic;

namespace Hashim.JourneyPoint.Domain.Domain.Hires
{
    /// <summary>
    /// The hire-specific copy of a published onboarding plan, representing the live onboarding instance.
    /// Generated from an OnboardingPlan when the Facilitator activates the hire.
    /// Changes to the source OnboardingPlan after generation do not affect this Journey.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.Journey")]
    public class Journey : FullAuditedEntity<Guid>
    {
        /// <summary>FK — the hire this journey belongs to.</summary>
        public virtual Guid HireId { get; set; }

        /// <summary>Navigation to hire.</summary>
        public virtual Hire Hire { get; set; }

        /// <summary>FK — the source onboarding plan this journey was generated from.</summary>
        public virtual Guid OnboardingPlanId { get; set; }

        /// <summary>Navigation to source plan.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>Current lifecycle state of the journey.</summary>
        [ReferenceList("JourneyPoint", "JourneyStatuses")]
        public virtual JourneyStatus Status { get; set; }

        /// <summary>When the journey was activated by the Facilitator.</summary>
        public virtual DateTime? ActivatedAt { get; set; }

        /// <summary>When the journey was paused by the Facilitator.</summary>
        public virtual DateTime? PausedAt { get; set; }

        /// <summary>When the journey was completed (all tasks done).</summary>
        public virtual DateTime? CompletedAt { get; set; }

        /// <summary>All tasks in this journey (copied from OnboardingTask at generation time).</summary>
        public virtual ICollection<JourneyTask> Tasks { get; set; }
    }
}
