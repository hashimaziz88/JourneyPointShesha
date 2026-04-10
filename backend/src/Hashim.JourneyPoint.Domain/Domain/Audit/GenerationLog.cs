using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using Shesha.Domain.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.Audit
{
    /// <summary>
    /// Append-only audit record of every AI extraction or personalisation workflow execution.
    /// One record per Groq call, never updated after creation.
    /// API keys and full prompt payloads are never stored here — only summaries safe for logging.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.GenerationLog")]
    public class GenerationLog : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>The type of AI workflow that produced this log entry.</summary>
        [ReferenceList("JourneyPoint", "GenerationLogWorkflowTypes")]
        public virtual GenerationLogWorkflowType WorkflowType { get; set; }

        /// <summary>Execution status of the workflow.</summary>
        [ReferenceList("JourneyPoint", "GenerationLogStatuses")]
        public virtual GenerationLogStatus Status { get; set; }

        /// <summary>FK — associated hire, if applicable.</summary>
        public virtual Guid? HireId { get; set; }

        /// <summary>Navigation to hire.</summary>
        public virtual Hire Hire { get; set; }

        /// <summary>FK — associated journey, if applicable.</summary>
        public virtual Guid? JourneyId { get; set; }

        /// <summary>Navigation to journey.</summary>
        public virtual Journey Journey { get; set; }

        /// <summary>FK — associated onboarding plan, if applicable.</summary>
        public virtual Guid? OnboardingPlanId { get; set; }

        /// <summary>Navigation to plan.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>FK — associated onboarding document, if applicable (extraction workflows).</summary>
        public virtual Guid? OnboardingDocumentId { get; set; }

        /// <summary>Navigation to document.</summary>
        public virtual OnboardingDocument OnboardingDocument { get; set; }

        /// <summary>AI model identifier used for this call (e.g. "llama3-8b-8192").</summary>
        [Required, StringLength(200)]
        public virtual string ModelName { get; set; }

        /// <summary>Non-sensitive summary of the prompt sent (e.g. "Extract tasks from: Graduate Policy v2"). Never contains the full document content.</summary>
        [Required, StringLength(4000)]
        public virtual string PromptSummary { get; set; }

        /// <summary>Brief summary of the AI response (e.g. "Extracted 7 tasks"). Null if workflow failed before response.</summary>
        [StringLength(4000)]
        public virtual string ResponseSummary { get; set; }

        /// <summary>Failure details if Status is Failed. Null on successful runs.</summary>
        [StringLength(1000)]
        public virtual string FailureReason { get; set; }

        /// <summary>Number of tasks added to the plan or journey by this run.</summary>
        public virtual int TasksAdded { get; set; }

        /// <summary>Number of tasks revised/personalised by this run.</summary>
        public virtual int TasksRevised { get; set; }

        /// <summary>UTC timestamp when the workflow started.</summary>
        public virtual DateTime StartedAt { get; set; }

        /// <summary>UTC timestamp when the workflow finished (success or failure).</summary>
        public virtual DateTime CompletedAt { get; set; }

        /// <summary>Total workflow execution duration in milliseconds.</summary>
        public virtual long DurationMilliseconds { get; set; }
    }
}
