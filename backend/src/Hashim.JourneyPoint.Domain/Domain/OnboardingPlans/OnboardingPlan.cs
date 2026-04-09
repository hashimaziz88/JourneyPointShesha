using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A reusable template that defines the structure, phases, and tasks for onboarding
    /// a specific type of hire (e.g. Graduate Developer, Senior Manager).
    /// Changes to a Published plan do not affect journeys already generated from it.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingPlan")]
    public class OnboardingPlan : FullAuditedEntity<Guid>
    {
        /// <summary>Display name of the onboarding plan.</summary>
        public virtual string Name { get; set; }

        /// <summary>Description of the plan's purpose and scope.</summary>
        public virtual string Description { get; set; }

        /// <summary>The hire profile this plan is designed for (e.g. "Graduate Developer").</summary>
        public virtual string TargetAudience { get; set; }

        /// <summary>Total expected duration of the onboarding journey in calendar days.</summary>
        public virtual int DurationDays { get; set; }

        /// <summary>Current lifecycle status of the plan.</summary>
        [ReferenceList("JourneyPoint", "OnboardingPlanStatuses")]
        public virtual RefListOnboardingPlanStatus? Status { get; set; }

        /// <summary>
        /// AI-generated enhancement suggestions from GroqExtractionService.
        /// Stored as JSON or plain text pending facilitator review. Null until EnhancePlanWithAi is called.
        /// </summary>
        public virtual string AiEnhancementSuggestions { get; set; }
    }
}
