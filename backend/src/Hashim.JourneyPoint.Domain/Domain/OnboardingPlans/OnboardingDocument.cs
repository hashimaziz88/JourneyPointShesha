using Abp.Domain.Entities.Auditing;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Shesha.Domain;
using Shesha.Domain.Attributes;
using System;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// A document uploaded to an OnboardingPlan for AI-assisted task extraction.
    /// The Facilitator uploads HR policy documents, role descriptions, or onboarding guides.
    /// GroqExtractionService processes the document and produces ExtractedTask proposals.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.OnboardingDocument")]
    public class OnboardingDocument : FullAuditedEntity<Guid>
    {
        /// <summary>The plan this document was uploaded against.</summary>
        public virtual OnboardingPlan Plan { get; set; }

        /// <summary>Display title of the document.</summary>
        public virtual string Title { get; set; }

        /// <summary>Reference to the uploaded file in Shesha's file store.</summary>
        public virtual StoredFile File { get; set; }

        /// <summary>Current processing status of the document.</summary>
        [ReferenceList("JourneyPoint", "OnboardingDocumentStatuses")]
        public virtual RefListOnboardingDocumentStatus? Status { get; set; }

        /// <summary>
        /// Raw text content extracted from the document prior to AI processing.
        /// Not logged to prevent sensitive HR content appearing in application logs.
        /// </summary>
        public virtual string ExtractedContent { get; set; }
    }
}
