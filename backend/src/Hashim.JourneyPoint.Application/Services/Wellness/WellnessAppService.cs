using Abp.Application.Services.Dto;
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
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.Wellness
{
    /// <summary>
    /// Manages wellness check-ins for hires during their onboarding journey.
    /// Check-in generation delegates to WellnessManager.
    /// </summary>
    [Route("api/services/app/Wellness/[action]")]
    public class WellnessAppService : SheshaAppServiceBase
    {
        private readonly IRepository<WellnessCheckIn, Guid> _checkInRepository;
        private readonly IRepository<WellnessQuestion, Guid> _questionRepository;
        private readonly IRepository<Journey, Guid> _journeyRepository;
        private readonly IRepository<Hire, Guid> _hireRepository;
        private readonly WellnessManager _wellnessManager;

        public WellnessAppService(
            IRepository<WellnessCheckIn, Guid> checkInRepository,
            IRepository<WellnessQuestion, Guid> questionRepository,
            IRepository<Journey, Guid> journeyRepository,
            IRepository<Hire, Guid> hireRepository,
            WellnessManager wellnessManager)
        {
            _checkInRepository = checkInRepository;
            _questionRepository = questionRepository;
            _journeyRepository = journeyRepository;
            _hireRepository    = hireRepository;
            _wellnessManager   = wellnessManager;
        }

        /// <summary>Returns all wellness check-ins for a hire. Facilitator use.</summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<WellnessCheckIn, Guid>>> GetHireWellnessOverview(Guid hireId)
        {
            var checkIns = await _checkInRepository.GetAllListAsync(c => c.HireId == hireId);
            var items = new List<DynamicDto<WellnessCheckIn, Guid>>();
            foreach (var checkIn in checkIns)
                items.Add(await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(checkIn));
            return new PagedResultDto<DynamicDto<WellnessCheckIn, Guid>>(items.Count, items);
        }

        /// <summary>Returns all wellness check-ins for the currently logged-in hire.</summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<WellnessCheckIn, Guid>>> GetMyCheckIns()
        {
            var hire = await _hireRepository.FirstOrDefaultAsync(h => h.PlatformUserId == AbpSession.UserId);
            if (hire == null)
                throw new UserFriendlyException("No hire record found for the current user.");

            var checkIns = await _checkInRepository.GetAllListAsync(c => c.HireId == hire.Id);
            var items = new List<DynamicDto<WellnessCheckIn, Guid>>();
            foreach (var checkIn in checkIns)
                items.Add(await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(checkIn));
            return new PagedResultDto<DynamicDto<WellnessCheckIn, Guid>>(items.Count, items);
        }

        /// <summary>
        /// Returns the current pending wellness check-in for the logged-in hire.
        /// Returns the earliest Pending check-in by scheduled date, or throws if none exist.
        /// </summary>
        [HttpGet]
        public async Task<DynamicDto<WellnessCheckIn, Guid>> GetMyPendingCheckIn()
        {
            var hire = await _hireRepository.FirstOrDefaultAsync(h => h.PlatformUserId == AbpSession.UserId);
            if (hire == null)
                throw new UserFriendlyException("No hire record found for the current user.");

            var checkIns = await _checkInRepository.GetAllListAsync(c =>
                c.HireId == hire.Id && c.Status == WellnessCheckInStatus.Pending);

            checkIns.Sort((a, b) => a.ScheduledDate.CompareTo(b.ScheduledDate));
            var pending = checkIns.Count > 0 ? checkIns[0] : null;

            if (pending == null)
                throw new UserFriendlyException("No pending wellness check-in found for the current user.");

            return await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(pending);
        }

        /// <summary>
        /// Returns a paged list of WellnessQuestions across every check-in for the currently logged-in hire.
        /// Use this as the data source for a question-level table in the enrolee portal.
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<WellnessQuestion, Guid>>> GetMyQuestions(int pageNumber = 1)
        {
            const int PAGE_SIZE = 10;

            var hire = await _hireRepository.FirstOrDefaultAsync(h => h.PlatformUserId == AbpSession.UserId);
            if (hire == null)
                throw new UserFriendlyException("No hire record found for the current user.");

            var checkIns = await _checkInRepository.GetAllListAsync(c => c.HireId == hire.Id);

            var allQuestions = new List<WellnessQuestion>();
            foreach (var checkIn in checkIns)
            {
                var questions = await _questionRepository.GetAllListAsync(q => q.WellnessCheckInId == checkIn.Id);
                allQuestions.AddRange(questions);
            }

            var totalCount = allQuestions.Count;
            var page = allQuestions
                .Skip((pageNumber - 1) * PAGE_SIZE)
                .Take(PAGE_SIZE)
                .ToList();

            var items = new List<DynamicDto<WellnessQuestion, Guid>>();
            foreach (var question in page)
                items.Add(await MapToDynamicDtoAsync<WellnessQuestion, Guid>(question));

            return new PagedResultDto<DynamicDto<WellnessQuestion, Guid>>(totalCount, items);
        }

        /// <summary>
        /// Returns all WellnessQuestions for a specific check-in.
        /// Use this when drilling into a single check-in from the enrolee portal.
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<WellnessQuestion, Guid>>> GetQuestionsForCheckIn(Guid checkInId)
        {
            var questions = await _questionRepository.GetAllListAsync(q => q.WellnessCheckInId == checkInId);

            var items = new List<DynamicDto<WellnessQuestion, Guid>>();
            foreach (var question in questions)
                items.Add(await MapToDynamicDtoAsync<WellnessQuestion, Guid>(question));

            return new PagedResultDto<DynamicDto<WellnessQuestion, Guid>>(items.Count, items);
        }

        /// <summary>Returns the full detail of a single check-in including all questions and answers.</summary>
        [HttpGet]
        public async Task<DynamicDto<WellnessCheckIn, Guid>> GetCheckInDetail(Guid checkInId)
        {
            var checkIn = await _checkInRepository.GetAsync(checkInId);
            return await MapToDynamicDtoAsync<WellnessCheckIn, Guid>(checkIn);
        }

        /// <summary>Saves the hire's answer to a single WellnessQuestion. Answers are saved incrementally.</summary>
        [HttpPost]
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
        [HttpPost]
        public async Task<DynamicDto<WellnessQuestion, Guid>> GenerateAnswerSuggestion(Guid questionId)
        {
            // TODO: delegate to GroqPersonalisationService for context-aware answer suggestion
            throw new NotImplementedException("GenerateAnswerSuggestion: call Groq with hire context, store in WellnessQuestion.AiSuggestedAnswer");
        }

        /// <summary>
        /// Submits a completed check-in. Validates that all questions have answers,
        /// then triggers Groq to generate an AI summary for the Facilitator.
        /// </summary>
        [HttpPost]
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
        [HttpPost]
        public async Task GenerateCheckInsForJourney(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsync(journeyId);
            if (journey == null)
                throw new UserFriendlyException($"Journey '{journeyId}' not found.");

            await _wellnessManager.GenerateCheckInsForJourneyAsync(journey.HireId, journeyId);
        }
    }
}
