using System;
using System.Collections.Generic;

namespace Hashim.JourneyPoint.Common.Services.OnboardingPlans.Dtos
{
    /// <summary>Result returned after a Groq document import creates a new plan.</summary>
    public class ImportDocumentResultDto
    {
        /// <summary>ID of the newly created OnboardingPlan.</summary>
        public Guid PlanId { get; set; }

        /// <summary>Name of the plan as extracted from the document.</summary>
        public string PlanName { get; set; } = string.Empty;

        /// <summary>Number of OnboardingModule records created.</summary>
        public int ModulesCreated { get; set; }

        /// <summary>Total number of OnboardingTask records created across all modules.</summary>
        public int TasksCreated { get; set; }

        /// <summary>Summary of each module created with its task count.</summary>
        public List<CreatedModuleSummaryDto> Modules { get; set; } = new();
    }

    /// <summary>Summary of a single module created during document import.</summary>
    public class CreatedModuleSummaryDto
    {
        /// <summary>ID of the created OnboardingModule.</summary>
        public Guid Id { get; set; }

        /// <summary>Name of the module as extracted by Groq.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Number of tasks created inside this module.</summary>
        public int TaskCount { get; set; }
    }
}
