using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.UI;
using Ardalis.GuardClauses;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using Hashim.JourneyPoint.Domain.Domain.OnboardingPlans;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain.Hires
{
    /// <summary>
    /// Orchestrates the hire lifecycle and journey generation.
    /// Responsible for creating draft journeys by copying OnboardingTasks to JourneyTasks,
    /// and for all journey state transitions (Activate, Pause, Complete, Exit).
    /// No AI is involved — this service is deterministic.
    /// </summary>
    public class HireJourneyManager : DomainService
    {
        #region Dependencies

        private readonly IRepository<Hire, Guid> _hireRepository;
        private readonly IRepository<Journey, Guid> _journeyRepository;
        private readonly IRepository<JourneyTask, Guid> _journeyTaskRepository;
        private readonly IRepository<OnboardingPlan, Guid> _planRepository;
        private readonly IRepository<OnboardingModule, Guid> _moduleRepository;
        private readonly IRepository<OnboardingTask, Guid> _onboardingTaskRepository;

        #endregion

        public HireJourneyManager(
            IRepository<Hire, Guid> hireRepository,
            IRepository<Journey, Guid> journeyRepository,
            IRepository<JourneyTask, Guid> journeyTaskRepository,
            IRepository<OnboardingPlan, Guid> planRepository,
            IRepository<OnboardingModule, Guid> moduleRepository,
            IRepository<OnboardingTask, Guid> onboardingTaskRepository)
        {
            _hireRepository = hireRepository;
            _journeyRepository = journeyRepository;
            _journeyTaskRepository = journeyTaskRepository;
            _planRepository = planRepository;
            _moduleRepository = moduleRepository;
            _onboardingTaskRepository = onboardingTaskRepository;
        }

        #region Public Methods

        /// <summary>
        /// Generates a Draft journey for the given hire by copying all tasks from
        /// the hire's enrolled OnboardingPlan. The plan must be Published.
        /// </summary>
        public async Task<Journey> CreateDraftJourneyAsync(Guid hireId)
        {
            var hire = await _hireRepository.GetAsync(hireId);
            Guard.Against.Null(hire, nameof(hire));

            var plan = await _planRepository.GetAsync(hire.OnboardingPlanId);
            Guard.Against.Null(plan, nameof(plan));

            if (plan.Status != OnboardingPlanStatus.Published)
                throw new UserFriendlyException("Only Published plans can be used to generate journeys.");

            if (await HasExistingJourneyAsync(hireId))
                throw new UserFriendlyException("A journey already exists for this hire.");

            var journey = await InsertJourneyAsync(hire);
            await CopyTasksToJourneyAsync(hire, journey);
            return journey;
        }

        /// <summary>
        /// Activates a Draft journey, making it visible to the hire and setting the hire to Active.
        /// </summary>
        public async Task<Journey> ActivateJourneyAsync(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsync(journeyId);
            Guard.Against.Null(journey, nameof(journey));

            if (journey.Status != JourneyStatus.Draft)
                throw new UserFriendlyException("Only Draft journeys can be activated.");

            journey.Status = JourneyStatus.Active;
            journey.ActivatedAt = DateTime.UtcNow;
            var updated = await _journeyRepository.UpdateAsync(journey);

            await SetHireStatusAsync(journey.HireId, HireLifecycleState.Active, activatedAt: DateTime.UtcNow);
            return updated;
        }

        /// <summary>
        /// Pauses an Active journey. The hire's tasks remain intact for later resumption.
        /// </summary>
        public async Task<Journey> PauseJourneyAsync(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsync(journeyId);
            Guard.Against.Null(journey, nameof(journey));

            if (journey.Status != JourneyStatus.Active)
                throw new UserFriendlyException("Only Active journeys can be paused.");

            journey.Status = JourneyStatus.Paused;
            journey.PausedAt = DateTime.UtcNow;
            return await _journeyRepository.UpdateAsync(journey);
        }

        /// <summary>
        /// Marks a journey as Completed and updates the hire record with the completion date.
        /// </summary>
        public async Task<Journey> CompleteJourneyAsync(Guid journeyId)
        {
            var journey = await _journeyRepository.GetAsync(journeyId);
            Guard.Against.Null(journey, nameof(journey));

            if (journey.Status != JourneyStatus.Active)
                throw new UserFriendlyException("Only Active journeys can be completed.");

            journey.Status = JourneyStatus.Completed;
            journey.CompletedAt = DateTime.UtcNow;
            var updated = await _journeyRepository.UpdateAsync(journey);

            await SetHireStatusAsync(journey.HireId, HireLifecycleState.Completed, completedAt: DateTime.UtcNow);
            return updated;
        }

        /// <summary>
        /// Marks a hire as Exited. All Pending journey tasks are soft-deleted.
        /// </summary>
        public async Task ExitHireAsync(Guid hireId)
        {
            var hire = await _hireRepository.GetAsync(hireId);
            Guard.Against.Null(hire, nameof(hire));

            hire.Status = HireLifecycleState.Exited;
            hire.ExitedAt = DateTime.UtcNow;
            await _hireRepository.UpdateAsync(hire);

            await SoftDeletePendingTasksAsync(hireId);
        }

        #endregion

        #region Private Methods

        private async Task<bool> HasExistingJourneyAsync(Guid hireId)
        {
            var existing = await _journeyRepository.FirstOrDefaultAsync(j => j.HireId == hireId);
            return existing != null;
        }

        private async Task<Journey> InsertJourneyAsync(Hire hire)
        {
            var journey = new Journey
            {

                HireId = hire.Id,
                OnboardingPlanId = hire.OnboardingPlanId,
                Status = JourneyStatus.Draft
            };
            return await _journeyRepository.InsertAsync(journey);
        }

        private async Task CopyTasksToJourneyAsync(Hire hire, Journey journey)
        {
            var modules = await _moduleRepository.GetAllListAsync(m => m.OnboardingPlanId == hire.OnboardingPlanId);
            var orderedModules = modules.OrderBy(m => m.OrderIndex).ToList();

            foreach (var module in orderedModules)
            {
                var tasks = await _onboardingTaskRepository.GetAllListAsync(t => t.OnboardingModuleId == module.Id);
                var orderedTasks = tasks.OrderBy(t => t.OrderIndex).ToList();

                foreach (var task in orderedTasks)
                    await _journeyTaskRepository.InsertAsync(BuildJourneyTask(hire, journey, module, task));
            }
        }

        private JourneyTask BuildJourneyTask(Hire hire, Journey journey, OnboardingModule module, OnboardingTask task)
        {
            return new JourneyTask
            {
                JourneyId = journey.Id,
                SourceOnboardingTaskId = task.Id,
                SourceOnboardingModuleId = module.Id,
                ModuleTitle = module.Name,
                ModuleOrderIndex = module.OrderIndex,
                TaskOrderIndex = task.OrderIndex,
                Title = task.Title,
                Description = task.Description,
                Category = task.Category,
                AssignmentTarget = task.AssignmentTarget,
                AcknowledgementRule = task.AcknowledgementRule,
                DueDayOffset = task.DueDayOffset,
                DueOn = hire.StartDate.AddDays(task.DueDayOffset),
                Status = JourneyTaskStatus.Pending
            };
        }

        private async Task SetHireStatusAsync(
            Guid hireId,
            HireLifecycleState status,
            DateTime? activatedAt = null,
            DateTime? completedAt = null)
        {
            var hire = await _hireRepository.GetAsync(hireId);
            hire.Status = status;
            if (activatedAt.HasValue) hire.ActivatedAt = activatedAt;
            if (completedAt.HasValue) hire.CompletedAt = completedAt;
            await _hireRepository.UpdateAsync(hire);
        }

        private async Task SoftDeletePendingTasksAsync(Guid hireId)
        {
            var journey = await _journeyRepository.FirstOrDefaultAsync(j => j.HireId == hireId && j.Status == JourneyStatus.Active);
            if (journey == null) return;

            var pendingTasks = await _journeyTaskRepository.GetAllListAsync(
                t => t.JourneyId == journey.Id && t.Status == JourneyTaskStatus.Pending);

            foreach (var task in pendingTasks)
                await _journeyTaskRepository.DeleteAsync(task);
        }

        #endregion
    }
}
