using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Shesha.Domain.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.Engagement
{
    /// <summary>
    /// A durable at-risk intervention record raised when a hire's engagement drops below threshold.
    /// At most one Active flag per hire at any time. Facilitators acknowledge and resolve flags
    /// via the pipeline board. Resolved flags are retained for audit history.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.AtRiskFlag")]
    public class AtRiskFlag : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — hire the flag is raised for.</summary>
        public virtual Guid HireId { get; set; }

        /// <summary>Navigation to hire.</summary>
        public virtual Hire Hire { get; set; }

        /// <summary>FK — journey the flag is raised against.</summary>
        public virtual Guid JourneyId { get; set; }

        /// <summary>Navigation to journey.</summary>
        public virtual Journey Journey { get; set; }

        /// <summary>When the flag was raised by the engagement scoring engine.</summary>
        public virtual DateTime RaisedAt { get; set; }

        /// <summary>The engagement classification that triggered this flag.</summary>
        [ReferenceList("JourneyPoint", "EngagementClassifications")]
        public virtual EngagementClassification ClassificationAtRaise { get; set; }

        /// <summary>Current lifecycle status of the flag.</summary>
        [ReferenceList("JourneyPoint", "AtRiskFlagStatuses")]
        public virtual AtRiskFlagStatus Status { get; set; }

        /// <summary>ABP UserId of the Facilitator who acknowledged the flag.</summary>
        public virtual long? AcknowledgedByUserId { get; set; }

        /// <summary>When the Facilitator acknowledged the flag.</summary>
        public virtual DateTime? AcknowledgedAt { get; set; }

        /// <summary>Facilitator notes recorded when acknowledging (e.g. planned intervention).</summary>
        [StringLength(2000)]
        public virtual string AcknowledgementNotes { get; set; }

        /// <summary>ABP UserId of the Facilitator who resolved the flag.</summary>
        public virtual long? ResolvedByUserId { get; set; }

        /// <summary>When the flag was resolved.</summary>
        public virtual DateTime? ResolvedAt { get; set; }

        /// <summary>How the at-risk situation was resolved. Null until resolved.</summary>
        [ReferenceList("JourneyPoint", "AtRiskResolutionTypes")]
        public virtual AtRiskResolutionType? ResolutionType { get; set; }

        /// <summary>Facilitator notes recorded when resolving (e.g. outcome description).</summary>
        [StringLength(2000)]
        public virtual string ResolutionNotes { get; set; }
    }
}
