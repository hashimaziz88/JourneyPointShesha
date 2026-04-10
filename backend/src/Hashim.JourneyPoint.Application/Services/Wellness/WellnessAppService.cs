using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.Wellness.Dtos;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Hashim.JourneyPoint.Domain.Domain.Wellness;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.Wellness
{
    /// <summary>
    /// Manages wellness check-ins for hires during their onboarding journey.
    /// Check-in generation delegates to WellnessManager.
    /// </summary>
    public class WellnessAppService : SheshaAppServiceBase
    {
        private readonly IRepository<WellnessCheckIn, Guid> _checkInRepository;
        private readonly IRepository<WellnessQuestion, Guid> _questionRepository;
        private readonly IRepository<Journey, Guid> _journeyRepository;
        private readonly WellnessManager _wellnessManager;

        public WellnessAppService(
            IRepository<WellnessCheckIn, Guid> checkInRepository,
            IRepository<WellnessQuestion, Guid> questionRepository,
            IRepository<Journey, Guid> journeyRepository,
            WellnessManager wellnessManager)
        {
            _checkInRepository = checkInRepository;
            _questionRepository = questionRepository;
            _journeyRepository = journeyRepository;
            _wellnessManager   = wellnessManager;
        }

        /// <summary>Returns a summary of all wellness check-ins for a hire, ordered by period.</summary>
        [HttpGet, Route("[action]")]
        public async Task<List<DynamicDto<WellnessCheckIn, Guid>>> GetHireWellnessOverview(Guid hireId)
        {
            var checkIns = await _checkInRepository.GetAllListAsync(c => c.HireId == hireId);
            var result = new List<DynamicDto<WellnessCheckIn, Guid>>();
            foreach (var checkIn in checkIns)
                result.Add(await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(checkIn));
            return result;
        }

        /// <summary>Returns the full detail of a single check-in including all questions and answers.</summary>
        [HttpGet, Route("[action]")]
        public async Task<DynamicDto<WellnessCheckIn, Guid>> GetCheckInDetail(Guid checkInId)
        {
            var checkIn = await _checkInRepository.GetAsync(checkInId);
            return await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(checkIn);
        }

        /// <summary>Saves the hire's answer to a single WellnessQuestion. Answers are saved incrementally.</summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<WellnessQuestion, Guid>> SaveAnswer(SaveAnswerDto input)
        {
            var question = await _questionRepository.GetAsync(input.QuestionId);
            question.AnswerText = input.Answer;
            question.IsAnswered = !string.IsNullOrWhiteSpace(input.Answer);
            var updated = await _questionRepository.UpdateAsync(question);
            return await MapToDynamicDtoAsync<WellnessQuestion, Guid>(updated);
        }

        /// <summary>
        /// Calls Groq to generate a suggested answer for a WellnessQuestion based on the hire's context.
        /// The suggestion is stored on the question but not applied until the hire chooses to use it.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<WellnessQuestion, Guid>> GenerateAnswerSuggestion(Guid questionId)
        {
            // TODO: delegate to GroqPersonalisationService for context-aware answer suggestion
            throw new NotImplementedException("GenerateAnswerSuggestion: call Groq with hire context, store in WellnessQuestion.AiSuggestedAnswer");
        }

        /// <summary>
        /// Submits a completed check-in. Validates that all questions have answers,
        /// then triggers Groq to generate an AI summary for the Facilitator.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task<DynamicDto<WellnessCheckIn, Guid>> SubmitCheckIn(Guid checkInId)
        {
            var checkIn = await _checkInRepository.GetAsync(checkInId);

            if (checkIn.Status == WellnessCheckInStatus.Completed)
                throw new UserFriendlyException("This check-in has already been submitted.");

            var questions = await _questionRepository.GetAllListAsync(q => q.WellnessCheckInId == checkInId);
            foreach (var q in questions)
            {
                if (string.IsNullOrWhiteSpace(q.AnswerText))
                    throw new UserFriendlyException($"All questions must be answered before submitting. Question '{q.QuestionText}' has no answer.");
            }

            checkIn.Status      = WellnessCheckInStatus.Completed;
            checkIn.SubmittedAt = DateTime.UtcNow;

            // TODO: enqueue background job → Groq summary generation → update checkIn.InsightSummary
            var updated = await _checkInRepository.UpdateAsync(checkIn);
            return await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(updated);
        }

        /// <summary>
        /// Generates the milestone WellnessCheckIn schedule for a journey.
        /// Called internally when a journey is activated — not intended for direct external use.
        /// </summary>
        [HttpPost, Route("[action]")]
        public async Task GenerateCheckInsForJourney(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsync(journeyId);
            if (journey == null)
                throw new UserFriendlyException($"Journey '{journeyId}' not found.");

            await _wellnessManager.GenerateCheckInsForJourneyAsync(journey.HireId, journeyId);
        }
    }
}
