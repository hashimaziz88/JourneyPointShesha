using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.Hires
{
    /// <summary>
    /// Represents an employee being onboarded through JourneyPoint.
    /// Created by the Facilitator when a new hire joins the organisation.
    /// Each Hire is enrolled in exactly one OnboardingPlan, which generates their Journey.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.Hire")]
    public class Hire : FullAuditedEntity<Guid>
    {
        /// <summary>Full legal name of the hire.</summary>
        public virtual string FullName { get; set; }

        /// <summary>Work email address used to create the hire's user account.</summary>
        public virtual string Email { get; set; }

        /// <summary>The hire's job title within the organisation.</summary>
        public virtual string RoleTitle { get; set; }

        /// <summary>Department or directorate the hire is joining.</summary>
        public virtual string DepartmentName { get; set; }

        /// <summary>Official first day of employment. Anchor date for all journey task due dates.</summary>
        public virtual DateTime StartDate { get; set; }

        /// <summary>Current lifecycle status of the hire record.</summary>
        [ReferenceList("JourneyPoint", "HireStatuses")]
        public virtual RefListHireStatus? Status { get; set; }

        /// <summary>The onboarding plan template this hire was enrolled in.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>ABP UserId of the hire's line manager. Nullable — not all hires have a manager assigned.</summary>
        public virtual long? ManagerUserId { get; set; }

        /// <summary>Date and time the welcome notification email was last sent.</summary>
        public virtual DateTime? WelcomeEmailSentAt { get; set; }
    }
}
