using Abp.Application.Services.Dto;
using Abp.Domain.Repositories;
using Abp.UI;
using Hashim.JourneyPoint.Common.Services.Hires.Dtos;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.Hires;
using Hashim.JourneyPoint.Domain.Domain.Wellness;
using Microsoft.AspNetCore.Mvc;
using Shesha;
using Shesha.DynamicEntities.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Common.Services.Hires
{
    /// <summary>
    /// Manages Journeys and JourneyTasks. Used by Facilitators, Enrolees, and Managers.
    /// Journey generation delegates to HireJourneyManager.
    /// Wellness check-in generation delegates to WellnessManager on activation.
    /// </summary>
    [Route("api/services/app/Journey/[action]")]
    public class JourneyAppService : SheshaAppServiceBase
    {
        private readonly IRepository<Journey, Guid> _journeyRepository;
        private readonly IRepository<JourneyTask, Guid> _taskRepository;
        private readonly IRepository<Hire, Guid> _hireRepository;
        private readonly HireJourneyManager _hireJourneyManager;
        private readonly WellnessManager _wellnessManager;

        public JourneyAppService(
            IRepository<Journey, Guid> journeyRepository,
            IRepository<JourneyTask, Guid> taskRepository,
            IRepository<Hire, Guid> hireRepository,
            HireJourneyManager hireJourneyManager,
            WellnessManager wellnessManager)
        {
            _journeyRepository   = journeyRepository;
            _taskRepository      = taskRepository;
            _hireRepository      = hireRepository;
            _hireJourneyManager  = hireJourneyManager;
            _wellnessManager     = wellnessManager;
        }

        /// <summary>Generates a Draft journey for a hire by copying all tasks from their OnboardingPlan.</summary>
        [HttpPost]
        public async Task<DynamicDto<Journey, Guid>> GenerateDraft(Guid hireId)
        {
            var journey = await _hireJourneyManager.CreateDraftJourneyAsync(hireId);
            return await MapToDynamicDtoAsync<Journey, Guid>(journey);
        }

        /// <summary>Returns the Draft journey for a hire, including all pending tasks.</summary>
        [HttpGet]
        public async Task<DynamicDto<Journey, Guid>> GetDraft(Guid hireId)
        {
            var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                j.HireId == hireId && j.Status == JourneyStatus.Draft);

            if (journey == null)
                throw new UserFriendlyException($"No draft journey found for hire '{hireId}'.");

            return await MapToDynamicDtoAsync<Journey, Guid>(journey);
        }

        /// <summary>Returns the active journey for the currently logged-in enrolee.</summary>
        [HttpGet]
        public async Task<DynamicDto<Journey, Guid>> GetMyJourney()
        {
            var hire = await _hireRepository.FirstOrDefaultAsync(h => h.PlatformUserId == AbpSession.UserId);
            if (hire == null)
                throw new UserFriendlyException("No hire record found for the current user.");

            var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                j.HireId == hire.Id && j.Status == JourneyStatus.Active);

            if (journey == null)
                throw new UserFriendlyException("No active journey found for the current user.");

            return await MapToDynamicDtoAsync<Journey, Guid>(journey);
        }

        /// <summary>
        /// Returns all JourneyTasks for the hire linked to the specified ABP platform user ID.
        /// Used to populate a Shesha DataTable filtered by a specific user.
        /// </summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<JourneyTask, Guid>>> GetTasksByUserId(long userId)
        {
            var hire = await _hireRepository.FirstOrDefaultAsync(h => h.PlatformUserId == userId);
            if (hire == null)
                throw new UserFriendlyException($"No hire record found for user '{userId}'.");

            var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                j.HireId == hire.Id && j.Status == JourneyStatus.Active);

            if (journey == null)
                throw new UserFriendlyException($"No active journey found for user '{userId}'.");

            var tasks = await _taskRepository.GetAllListAsync(t => t.JourneyId == journey.Id);

            var items = new List<DynamicDto<JourneyTask, Guid>>();
            foreach (var task in tasks)
                items.Add(await MapToDynamicDtoAsync<JourneyTask, Guid>(task));

            return new PagedResultDto<DynamicDto<JourneyTask, Guid>>(items.Count, items);
        }

        /// <summary>Returns all Enrolee-assigned JourneyTasks for the currently logged-in hire's active journey.</summary>
        [HttpGet]
        public async Task<PagedResultDto<DynamicDto<JourneyTask, Guid>>> GetMyTasks()
        {
            var hire = await _hireRepository.FirstOrDefaultAsync(h => h.PlatformUserId == AbpSession.UserId);
            if (hire == null)
                throw new UserFriendlyException("No hire record found for the current user.");

            var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                j.HireId == hire.Id && j.Status == JourneyStatus.Active);

            if (journey == null)
                throw new UserFriendlyException("No active journey found for the current user.");

            var tasks = await _taskRepository.GetAllListAsync(t =>
                t.JourneyId == journey.Id
                && t.AssignmentTarget == OnboardingTaskAssignmentTarget.Enrolee);

            var items = new List<DynamicDto<JourneyTask, Guid>>();
            foreach (var task in tasks)
                items.Add(await MapToDynamicDtoAsync<JourneyTask, Guid>(task));

            return new PagedResultDto<DynamicDto<JourneyTask, Guid>>(items.Count, items);
        }

        /// <summary>Returns all JourneyTasks assigned to the currently logged-in manager.</summary>
        [HttpGet]
        public async Task<List<DynamicDto<JourneyTask, Guid>>> GetManagerTasks()
        {
            var managedHires = await _hireRepository.GetAllListAsync(h => h.ManagerUserId == AbpSession.UserId);

            var result = new List<DynamicDto<JourneyTask, Guid>>();
            foreach (var hire in managedHires)
            {
                var journey = await _journeyRepository.FirstOrDefaultAsync(j =>
                    j.HireId == hire.Id && j.Status == JourneyStatus.Active);

                if (journey == null) continue;

                var tasks = await _taskRepository.GetAllListAsync(t =>
                    t.JourneyId == journey.Id
                    && t.AssignmentTarget == OnboardingTaskAssignmentTarget.Manager);

                foreach (var task in tasks)
                    result.Add(await MapToDynamicDtoAsync<JourneyTask, Guid>(task));
            }
            return result;
        }

        /// <summary>Returns a single JourneyTask for the currently logged-in enrolee.</summary>
        [HttpGet]
        public async Task<DynamicDto<JourneyTask, Guid>> GetMyTask(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(task);
        }

        /// <summary>Returns a single JourneyTask for the currently logged-in manager.</summary>
        [HttpGet]
        public async Task<DynamicDto<JourneyTask, Guid>> GetManagerTask(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(task);
        }

        /// <summary>Records that the enrolee has acknowledged a task that requires acknowledgement.</summary>
        [HttpPost]
        public async Task<DynamicDto<JourneyTask, Guid>> AcknowledgeMyTask(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);

            if (task.AcknowledgementRule != OnboardingTaskAcknowledgementRule.Required)
                throw new UserFriendlyException("This task does not require acknowledgement.");

            task.AcknowledgedAt = DateTime.UtcNow;
            var updated = await _taskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(updated);
        }

        /// <summary>Marks a JourneyTask as Complete for the currently logged-in enrolee.</summary>
        [HttpPost]
        public async Task<DynamicDto<JourneyTask, Guid>> CompleteMyTask(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);

            if (task.AcknowledgementRule == OnboardingTaskAcknowledgementRule.Required
                && task.AcknowledgedAt == null)
                throw new UserFriendlyException("Task must be acknowledged before it can be completed.");

            task.Status = JourneyTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedByUserId = AbpSession.UserId;
            var updated = await _taskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(updated);
        }

        /// <summary>Marks a manager-assigned JourneyTask as Complete.</summary>
        [HttpPost]
        public async Task<DynamicDto<JourneyTask, Guid>> CompleteManagerTask(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);
            task.Status = JourneyTaskStatus.Completed;
            task.CompletedAt = DateTime.UtcNow;
            task.CompletedByUserId = AbpSession.UserId;
            var updated = await _taskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(updated);
        }

        /// <summary>Updates the details of an existing JourneyTask. Facilitator only.</summary>
        [HttpPut]
        public async Task<DynamicDto<JourneyTask, Guid>> UpdateTask(UpdateJourneyTaskDto input)
        {
            var task = await _taskRepository.GetAsync(input.TaskId);

            if (input.Title != null) task.Title = input.Title;
            if (input.Description != null) task.Description = input.Description;
            if (input.Category.HasValue) task.Category = input.Category.Value;
            if (input.DueOn.HasValue) task.DueOn = input.DueOn.Value;
            if (input.AssignmentTarget.HasValue) task.AssignmentTarget = input.AssignmentTarget.Value;
            if (input.AcknowledgementRule.HasValue) task.AcknowledgementRule = input.AcknowledgementRule.Value;

            var updated = await _taskRepository.UpdateAsync(task);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(updated);
        }

        /// <summary>Adds a new ad-hoc task to an active journey. Facilitator only.</summary>
        [HttpPost]
        public async Task<DynamicDto<JourneyTask, Guid>> AddTask(AddJourneyTaskDto input)
        {
            var journey = await _journeyRepository.GetAsync(input.JourneyId);

            var task = new JourneyTask
            {
                JourneyId           = journey.Id,
                Title               = input.Title,
                Description         = input.Description,
                Category            = input.Category,
                DueOn               = input.DueOn,
                AssignmentTarget    = input.AssignmentTarget,
                AcknowledgementRule = input.AcknowledgementRule,
                ModuleTitle         = "Ad-hoc",
                Status              = JourneyTaskStatus.Pending
            };

            var saved = await _taskRepository.InsertAsync(task);
            return await MapToDynamicDtoAsync<JourneyTask, Guid>(saved);
        }

        /// <summary>Soft-deletes a Pending JourneyTask. Only Pending tasks may be removed.</summary>
        [HttpDelete]
        public async Task RemovePendingTask(Guid taskId)
        {
            var task = await _taskRepository.GetAsync(taskId);

            if (task.Status != JourneyTaskStatus.Pending)
                throw new UserFriendlyException("Only Pending tasks can be removed.");

            await _taskRepository.DeleteAsync(task);
        }

        /// <summary>
        /// Sends the hire's profile and plan context to Groq and returns AI personalisation suggestions.
        /// Marks affected JourneyTask records with PersonalisedAt after application.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<Journey, Guid>> RequestPersonalisation(Guid journeyId)
        {
            // TODO: delegate to GroqPersonalisationService
            throw new NotImplementedException("RequestPersonalisation: call GroqPersonalisationService, update JourneyTask.PersonalisedAt on affected tasks");
        }

        /// <summary>Applies the previously generated AI personalisation to the journey's tasks.</summary>
        [HttpPost]
        public async Task<DynamicDto<Journey, Guid>> ApplyPersonalisation(ApplyPersonalisationDto input)
        {
            // TODO: parse PersonalisationJson and update affected JourneyTask records
            throw new NotImplementedException("ApplyPersonalisation: parse and apply personalisation JSON to JourneyTasks");
        }

        /// <summary>
        /// Activates a Draft journey, making it visible to the hire in their portal.
        /// Also generates wellness check-ins for each milestone period.
        /// </summary>
        [HttpPost]
        public async Task<DynamicDto<Journey, Guid>> Activate(Guid journeyId)
        {
            var journey = await _hireJourneyManager.ActivateJourneyAsync(journeyId);
            await _wellnessManager.GenerateCheckInsForJourneyAsync(journey.HireId, journeyId);
            return await MapToDynamicDtoAsync<Journey, Guid>(journey);
        }
    }
}
