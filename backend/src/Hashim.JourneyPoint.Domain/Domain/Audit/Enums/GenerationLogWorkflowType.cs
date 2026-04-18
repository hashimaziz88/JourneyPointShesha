using Shesha.Domain.Attributes;
using System.ComponentModel;

namespace Hashim.JourneyPoint.Domain.Domain.Enums
{
    /// <summary>Type of AI workflow recorded in a GenerationLog entry.</summary>
    [ReferenceList("JourneyPoint", "GenerationLogWorkflowTypes")]
    public enum GenerationLogWorkflowType : long
    {
        /// <summary>AI task extraction from an uploaded OnboardingDocument.</summary>
        [Description("Extraction")]
        Extraction = 1,

        /// <summary>AI personalisation of a hire's Journey tasks.</summary>
        [Description("Personalisation")]
        Personalisation = 2,

        /// <summary>AI generation of WellnessCheckIn questions for a hire.</summary>
        [Description("Wellness Question Generation")]
        WellnessQuestionGeneration = 3,

        /// <summary>AI suggestion of an answer for a WellnessQuestion based on hire context.</summary>
        [Description("Wellness Answer Suggestion")]
        WellnessAnswerSuggestion = 4,

        /// <summary>AI enhancement of OnboardingPlan module and task content.</summary>
        [Description("Plan Enhancement")]
        PlanEnhancement = 5
    }
}
