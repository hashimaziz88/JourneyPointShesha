using Abp.Domain.Entities;
using Abp.Domain.Entities.Auditing;
using Shesha.Domain.Attributes;
using System;
using System.ComponentModel.DataAnnotations;

namespace Hashim.JourneyPoint.Domain.Domain.Wellness
{
    /// <summary>
    /// One AI-generated question within a WellnessCheckIn, along with the hire's answer.
    /// Questions are pre-populated at check-in generation time by Groq.
    /// Answers are saved incrementally; submission requires all questions to be answered.
    /// </summary>
    [Entity(TypeShortAlias = "JourneyPoint.WellnessQuestion")]
    public class WellnessQuestion : FullAuditedEntity<Guid>, IMustHaveTenant
    {
        /// <summary>Tenant ownership.</summary>
        public virtual int TenantId { get; set; }

        /// <summary>FK — parent check-in.</summary>
        public virtual Guid WellnessCheckInId { get; set; }

        /// <summary>Navigation to parent check-in.</summary>
        public virtual WellnessCheckIn WellnessCheckIn { get; set; }

        /// <summary>Display order within the check-in.</summary>
        public virtual int OrderIndex { get; set; }

        /// <summary>AI-generated question text shown to the hire.</summary>
        [Required, StringLength(1000)]
        public virtual string QuestionText { get; set; }

        /// <summary>The hire's free-text answer. Null until the hire has typed a response.</summary>
        [StringLength(3000)]
        public virtual string AnswerText { get; set; }

        /// <summary>
        /// AI-drafted answer generated on demand by GenerateAnswerSuggestion.
        /// The hire may use this as a starting point or ignore it entirely.
        /// </summary>
        [StringLength(3000)]
        public virtual string AiSuggestedAnswer { get; set; }

        /// <summary>True if the hire has submitted a non-empty answer for this question.</summary>
        public virtual bool IsAnswered { get; set; }
    }
}
