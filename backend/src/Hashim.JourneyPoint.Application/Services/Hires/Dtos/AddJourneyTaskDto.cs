using Hashim.JourneyPoint.Domain.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Hires.Dtos
{
    /// <summary>Request model for adding a new ad-hoc task to an active journey.</summary>
    public class AddJourneyTaskDto
    {
        [Required]
        public Guid JourneyId { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }
        public OnboardingTaskCategory Category { get; set; }

        [Required]
        public DateTime DueOn { get; set; }

        public OnboardingTaskAssignmentTarget AssignmentTarget { get; set; }
        public OnboardingTaskAcknowledgementRule AcknowledgementRule { get; set; }
    }
}
