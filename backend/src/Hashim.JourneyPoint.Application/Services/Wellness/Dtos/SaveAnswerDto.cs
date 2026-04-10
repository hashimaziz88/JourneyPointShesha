using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Wellness.Dtos
{
    /// <summary>Request model for saving a hire's answer to a WellnessQuestion.</summary>
    public class SaveAnswerDto
    {
        [Required]
        public Guid QuestionId { get; set; }

        /// <summary>The hire's free-text answer.</summary>
        [Required]
        public string Answer { get; set; }
    }
}
