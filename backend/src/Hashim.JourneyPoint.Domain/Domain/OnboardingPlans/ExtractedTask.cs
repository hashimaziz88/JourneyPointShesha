using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A Facilitator-reviewable AI-extracted task proposal from an uploaded OnboardingDocument.
    /// Proposals are reviewed individually — Accepted proposals are converted to OnboardingTask
    /// records via ApplyAcceptedProposals. Rejected proposals are retained for audit history.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.ExtractedTask")]
    public class ExtractedTask : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — source document this task was extracted from.</summary>
        public virtual Guid OnboardingDocumentId { get; set; }

        /// <summary>Navigation to source document.</summary>
        public virtual OnboardingDocument OnboardingDocument { get; set; }

        /// <summary>AI-suggested module to place this task in. Optional.</summary>
        public virtual Guid? SuggestedModuleId { get; set; }

        /// <summary>AI-proposed task title.</summary>
        [Required, StringLength(200)]
        public virtual string Title { get; set; }

        /// <summary>AI-proposed task description.</summary>
        [Required, StringLength(4000)]
        public virtual string Description { get; set; }

        /// <summary>AI-proposed functional category.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskCategories")]
        public virtual OnboardingTaskCategory Category { get; set; }

        /// <summary>AI-proposed due day offset from hire start date.</summary>
        public virtual int DueDayOffset { get; set; }

        /// <summary>AI-proposed assignee role.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskAssignmentTargets")]
        public virtual OnboardingTaskAssignmentTarget AssignmentTarget { get; set; }

        /// <summary>AI-proposed acknowledgement rule.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskAcknowledgementRules")]
        public virtual OnboardingTaskAcknowledgementRule AcknowledgementRule { get; set; }

        /// <summary>Facilitator review status of this proposal.</summary>
        [ReferenceList("JourneyPoint", "ExtractedTaskReviewStatuses")]
        public virtual ExtractedTaskReviewStatus ReviewStatus { get; set; }

        /// <summary>ABP UserId of the Facilitator who reviewed this proposal.</summary>
        public virtual long? ReviewedByUserId { get; set; }

        /// <summary>When the Facilitator reviewed this proposal.</summary>
        public virtual DateTime? ReviewedTime { get; set; }

        /// <summary>
        /// The Id of the OnboardingTask created when this proposal was accepted and applied.
        /// Null until AcceptProposal + ApplyAcceptedProposals has been called.
        /// </summary>
        public virtual Guid? AppliedOnboardingTaskId { get; set; }
    }
}
