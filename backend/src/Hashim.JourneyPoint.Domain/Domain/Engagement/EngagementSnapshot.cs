using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.Engagement
{
    /// <summary>
    /// An append-only engagement score record computed for a hire's journey at a point in time.
    /// Never updated after creation — score history accumulates over the life of the journey.
    /// Computed by EngagementScoreService on a schedule and on significant task events.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.EngagementSnapshot")]
    public class EngagementSnapshot : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — hire being scored.</summary>
        public virtual Guid HireId { get; set; }

        /// <summary>Navigation to hire.</summary>
        public virtual Hire Hire { get; set; }

        /// <summary>FK — journey being scored.</summary>
        public virtual Guid JourneyId { get; set; }

        /// <summary>Navigation to journey.</summary>
        public virtual Journey Journey { get; set; }

        /// <summary>Percentage of tasks completed out of all tasks due at computation time (0–100).</summary>
        public virtual decimal CompletionRate { get; set; }

        /// <summary>Calendar days since the hire last completed any task.</summary>
        public virtual int DaysSinceLastActivity { get; set; }

        /// <summary>Count of tasks that are past their due date and still Pending.</summary>
        public virtual int OverdueTaskCount { get; set; }

        /// <summary>Weighted composite engagement score (0–100).</summary>
        public virtual decimal CompositeScore { get; set; }

        /// <summary>Engagement classification derived from CompositeScore.</summary>
        [ReferenceList("JourneyPoint", "EngagementClassifications")]
        public virtual EngagementClassification Classification { get; set; }

        /// <summary>UTC timestamp when this snapshot was computed.</summary>
        public virtual DateTime ComputedAt { get; set; }
    }
}
