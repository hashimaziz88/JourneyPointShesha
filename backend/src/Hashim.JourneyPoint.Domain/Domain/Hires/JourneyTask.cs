using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.Hires
{
    /// <summary>
    /// A single task within a hire's active Journey.
    /// Copied from OnboardingTask at journey generation time. Editable by Facilitators only.
    /// Soft-delete Pending tasks; never delete Completed tasks.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.JourneyTask")]
    public class JourneyTask : FullAuditedEntity<Guid>
    {
        /// <summary>The journey this task belongs to.</summary>
        public virtual Journey Journey { get; set; }

        /// <summary>Display title of the task.</summary>
        public virtual string Title { get; set; }

        /// <summary>Full description of what the assignee needs to do.</summary>
        public virtual string Description { get; set; }

        /// <summary>Functional category of the task.</summary>
        [ReferenceList("JourneyPoint", "TaskCategories")]
        public virtual RefListTaskCategory? Category { get; set; }

        /// <summary>Date by which the task should be completed.</summary>
        public virtual DateTime DueDate { get; set; }

        /// <summary>Current completion status of the task.</summary>
        [ReferenceList("JourneyPoint", "JourneyTaskStatuses")]
        public virtual RefListJourneyTaskStatus? Status { get; set; }

        /// <summary>Date and time the task was marked complete.</summary>
        public virtual DateTime? CompletedAt { get; set; }

        /// <summary>Which role is responsible for completing this task.</summary>
        [ReferenceList("JourneyPoint", "TaskAssignedTo")]
        public virtual RefListTaskAssignedTo? AssignedTo { get; set; }

        /// <summary>
        /// When true, the hire must explicitly acknowledge reading/understanding this task
        /// before it can be marked complete.
        /// </summary>
        public virtual bool RequiresAcknowledgement { get; set; }

        /// <summary>True if this task was AI-personalised by GroqPersonalisationService.</summary>
        public virtual bool IsPersonalised { get; set; }

        /// <summary>Date and time the hire acknowledged this task (when RequiresAcknowledgement is true).</summary>
        public virtual DateTime? AcknowledgedAt { get; set; }

        /// <summary>Display order of the task within the journey.</summary>
        public virtual int SortOrder { get; set; }
    }
}
