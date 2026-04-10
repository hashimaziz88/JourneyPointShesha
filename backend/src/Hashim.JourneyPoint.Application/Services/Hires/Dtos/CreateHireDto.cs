using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Common.Services.Hires.Dtos
{
    /// <summary>Request model for creating a new hire and auto-generating their onboarding journey.</summary>
    public class CreateHireDto
    {
        /// <summary>Full legal name of the hire.</summary>
        [Required]
        public string FullName { get; set; }

        /// <summary>Work email address — used to create the hire's user account.</summary>
        [Required, EmailAddress]
        public string Email { get; set; }

        /// <summary>The hire's job title within the organisation.</summary>
        [Required]
        public string RoleTitle { get; set; }

        /// <summary>Department or directorate the hire is joining.</summary>
        [Required]
        public string DepartmentName { get; set; }

        /// <summary>Official first day of employment.</summary>
        [Required]
        public DateTime StartDate { get; set; }

        /// <summary>The published OnboardingPlan to enrol this hire in.</summary>
        [Required]
        public Guid OnboardingPlanId { get; set; }

        /// <summary>Optional ABP UserId of the hire's line manager.</summary>
        public long? ManagerUserId { get; set; }
    }
}
