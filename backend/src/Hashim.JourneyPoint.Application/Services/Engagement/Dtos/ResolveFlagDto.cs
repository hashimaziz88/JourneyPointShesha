using Hashim.JourneyPoint.Domain.Domain.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Engagement.Dtos
{
    /// <summary>Request model for resolving an acknowledged AtRiskFlag.</summary>
    public class ResolveFlagDto
    {
        [Required]
        public Guid FlagId { get; set; }

        /// <summary>How the at-risk situation was resolved (ManualFacilitatorResolution, AutomaticHealthyRecovery, or HireExited).</summary>
        [Required]
        public AtRiskResolutionType ResolutionType { get; set; }

        /// <summary>Optional resolution notes explaining how the risk was addressed.</summary>
        public string ResolutionNotes { get; set; }
    }
}
