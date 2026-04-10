using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Shesha.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// An ordered phase or section inside an OnboardingPlan.
    /// Groups related OnboardingTasks together (e.g. "Week 1 — Getting Started").
    /// OrderIndex must be unique within its parent plan.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingModule")]
    public class OnboardingModule : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — parent plan.</summary>
        public virtual Guid OnboardingPlanId { get; set; }

        /// <summary>Navigation to parent plan.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>Display name of the module/phase.</summary>
        [Required, StringLength(200)]
        public virtual string Name { get; set; }

        /// <summary>Description of the module's objectives.</summary>
        [Required, StringLength(2000)]
        public virtual string Description { get; set; }

        /// <summary>Display and execution order within the plan. Must be unique per plan.</summary>
        public virtual int OrderIndex { get; set; }

        /// <summary>Template tasks contained in this module.</summary>
        public virtual ICollection<OnboardingTask> Tasks { get; set; }
    }
}
