using Abp.Domain.Entities.Auditing;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A phase or section within an OnboardingPlan. Groups related OnboardingTasks together
    /// (e.g. "Week 1 — Getting Started", "Month 1 — Technical Ramp-Up").
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingModule")]
    public class OnboardingModule : FullAuditedEntity<Guid>
    {
        /// <summary>The plan this module belongs to.</summary>
        public virtual OnboardingPlan Plan { get; set; }

        /// <summary>Display name of the module/phase.</summary>
        public virtual string Name { get; set; }

        /// <summary>Optional description of the module's objectives.</summary>
        public virtual string Description { get; set; }

        /// <summary>Display order of this module within the plan.</summary>
        public virtual int SortOrder { get; set; }

        /// <summary>Expected duration of this phase in calendar days.</summary>
        public virtual int DurationDays { get; set; }
    }
}
