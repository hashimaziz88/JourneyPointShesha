using Hashim.JourneyPoint.Domain.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Hires.Dtos
{
    /// <summary>Request model for updating the details of an existing JourneyTask.</summary>
    public class UpdateJourneyTaskDto
    {
        [Required]
        public Guid TaskId { get; set; }

        public string Title { get; set; }
        public string Description { get; set; }
        public OnboardingTaskCategory? Category { get; set; }
        public DateTime? DueOn { get; set; }
        public OnboardingTaskAssignmentTarget? AssignmentTarget { get; set; }
        public OnboardingTaskAcknowledgementRule? AcknowledgementRule { get; set; }
    }
}
