using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// An AI-proposed onboarding task extracted from an OnboardingDocument by GroqExtractionService.
    /// The Facilitator reviews proposals and accepts or rejects each one individually.
    /// Accepted proposals are converted to OnboardingTasks via ApplyAcceptedProposals.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.ExtractedTask")]
    public class ExtractedTask : FullAuditedEntity<Guid>
    {
        /// <summary>The document this task was extracted from.</summary>
        public virtual OnboardingDocument Document { get; set; }

        /// <summary>AI-proposed display title of the task.</summary>
        public virtual string Title { get; set; }

        /// <summary>AI-proposed description of what needs to be done.</summary>
        public virtual string Description { get; set; }

        /// <summary>AI-proposed functional category for the task.</summary>
        [ReferenceList("JourneyPoint", "TaskCategories")]
        public virtual RefListTaskCategory? Category { get; set; }

        /// <summary>AI-proposed assignee role for the task.</summary>
        [ReferenceList("JourneyPoint", "TaskAssignedTo")]
        public virtual RefListTaskAssignedTo? AssignedTo { get; set; }

        /// <summary>Current review status of this extracted proposal.</summary>
        [ReferenceList("JourneyPoint", "ExtractedTaskStatuses")]
        public virtual RefListExtractedTaskStatus? Status { get; set; }

        /// <summary>Display order of this proposal within the document's extraction results.</summary>
        public virtual int SortOrder { get; set; }
    }
}
