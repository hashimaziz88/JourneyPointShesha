using Abp.Domain.Repositories;
using Abp.Domain.Services;
using Abp.UI;
using Ardalis.GuardClauses;
using Hashim.JourneyPoint.Domain.Domain.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Hashim.JourneyPoint.Domain.Domain.OnboardingPlans
{
    /// <summary>
    /// Manages the OnboardingPlan lifecycle and structural operations.
    /// Handles deep-cloning of plans (with modules and tasks) and AI enhancement application.
    /// Published plans are immutable — clone first, then edit the copy.
    /// </summary>
    public class OnboardingPlanManager : DomainService
    {
        #region Dependencies

        private readonly IRepository<OnboardingPlan, Guid> _planRepository;
        private readonly IRepository<OnboardingModule, Guid> _moduleRepository;
        private readonly IRepository<OnboardingTask, Guid> _taskRepository;

        #endregion

        public OnboardingPlanManager(
            IRepository<OnboardingPlan, Guid> planRepository,
            IRepository<OnboardingModule, Guid> moduleRepository,
            IRepository<OnboardingTask, Guid> taskRepository)
        {
            _planRepository = planRepository;
            _moduleRepository = moduleRepository;
            _taskRepository = taskRepository;
        }

        #region Public Methods

        /// <summary>
        /// Creates a Draft copy of a plan with all of its modules and tasks.
        /// The clone's name is suffixed with " (Copy)". Status is always Draft.
        /// </summary>
        public async Task<OnboardingPlan> ClonePlanAsync(Guid sourcePlanId)
        {
            var source = await _planRepository.GetAsync(sourcePlanId);
            Guard.Against.Null(source, nameof(source));

            var clone = await InsertClonedPlanAsync(source);
            await CloneModulesAsync(sourcePlanId, clone);
            return clone;
        }

        /// <summary>
        /// Adds a new task to the last module of a plan.
        /// Used by OnboardingDocumentManager when applying accepted AI proposals.
        /// </summary>
        public async Task<OnboardingTask> AddTaskToLastModuleAsync(Guid planId, OnboardingTask task)
        {
            Guard.Against.Null(task, nameof(task));

            var plan = await _planRepository.GetAsync(planId);
            Guard.Against.Null(plan, nameof(plan));

            if (plan.Status == OnboardingPlanStatus.Published)
                throw new UserFriendlyException("Published plans cannot be modified. Clone the plan first.");

            var lastModule = await GetLastModuleAsync(planId);
            if (lastModule == null)
                throw new UserFriendlyException("The plan has no modules. Add at least one module before adding tasks.");

            var modules = await _moduleRepository.GetAllListAsync(m => m.OnboardingPlanId == planId);
            var maxOrderIndex = await GetMaxTaskOrderIndexAsync(lastModule.Id);

            task.OnboardingModuleId = lastModule.Id;
            task.OrderIndex = maxOrderIndex + 1;
            return await _taskRepository.InsertAsync(task);
        }

        #endregion

        #region Private Methods

        private async Task<OnboardingPlan> InsertClonedPlanAsync(OnboardingPlan source)
        {
            var clone = new OnboardingPlan
            {
                Name = $"{source.Name} (Copy)",
                Description = source.Description,
                TargetAudience = source.TargetAudience,
                DurationDays = source.DurationDays,
                Status = OnboardingPlanStatus.Draft,
                TenantId = source.TenantId
            };
            return await _planRepository.InsertAsync(clone);
        }

        private async Task CloneModulesAsync(Guid sourcePlanId, OnboardingPlan clone)
        {
            var modules = await _moduleRepository.GetAllListAsync(m => m.OnboardingPlanId == sourcePlanId);
            var orderedModules = modules.OrderBy(m => m.OrderIndex).ToList();

            foreach (var module in orderedModules)
            {
                var clonedModule = await InsertClonedModuleAsync(module, clone.Id, clone.TenantId);
                await CloneTasksAsync(module.Id, clonedModule);
            }
        }

        private async Task<OnboardingModule> InsertClonedModuleAsync(OnboardingModule source, Guid newPlanId, int tenantId)
        {
            var clone = new OnboardingModule
            {
                OnboardingPlanId = newPlanId,
                Name = source.Name,
                Description = source.Description,
                OrderIndex = source.OrderIndex,
                TenantId = tenantId
            };
            return await _moduleRepository.InsertAsync(clone);
        }

        private async Task CloneTasksAsync(Guid sourceModuleId, OnboardingModule newModule)
        {
            var tasks = await _taskRepository.GetAllListAsync(t => t.OnboardingModuleId == sourceModuleId);
            var orderedTasks = tasks.OrderBy(t => t.OrderIndex).ToList();

            foreach (var task in orderedTasks)
                await _taskRepository.InsertAsync(BuildClonedTask(task, newModule));
        }

        private OnboardingTask BuildClonedTask(OnboardingTask source, OnboardingModule newModule)
        {
            return new OnboardingTask
            {
                OnboardingModuleId = newModule.Id,
                TenantId = newModule.TenantId,
                Title = source.Title,
                Description = source.Description,
                Category = source.Category,
                OrderIndex = source.OrderIndex,
                DueDayOffset = source.DueDayOffset,
                AssignmentTarget = source.AssignmentTarget,
                AcknowledgementRule = source.AcknowledgementRule
            };
        }

        private async Task<OnboardingModule> GetLastModuleAsync(Guid planId)
        {
            var modules = await _moduleRepository.GetAllListAsync(m => m.OnboardingPlanId == planId);
            return modules.OrderByDescending(m => m.OrderIndex).FirstOrDefault();
        }

        private async Task<int> GetMaxTaskOrderIndexAsync(Guid moduleId)
        {
            var tasks = await _taskRepository.GetAllListAsync(t => t.OnboardingModuleId == moduleId);
            return tasks.Any() ? tasks.Max(t => t.OrderIndex) : 0;
        }

        #endregion
    }
}
