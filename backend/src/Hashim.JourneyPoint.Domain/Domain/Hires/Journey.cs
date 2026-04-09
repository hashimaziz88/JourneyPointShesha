using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.Hires
{
    /// <summary>
    /// The live onboarding instance for a specific hire.
    /// Generated from an OnboardingPlan at enrolment time. Each hire has at most one active Journey.
    /// Changes to the source OnboardingPlan do not affect a Journey that has already been generated.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.Journey")]
    public class Journey : FullAuditedEntity<Guid>
    {
        /// <summary>The hire this journey belongs to.</summary>
        public virtual Hire Hire { get; set; }

        /// <summary>Snapshot reference to the plan this journey was generated from.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>Current lifecycle status of the journey.</summary>
        [ReferenceList("JourneyPoint", "JourneyStatuses")]
        public virtual RefListJourneyStatus? Status { get; set; }

        /// <summary>Date and time the journey was activated by the Facilitator.</summary>
        public virtual DateTime? ActivatedAt { get; set; }

        /// <summary>Date and time the journey was completed (all tasks done).</summary>
        public virtual DateTime? CompletedAt { get; set; }

        /// <summary>
        /// AI-generated personalisation notes produced by GroqPersonalisationService.
        /// Stored as context for how tasks were personalised for this hire.
        /// </summary>
        public virtual string PersonalisationNotes { get; set; }
    }
}
