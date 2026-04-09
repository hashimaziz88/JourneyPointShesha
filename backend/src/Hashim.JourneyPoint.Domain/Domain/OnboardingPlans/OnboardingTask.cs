using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A task template within an OnboardingModule. Defines the work that needs to be done
    /// during that phase of onboarding. Copied to JourneyTask records when a Journey is generated.
    /// Changes to OnboardingTask do not affect JourneyTasks already generated.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingTask")]
    public class OnboardingTask : FullAuditedEntity<Guid>
    {
        /// <summary>The module this task belongs to.</summary>
        public virtual OnboardingModule Module { get; set; }

        /// <summary>Display title of the task.</summary>
        public virtual string Title { get; set; }

        /// <summary>Full description of what the assignee needs to do.</summary>
        public virtual string Description { get; set; }

        /// <summary>Functional category of the task.</summary>
        [ReferenceList("JourneyPoint", "TaskCategories")]
        public virtual RefListTaskCategory? Category { get; set; }

        /// <summary>Which role is responsible for completing this task.</summary>
        [ReferenceList("JourneyPoint", "TaskAssignedTo")]
        public virtual RefListTaskAssignedTo? AssignedTo { get; set; }

        /// <summary>
        /// Number of days after the hire's StartDate by which this task should be due.
        /// Used to calculate JourneyTask.DueDate at journey generation time.
        /// </summary>
        public virtual int DueDayOffset { get; set; }

        /// <summary>When true, the generated JourneyTask will require explicit hire acknowledgement.</summary>
        public virtual bool RequiresAcknowledgement { get; set; }

        /// <summary>Display order of this task within its module.</summary>
        public virtual int SortOrder { get; set; }
    }
}
