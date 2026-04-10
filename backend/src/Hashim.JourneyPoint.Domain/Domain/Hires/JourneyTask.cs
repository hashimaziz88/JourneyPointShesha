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
    /// One copied and optionally personalised task inside a hire-specific Journey.
    /// Generated from an OnboardingTask at journey creation time. The Facilitator may edit,
    /// add, or soft-delete Pending tasks. Completed tasks must never be deleted.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.JourneyTask")]
    public class JourneyTask : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — parent journey.</summary>
        public virtual Guid JourneyId { get; set; }

        /// <summary>Navigation to journey.</summary>
        public virtual Journey Journey { get; set; }

        /// <summary>FK — the template task this was copied from. Null for ad-hoc tasks.</summary>
        public virtual Guid? SourceOnboardingTaskId { get; set; }

        /// <summary>Navigation to source template task.</summary>
        public virtual OnboardingTask SourceOnboardingTask { get; set; }

        /// <summary>FK — the template module this task was copied from. Null for ad-hoc tasks.</summary>
        public virtual Guid? SourceOnboardingModuleId { get; set; }

        /// <summary>Navigation to source template module.</summary>
        public virtual OnboardingModule SourceOnboardingModule { get; set; }

        /// <summary>Snapshot of the module name at the time the journey was generated.</summary>
        [Required, StringLength(200)]
        public virtual string ModuleTitle { get; set; }

        /// <summary>Display order of the module within the journey (copied from OnboardingModule.OrderIndex).</summary>
        public virtual int ModuleOrderIndex { get; set; }

        /// <summary>Display order of this task within its module (copied from OnboardingTask.OrderIndex).</summary>
        public virtual int TaskOrderIndex { get; set; }

        /// <summary>Task title.</summary>
        [Required, StringLength(200)]
        public virtual string Title { get; set; }

        /// <summary>Full description of what the assignee needs to do.</summary>
        [Required, StringLength(4000)]
        public virtual string Description { get; set; }

        /// <summary>Functional category of the task.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskCategories")]
        public virtual OnboardingTaskCategory Category { get; set; }

        /// <summary>Which role is responsible for completing this task.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskAssignmentTargets")]
        public virtual OnboardingTaskAssignmentTarget AssignmentTarget { get; set; }

        /// <summary>Acknowledgement requirement for this task.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskAcknowledgementRules")]
        public virtual OnboardingTaskAcknowledgementRule AcknowledgementRule { get; set; }

        /// <summary>Days from the hire's StartDate when this task is due.</summary>
        public virtual int DueDayOffset { get; set; }

        /// <summary>Computed absolute due date (Hire.StartDate + DueDayOffset days).</summary>
        public virtual DateTime DueOn { get; set; }

        /// <summary>Current completion state of the task.</summary>
        [ReferenceList("JourneyPoint", "JourneyTaskStatuses")]
        public virtual JourneyTaskStatus Status { get; set; }

        /// <summary>When the hire acknowledged this task (if AcknowledgementRule requires it).</summary>
        public virtual DateTime? AcknowledgedAt { get; set; }

        /// <summary>When the task was marked complete.</summary>
        public virtual DateTime? CompletedAt { get; set; }

        /// <summary>ABP UserId of the person who completed the task.</summary>
        public virtual long? CompletedByUserId { get; set; }

        /// <summary>When AI personalisation was applied to this task by GroqPersonalisationService.</summary>
        public virtual DateTime? PersonalisedAt { get; set; }
    }
}
