using Hashim.JourneyPoint.Domain.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos
{
    /// <summary>Request model for editing an AI-extracted task proposal before accepting it.</summary>
    public class UpdateProposalDto
    {
        [Required]
        public Guid ExtractedTaskId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public OnboardingTaskCategory? Category { get; set; }
        public OnboardingTaskAssignmentTarget? AssignmentTarget { get; set; }
        public OnboardingTaskAcknowledgementRule? AcknowledgementRule { get; set; }
        public int? DueDayOffset { get; set; }
    }
}
