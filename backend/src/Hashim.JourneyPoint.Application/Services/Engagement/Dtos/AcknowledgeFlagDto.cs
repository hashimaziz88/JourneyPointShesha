using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Engagement.Dtos
{
    /// <summary>Request model for acknowledging an active AtRiskFlag.</summary>
    public class AcknowledgeFlagDto
    {
        [Required]
        public Guid FlagId { get; set; }

        /// <summary>Optional notes from the Facilitator describing their planned response.</summary>
        public string Notes { get; set; }
    }
}
