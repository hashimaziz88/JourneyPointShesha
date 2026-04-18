using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Hires.Dtos
{
    /// <summary>Request model for creating a new hire. Field names match Shesha's wire format.</summary>
    public class CreateHireDto
    {
        /// <summary>Full legal name of the hire.</summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>Work email address — used to create the hire's user account.</summary>
        [Required, EmailAddress]
        public string EmailAddress { get; set; }

        /// <summary>The hire's job title within the organisation.</summary>
        [Required]
        public string RoleTitle { get; set; }

        /// <summary>Department or directorate the hire is joining.</summary>
        [Required]
        public string Department { get; set; }

        /// <summary>Official first day of employment.</summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>The OnboardingPlan to enrol this hire in (Shesha sends as nested object).</summary>
        [Required]
        public EntityRefDto OnboardingPlan { get; set; }

        /// <summary>Optional ABP UserId of the hire's line manager.</summary>
        public long? ManagerUserId { get; set; }
    }

    /// <summary>Minimal entity reference as sent by Shesha's form engine.</summary>
    public class EntityRefDto
    {
        public Guid Id { get; set; }
    }
}
