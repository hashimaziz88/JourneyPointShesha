using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// An uploaded document attached to an OnboardingPlan for AI-assisted task extraction.
    /// The Facilitator uploads HR policy documents, role descriptions, or onboarding guides.
    /// GroqExtractionService processes the document and produces ExtractedTask proposals.
    /// The full document content is never stored in application logs.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingDocument")]
    public class OnboardingDocument : FullAuditedEntity<Guid>
    {
        /// <summary>FK — parent plan.</summary>
        public virtual Guid OnboardingPlanId { get; set; }

        /// <summary>Navigation to parent plan.</summary>
        public virtual OnboardingPlan OnboardingPlan { get; set; }

        /// <summary>Original uploaded file name (e.g. "graduate-onboarding-policy.pdf").</summary>
        [Required, StringLength(260)]
        public virtual string FileName { get; set; }

        /// <summary>Server-side storage path where the file is persisted.</summary>
        [Required, StringLength(500)]
        public virtual string StoragePath { get; set; }

        /// <summary>MIME type of the uploaded file (e.g. "application/pdf").</summary>
        [Required, StringLength(200)]
        public virtual string ContentType { get; set; }

        /// <summary>File size in bytes.</summary>
        public virtual long FileSizeBytes { get; set; }

        /// <summary>Current processing status of the document.</summary>
        [ReferenceList("JourneyPoint", "OnboardingDocumentStatuses")]
        public virtual OnboardingDocumentStatus Status { get; set; }

        /// <summary>Total number of task proposals extracted from this document by AI.</summary>
        public virtual int ExtractedTaskCount { get; set; }

        /// <summary>Number of extracted proposals accepted by the Facilitator.</summary>
        public virtual int AcceptedTaskCount { get; set; }

        /// <summary>Extraction failure reason if Status is Failed.</summary>
        [StringLength(2000)]
        public virtual string FailureReason { get; set; }

        /// <summary>UTC timestamp when extraction completed (successfully or with failure).</summary>
        public virtual DateTime? ExtractionCompletedTime { get; set; }

        /// <summary>All AI-extracted task proposals from this document.</summary>
        public virtual ICollection<ExtractedTask> ExtractedTasks { get; set; }
    }
}
