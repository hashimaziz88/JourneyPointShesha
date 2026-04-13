using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A reusable template task inside an OnboardingModule.
    /// Defines the work to be done during a phase of onboarding.
    /// Copied to JourneyTask records at journey generation time.
    /// Changes to an OnboardingTask do not affect JourneyTasks already generated.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingTask")]
    public class OnboardingTask : FullAuditedEntity<Guid>
    {
        /// <summary>FK — parent module.</summary>
        public virtual Guid OnboardingModuleId { get; set; }

        /// <summary>Navigation to parent module.</summary>
        public virtual OnboardingModule OnboardingModule { get; set; }

        /// <summary>Task title.</summary>
        [Required, StringLength(200)]
        public virtual string Title { get; set; }

        /// <summary>Full description of what the assignee needs to do.</summary>
        [Required, StringLength(4000)]
        public virtual string Description { get; set; }

        /// <summary>Functional category of the task.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskCategories")]
        public virtual OnboardingTaskCategory Category { get; set; }

        /// <summary>Display order within the module. Must be unique per module.</summary>
        public virtual int OrderIndex { get; set; }

        /// <summary>Days from the hire's StartDate when the generated JourneyTask will be due.</summary>
        public virtual int DueDayOffset { get; set; }

        /// <summary>Which role is responsible for completing this task.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskAssignmentTargets")]
        public virtual OnboardingTaskAssignmentTarget AssignmentTarget { get; set; }

        /// <summary>Whether the hire must acknowledge this task before completing it.</summary>
        [ReferenceList("JourneyPoint", "OnboardingTaskAcknowledgementRules")]
        public virtual OnboardingTaskAcknowledgementRule AcknowledgementRule { get; set; }
    }
}
