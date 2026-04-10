using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Hires.Dtos
{
    /// <summary>Request model for applying AI personalisation suggestions to a draft journey.</summary>
    public class ApplyPersonalisationDto
    {
        [Required]
        public Guid JourneyId { get; set; }

        /// <summary>
        /// Serialised personalisation suggestions returned by RequestPersonalisation,
        /// possibly edited by the Facilitator before applying.
        /// </summary>
        [Required]
        public string PersonalisationJson { get; set; }
    }
}
